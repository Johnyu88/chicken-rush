using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
#endif

namespace ChickenRush
{
    /// <summary>
    /// 按住螢幕／滑鼠左鍵，以每秒 N 隻生成小雞；左右拖動只影響之後出生的小雞。
    /// 支援 Input System 與舊 Input Manager；Both 模式優先使用 Input System。
    /// </summary>
    public class ChickenSpawner : MonoBehaviour
    {
        [Header("必要引用")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private ChickenController chickenPrefab;

        [Header("按住生成")]
        [Tooltip("N：持續按住時每秒生成數；0 表示停止。按下時額外立即生成第一隻。")]
        [SerializeField, Range(0f, 100f)] private float chickensPerSecond = 5f;
        [SerializeField, Range(0f, 0.49f)] private float horizontalMargin = 0.1f;
        [SerializeField, Min(0f)] private float topOffset = 0.5f;
        [SerializeField] private float gameplayZ = 0f;

        [Header("拖動噴射與重力")]
        [Tooltip("從按下點左右拖動多少螢幕寬度，就達到最大水平初速。0.25 代表四分之一螢幕。")]
        [SerializeField, Range(0.01f, 1f)] private float dragWidthFraction = 0.25f;
        [Tooltip("水平初速上限，單位為世界單位／秒；小數值呈現輕微噴射偏角。")]
        [SerializeField, Min(0f)] private float maxHorizontalSpeed = 1.5f;
        [SerializeField, Min(0f)] private float initialDownwardSpeed = 0.5f;
        [SerializeField, Min(0.01f)] private float gravityScale = 1f;

        private readonly HoldSpawnInput holdInput = new HoldSpawnInput();
        private int activeFingerId = -1;
        private bool hasFocus = true;
        private bool applicationPaused;
        public float CurrentHorizontalSpeed => holdInput.HorizontalSpeed;
        private PhysicsMaterial2D difficultyMaterial;
        private bool waitForRelease;

        public void ConfigureDifficulty(PhysicsMaterial2D material)
        {
            difficultyMaterial = material;
            waitForRelease = true;
            ResetInput();
        }

        private void Start()
        {
            // 相機沿用 MVP 設定：正交、無旋轉、朝 +Z，遊戲平面通常在 Z=0。
            if (worldCamera == null) worldCamera = Camera.main;
            if (gameManager == null || chickenPrefab == null || worldCamera == null || !worldCamera.orthographic)
            {
                Debug.LogError("Spawner 需要 GameManager、小雞 Prefab 與正交相機。", this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (!hasFocus || applicationPaused || gameManager == null || !gameManager.IsPlaying || gameManager.IsChangingNest)
            {
                ResetInput();
                return;
            }
            Vector2 position;
            bool held = ReadPointer(out position);
            // A menu click/touch must be released before it can spawn a chicken.
            if (waitForRelease)
            {
                if (!held) waitForRelease = false;
                ResetInput();
                return;
            }
            int count = holdInput.Step(held, position.x / Mathf.Max(1, Screen.width), Time.deltaTime,
                Mathf.Clamp(chickensPerSecond, 0f, 100f), dragWidthFraction, maxHorizontalSpeed);
            // 保留小數額度：20 隻／秒而幀率只有 10 FPS 時，每幀可生成兩隻。
            // deltaTime 不包含暫停時間；Unity 的 maximumDeltaTime 限制長卡頓補量。
            for (int i = 0; i < count; i++) SpawnChicken();
        }

        /// <summary>鎖定本次按下的手指，其他手指不改變瞄準；觸控優先於滑鼠。</summary>
        private bool ReadPointer(out Vector2 position)
        {
            position = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            // EnhancedTouch 保留觸控歷程，適合在 Update 輪詢，避免遺失短促的觸控階段。
            foreach (var touch in EnhancedTouch.activeTouches)
            {
                if (activeFingerId >= 0 ? touch.touchId != activeFingerId :
                    touch.phase != UnityEngine.InputSystem.TouchPhase.Began) continue;
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                    touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled) continue;
                activeFingerId = touch.touchId;
                position = touch.screenPosition;
                return true;
            }
            if (activeFingerId >= 0) { activeFingerId = -1; return false; }
            if (EnhancedTouch.activeTouches.Count == 0 && Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                position = Mouse.current.position.ReadValue();
                return true;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER || !UNITY_2019_3_OR_NEWER
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (activeFingerId >= 0 ? touch.fingerId != activeFingerId : touch.phase != TouchPhase.Began) continue;
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) continue;
                activeFingerId = touch.fingerId;
                position = touch.position;
                return true;
            }
            if (activeFingerId >= 0) { activeFingerId = -1; return false; }
            // 手機的模擬滑鼠事件不得成為第二個生成來源。
            if (Input.touchCount == 0 && Input.GetMouseButton(0))
            {
                position = Input.mousePosition;
                return true;
            }
#endif
            return false;
        }

        private void ResetInput() { activeFingerId = -1; holdInput.Reset(); }
        private void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            EnhancedTouchSupport.Enable(); // Enable/Disable 成對呼叫，與其他使用者共享引用計數。
#endif
        }
        private void OnDisable()
        {
            ResetInput();
#if ENABLE_INPUT_SYSTEM
            EnhancedTouchSupport.Disable();
#endif
        }
        private void OnApplicationFocus(bool focused) { hasFocus = focused; ResetInput(); }
        private void OnApplicationPause(bool paused) { applicationPaused = paused; ResetInput(); }

        /// <summary>
        /// 沿用上緣隨機出生點，設定一次初速後交給 2D 重力；不影響已在空中的小雞。
        /// 此公開接口可供波次系統直接呼叫；正常玩家輸入由 Update 控制按住與頻率。
        /// </summary>
        public void SpawnChicken()
        {
            if (gameManager == null || !gameManager.IsPlaying || gameManager.IsChangingNest || worldCamera == null || chickenPrefab == null) return;
            float margin = Mathf.Clamp(horizontalMargin, 0f, 0.49f);
            Vector3 position = worldCamera.ViewportToWorldPoint(new Vector3(Random.Range(margin, 1f - margin),
                1f, gameplayZ - worldCamera.transform.position.z));
            position.y += Mathf.Max(0f, topOffset);
            position.z = gameplayZ;
            ChickenController chicken = Instantiate(chickenPrefab, position, Quaternion.identity);
            Rigidbody2D body = chicken.GetComponent<Rigidbody2D>();
            if (difficultyMaterial != null)
            {
                body.sharedMaterial = difficultyMaterial;
                foreach (var collider in chicken.GetComponents<Collider2D>())
                    collider.sharedMaterial = difficultyMaterial;
            }
            body.bodyType = RigidbodyType2D.Dynamic;
            body.simulated = true;
            body.gravityScale = Mathf.Max(0.01f, gravityScale);
            Vector2 velocity = new Vector2(holdInput.HorizontalSpeed, -Mathf.Max(0f, initialDownwardSpeed));
            // 只設定一次初速，讓不同質量的小雞具有相同偏速；後續由物理系統計算。
#if UNITY_6000_0_OR_NEWER
            body.linearVelocity = velocity;
#else
            body.velocity = velocity;
#endif
            chicken.Initialize(gameManager, Vector2.zero); // 保留原控制器接口，不額外疊加衝量。
        }
    }
}
