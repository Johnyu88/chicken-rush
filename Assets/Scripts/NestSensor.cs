using UnityEngine;

namespace ChickenRush
{
    /// <summary>掛在雞窩內部的 Trigger 子物件上；外框碰撞不等於進窩。</summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class NestSensor : MonoBehaviour
    {
        [SerializeField] private NestController nest;
        private void Reset() { GetComponent<BoxCollider2D>().isTrigger = true; }
        private void Awake()
        {
            if (nest == null) nest = GetComponentInParent<NestController>();
            GetComponent<BoxCollider2D>().isTrigger = true;
            if (nest == null) Debug.LogError("NestSensor 必須指定雞窩控制器。", this);
        }

        private void OnTriggerEnter2D(Collider2D other) { Detect(other); }
        // 暫停時剛好重疊的物件，恢復後仍有機會被接收；小雞狀態防止重複入窩。
        private void OnTriggerStay2D(Collider2D other) { Detect(other); }
        private void Detect(Collider2D other)
        {
            if (!isActiveAndEnabled || nest == null || other.attachedRigidbody == null) return;
            ChickenController chicken = other.attachedRigidbody.GetComponent<ChickenController>();
            if (chicken != null) nest.TryAcceptChicken(chicken);
        }
    }
}
