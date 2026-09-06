using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ChickenRush
{
    /// <summary>單一場景的遊戲資料中心；場景中只放一個，由 Inspector 明確注入其他元件。</summary>
    public class GameManager : MonoBehaviour
    {
        public enum GameState { Playing, GameOver, Paused, MainMenu }
        public enum RescueState { Available, Active, Used }

        [Header("計分設定")]
        [SerializeField, Min(1)] private int pointsPerNest = 100;
        [SerializeField, Min(1)] private int maxMultiplier = 5;
        [Header("滿窩金幣獎勵")]
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField, Min(0)] private int coinsPerNest = 10;
        [Header("雞窩生成：由管理器產生第一窩及後續雞窩")]
        [SerializeField] private NestController nestPrefab;
        [SerializeField] private Transform nestSpawnPoint;
        [SerializeField] private Camera worldCamera;
        [Header("成功音效：使用場景上的專用音源")]
        [SerializeField] private AudioSource successAudioSource;
        [SerializeField] private AudioClip successClip;
        [SerializeField, Min(0f)] private float pitchPerCombo = 0.1f;
        [System.Serializable] public class IntEvent : UnityEvent<int> { }
        [SerializeField] private IntEvent onComboChanged = new IntEvent();
        [Tooltip("满窩時計算好的獎勵，尚未入帳；可顯示 +100 等提示。")]
        [SerializeField] private IntEvent onNestRewardPrepared = new IntEvent();
        [Tooltip("退場銷毀後發布本次實際加分值。")]
        [SerializeField] private IntEvent onScoreAwarded = new IntEvent();
        [Header("事件：UI 可讀取下方唯讀屬性更新畫面")]
        [SerializeField] private UnityEvent onDataChanged = new UnityEvent();
        [SerializeField] private UnityEvent onRescueRequested = new UnityEvent();
        [SerializeField] private UnityEvent onFailure = new UnityEvent();
        [SerializeField] private UnityEvent onRescueCompleted = new UnityEvent();
        private readonly HashSet<ChickenController> chickens = new HashSet<ChickenController>();
        public float RescueSecondsRemaining { get; private set; }
        private bool restarting;
        [SerializeField] private bool waitForDifficulty;
        public float NestMoveSpeed { get; private set; }

        public void RegisterChicken(ChickenController chicken) { chickens.Add(chicken); }
        public void UnregisterChicken(ChickenController chicken) { chickens.Remove(chicken); }

        /// <summary>死區與底部保底偵測共用入口；同幀多隻死亡只觸發一次，保留失敗前 Combo。</summary>
        public void ReportChickenLost(ChickenController chicken)
        {
            if (!IsPlaying || chicken == null || !chickens.Contains(chicken) ||
                chicken.State == ChickenController.ChickenState.Nested) return;
            EndGame();
            onFailure.Invoke();
        }

        public GameState State { get; private set; } = GameState.Playing;
        public RescueState Rescue { get; private set; } = RescueState.Available;
        private readonly NestScoreState scoring = new NestScoreState();
        public int Score => scoring.Score;
        public int Combo => scoring.Combo;
        public NestController ActiveNest { get; private set; }
        public bool IsChangingNest { get; private set; }
        public bool IsPlaying => State == GameState.Playing;

        // MVP 假設本元件獨占 Time.timeScale；正式整合其他系統時改由統一的暫停服務管理。
        private void Awake()
        {
            Time.timeScale = 1f;
            if (waitForDifficulty) State = GameState.MainMenu;
        }
        private void OnDestroy() { Time.timeScale = 1f; }

        private void Start()
        {
            if (worldCamera == null) worldCamera = Camera.main;
            if (nestPrefab == null || nestSpawnPoint == null || worldCamera == null || !worldCamera.orthographic)
            {
                Debug.LogError("GameManager 需要雞窩 Prefab、生成點與正交相機。", this);
                EndGame();
                return;
            }
            if (!waitForDifficulty && ActiveNest == null) SpawnNextNest();
        }

        public bool StartGame(GameDifficultySettings settings)
        {
            if (State != GameState.MainMenu || ActiveNest != null) return false;
            if (worldCamera == null) worldCamera = Camera.main;
            if (nestPrefab == null || nestSpawnPoint == null || worldCamera == null || !worldCamera.orthographic)
                return false;
            NestMoveSpeed = settings.NestSpeed;
            SpawnNextNest();
            SetState(GameState.Playing);
            return true;
        }

        private void SpawnNextNest()
        {
            ActiveNest = Instantiate(nestPrefab, nestSpawnPoint.position, nestSpawnPoint.rotation);
            ActiveNest.Initialize(this, worldCamera);
        }

        /// <summary>只有目前的滿窩可開始結算。鎖定獎勵，更新 Combo，再進行退場。</summary>
        public void BeginNestCompletion(NestController nest)
        {
            if (!IsPlaying || IsChangingNest || nest == null || nest != ActiveNest || !nest.IsFull) return;
            if (!scoring.Begin(pointsPerNest, maxMultiplier)) return;
            IsChangingNest = true;
            if (successAudioSource != null && successClip != null)
            {
                successAudioSource.pitch = scoring.Pitch(pitchPerCombo);
                successAudioSource.PlayOneShot(successClip);
            }
            StartCoroutine(FinishNest(nest));
            onComboChanged.Invoke(Combo);
            onNestRewardPrepared.Invoke(scoring.PendingPoints);
            onDataChanged.Invoke();
        }

        private IEnumerator FinishNest(NestController nest)
        {
            yield return nest.SlideOut();
            if (nest != null) Destroy(nest.gameObject);
            // Destroy 延至幀末生效；下一幀才入帳並生成新窩，Coroutine 不掛在即將銷毀的物件。
            yield return null;
            while (!IsPlaying) yield return null;
            ActiveNest = null;
            int awarded = scoring.Settle();
            if (awarded > 0 && inventoryManager != null && coinsPerNest > 0)
                inventoryManager.AddCoins(coinsPerNest);
            SpawnNextNest();
            IsChangingNest = false;
            onScoreAwarded.Invoke(awarded);
            onDataChanged.Invoke();
        }

        /// <summary>撞飛或漏接會中斷 Combo；此骨架不自動結束遊戲，失敗規則由外部呼叫 EndGame。</summary>
        public void RegisterMiss()
        {
            if (!IsPlaying || IsChangingNest) return; // 換窩空窗漏接不破壞已完成的連擊。
            scoring.Miss();
            onComboChanged.Invoke(Combo);
            onDataChanged.Invoke();
        }

        public void Pause() { if (IsPlaying) SetState(GameState.Paused); }
        public void Resume() { if (State == GameState.Paused) SetState(GameState.Playing); }
        public void EndGame() { if (State != GameState.GameOver) SetState(GameState.GameOver); }

        private void SetState(GameState state)
        {
            State = state;
            Time.timeScale = IsPlaying ? 1f : 0f; // 同時停止物理、生成倒數與雞窩滑動。
            onDataChanged.Invoke();
        }

        /// <summary>每局一次 Mock 廣告救援；倒數由管理器持有，UI 關閉不會取消流程。</summary>
        public void RequestRescue()
        {
            if (State != GameState.GameOver || Rescue != RescueState.Available) return;
            Rescue = RescueState.Active;
            RescueSecondsRemaining = 5f;
            StartCoroutine(WatchMockAd());
            onDataChanged.Invoke();
            onRescueRequested.Invoke();
        }

        private IEnumerator WatchMockAd()
        {
            // timeScale 為 0 仍會逐幀執行，unscaledDeltaTime 不受暫停影響。
            while (RescueSecondsRemaining > 0f)
            {
                yield return null;
                RescueSecondsRemaining = Mathf.Max(0f, RescueSecondsRemaining - Time.unscaledDeltaTime);
            }
            // 母雞掃場 Mock：只清未收納的小雞，保留當前雞窩內容與未入帳獎勵。
            foreach (ChickenController chicken in new List<ChickenController>(chickens))
            {
                if (chicken == null || chicken.State == ChickenController.ChickenState.Nested) continue;
                chicken.gameObject.SetActive(false); // Destroy 前先停用，恢復物理時不再觸發死亡。
                Destroy(chicken.gameObject);
            }
            yield return null; // 等待幀末銷毀完成再恢復。
            Rescue = RescueState.Used;
            SetState(GameState.Playing); // 不呼叫 scoring.Miss，保留 Score、Combo 與待結算獎勵。
            onRescueCompleted.Invoke();
        }

        /// <summary>重新載入目前場景，重建分數、Combo、救援額度、雞窩與生成器。</summary>
        public void RestartGame()
        {
            if (restarting || Rescue == RescueState.Active) return;
            Scene scene = SceneManager.GetActiveScene();
            if (scene.buildIndex < 0)
            {
                Debug.LogError("請先儲存場景並加入 Build Settings／Build Profiles 的場景清單。", this);
                return;
            }
            restarting = true;
            StopAllCoroutines();
            Time.timeScale = 1f;
            SceneManager.LoadScene(scene.buildIndex);
        }
    }
}
