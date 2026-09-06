using UnityEngine;

namespace ChickenRush
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class ChickenController : MonoBehaviour
    {
        public enum ChickenState { Falling, Nested, KnockedAway }
        [Header("撞飛與清理")]
        [Tooltip("只有這些圖層的實體碰撞才算撞飛；雞窩收納使用 Trigger。")]
        [SerializeField] private LayerMask knockAwayLayers;
        [SerializeField, Min(0f)] private float knockAwayImpulse = 4f;
        [SerializeField, Min(0.1f)] private float knockedAwayLifetime = 3f;
        [SerializeField] private float despawnBelowY = -10f;
        private Rigidbody2D body;
        private GameManager gameManager;
        public ChickenState State { get; private set; } = ChickenState.Falling;
        public bool CanEnterNest => State == ChickenState.Falling && gameManager != null && gameManager.IsPlaying;

        private void Awake() { body = GetComponent<Rigidbody2D>(); }

        /// <summary>生成後注入管理器並施加一次衝量；Prefab 的 Rigidbody2D 應為 Dynamic。</summary>
        public void Initialize(GameManager manager, Vector2 initialImpulse)
        {
            gameManager = manager;
            body.AddForce(initialImpulse, ForceMode2D.Impulse);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!CanEnterNest) return;
            // Trigger 可以位於雞窩子物件；向父層尋找控制器。
            NestController nest = other.GetComponentInParent<NestController>();
            if (nest != null) nest.TryAcceptChicken(this);
        }

        /// <summary>雞窩先確認有容量後才可呼叫；狀態鎖防止多 Collider 重複計數。</summary>
        public bool TryEnterNest(Transform nest)
        {
            if (!CanEnterNest || nest == null) return false;
            State = ChickenState.Nested;
            body.simulated = false; // 收納後退出物理世界，避免反覆觸發或被撞出去。
            transform.SetParent(nest, true); // 保留進窩位置，之後隨雞窩一起滑走。
            return true;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!CanEnterNest || (knockAwayLayers.value & (1 << collision.gameObject.layer)) == 0) return;
            Vector2 direction = collision.contactCount > 0 ? collision.GetContact(0).normal : Vector2.up;
            KnockAway(direction);
        }

        public void KnockAway(Vector2 direction)
        {
            if (!CanEnterNest) return;
            State = ChickenState.KnockedAway;
            body.AddForce(direction.normalized * knockAwayImpulse, ForceMode2D.Impulse);
            gameManager.RegisterMiss();
            Destroy(gameObject, knockedAwayLifetime); // 使用遊戲時間；暫停時不倒數。
        }

        private void Update()
        {
            if (gameManager == null || !gameManager.IsPlaying || State == ChickenState.Nested) return;
            if (transform.position.y >= despawnBelowY) return;
            if (State == ChickenState.Falling) gameManager.RegisterMiss();
            Destroy(gameObject); // 撞飛的小雞不再重複回報失誤。
        }
    }
}
