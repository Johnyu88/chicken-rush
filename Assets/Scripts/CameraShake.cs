using UnityEngine;

namespace ChickenRush
{
    [DisallowMultipleComponent]
    public sealed class CameraShake : MonoBehaviour
    {
        [SerializeField, Min(0)] private float maximumMagnitude = .35f;
        private float remaining, duration, magnitude;
        private Vector3 offset;
        private readonly System.Random noise = new System.Random();
        public bool IsShaking => remaining > 0;
        public void Shake(float duration, float magnitude)
        {
            if (!isActiveAndEnabled || float.IsNaN(duration) || float.IsInfinity(duration) ||
                float.IsNaN(magnitude) || float.IsInfinity(magnitude) || duration <= 0 || magnitude <= 0) return;
            // Merge concurrent impacts instead of stacking coroutines or unbounded displacement.
            remaining = Mathf.Max(remaining, Mathf.Min(duration, 2f));
            this.duration = remaining;
            this.magnitude = Mathf.Max(this.magnitude, Mathf.Min(magnitude, maximumMagnitude));
        }
        private void LateUpdate()
        {
            transform.localPosition -= offset; offset = Vector3.zero;
            if (remaining <= 0 || Time.deltaTime <= 0) return;
            remaining = Mathf.Max(0, remaining - Time.deltaTime);
            if (remaining == 0) { magnitude = 0; return; }
            float angle = (float)noise.NextDouble() * Mathf.PI * 2;
            float radius = (float)noise.NextDouble() * magnitude * remaining / duration;
            offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            transform.localPosition += offset;
        }
        public void StopShake()
        {
            transform.localPosition -= offset; offset = Vector3.zero;
            remaining = duration = magnitude = 0;
        }
        private void OnDisable() { StopShake(); }
    }
}
