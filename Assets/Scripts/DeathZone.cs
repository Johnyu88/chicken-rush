using UnityEngine;

namespace ChickenRush
{
    /// <summary>掛在畫面下方／左右界外的 BoxCollider2D，與小雞 Layer 開啟碰撞。</summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class DeathZone : MonoBehaviour
    {
        private void Reset() { GetComponent<BoxCollider2D>().isTrigger = true; }
        private void Awake() { GetComponent<BoxCollider2D>().isTrigger = true; }
        private void OnTriggerEnter2D(Collider2D other) { Detect(other); }
        private void OnTriggerStay2D(Collider2D other) { Detect(other); }
        private void Detect(Collider2D other)
        {
            if (!isActiveAndEnabled || other.attachedRigidbody == null) return;
            ChickenController chicken = other.attachedRigidbody.GetComponent<ChickenController>();
            if (chicken != null) chicken.ReportLost();
        }
    }
}
