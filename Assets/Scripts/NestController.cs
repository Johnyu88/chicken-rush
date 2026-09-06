using UnityEngine;
using UnityEngine.Events;

namespace ChickenRush
{
    /// <summary>接收小雞、封鎖已滿雞窩、滑出後生成下一個；事件只連接 UI／音效等表現。</summary>
    public class NestController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField, Min(1)] private int capacity = 3;
        [Header("切換設定")]
        [Tooltip("拖入 Project 中的雞窩 Prefab，可引用自己以形成相同雞窩的循環。")]
        [SerializeField] private NestController nextNestPrefab;
        [SerializeField, Min(0.01f)] private float slideSpeed = 5f;
        [Tooltip("世界座標；應設在鏡頭左界之外，並預留整個雞窩的寬度。")]
        [SerializeField] private float exitX = -12f;
        [Header("Inspector 事件")]
        [SerializeField] private UnityEvent onChickenAccepted = new UnityEvent();
        [SerializeField] private UnityEvent onFilled = new UnityEvent();
        [Tooltip("可連接場景上 AudioSource.Play，避免音源隨雞窩銷毀而中斷。")]
        [SerializeField] private UnityEvent onPlayFilledSound = new UnityEvent();
        private Vector3 initialPosition;
        private bool isLeaving;
        public int CurrentCount { get; private set; }
        public int Capacity => Mathf.Max(1, capacity);
        public bool IsFull => CurrentCount >= Capacity;

        private void Awake() { initialPosition = transform.position; }

        /// <summary>原子式接收：先確認雙方狀態，再鎖定小雞、增加數量，最後發布事件。</summary>
        public bool TryAcceptChicken(ChickenController chicken)
        {
            if (gameManager == null || !gameManager.IsPlaying || isLeaving || IsFull || chicken == null) return false;
            if (!chicken.TryEnterNest(transform)) return false;
            CurrentCount++;
            bool completedNow = IsFull;
            if (completedNow) isLeaving = true; // 在任何事件之前上鎖，防止重入重複得分。
            if (completedNow) gameManager.RegisterNestCompleted();
            onChickenAccepted.Invoke();
            if (completedNow)
            {
                onFilled.Invoke();
                onPlayFilledSound.Invoke();
            }
            return true;
        }

        private void Update()
        {
            if (!isLeaving || gameManager == null || !gameManager.IsPlaying) return;
            transform.position += Vector3.left * (Mathf.Max(0.01f, slideSpeed) * Time.deltaTime);
            if (transform.position.x > exitX) return;
            if (nextNestPrefab != null)
            {
                NestController next = Instantiate(nextNestPrefab, initialPosition, transform.rotation);
                next.gameManager = gameManager; // Prefab 不應持有場景物件引用，由舊雞窩注入。
            }
            else Debug.LogWarning("未指定下一個雞窩 Prefab；切換流程在此停止。", this);
            Destroy(gameObject); // 同時清理已收納的小雞子物件。
        }
    }
}
