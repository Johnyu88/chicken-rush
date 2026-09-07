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
            gameManager.RegisterChicken(this);
            body.AddForce(initialImpulse, ForceMode2D.Impulse);
        }

        // 進窩由 NestSensor 統一偵測，避免碰到雞窩其他 Trigger 就誤算收納。

        /// <summary>雞窩先確認有容量後才可呼叫；狀態鎖防止多 Collider 重複計數。</summary>
        public bool TryEnterNest(Transform nest)
        {
            if (!CanEnterNest || nest == null) return false;
            State = ChickenState.Nested;
            var anger = GetComponent<ChickenAnger>();
            if (anger != null) anger.StopFeedback();
            body.simulated = false; // 收納後退出物理世界，避免反覆觸發或被撞出去。
            transform.SetParent(nest, true); // 保留進窩位置，之後隨雞窩一起滑走。
            return true;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (CanEnterNest && collision.relativeVelocity.sqrMagnitude > 36f)
                gameManager.ShakeCamera(.12f, .08f);
            if (!CanEnterNest || (knockAwayLayers.value & (1 << collision.gameObject.layer)) == 0) return;
            Vector2 direction = collision.contactCount > 0 ? collision.GetContact(0).normal : Vector2.up;
            KnockAway(direction);
        }

        public void KnockAway(Vector2 direction)
        {
            if (!CanEnterNest) return;
            State = ChickenState.KnockedAway;
            body.AddForce(direction.normalized * knockAwayImpulse, ForceMode2D.Impulse);
            // 撞飛先保留 Combo；實際進死區才失敗，救援可保留失敗前的連擊。
            // 不再提前銷毀，以免越界小雞在碰到死區前就消失。
        }

        private void Update()
        {
            if (gameManager == null || !gameManager.IsPlaying || State == ChickenState.Nested) return;
            if (transform.position.y >= despawnBelowY) return;
            ReportLost(); // 即使死區漏放，底部界線也會觸發相同失敗流程。
        }

        public void ReportLost()
        {
            if (gameManager != null) gameManager.ReportChickenLost(this);
        }

        private void OnDestroy()
        {
            if (gameManager != null) gameManager.UnregisterChicken(this);
        }
    }
}
