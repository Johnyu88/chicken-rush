using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace ChickenRush
{
    /// <summary>雞窩只管理收納與退場；生命週期及得分由 GameManager 接手。</summary>
    public class NestController : MonoBehaviour
    {
        [SerializeField, Min(1)] private int capacity = 10;
        [SerializeField, Min(0.05f)] private float slideDuration = 0.7f;
        [Tooltip("完全離開鏡頭左側後再多滑出的世界距離。")]
        [SerializeField, Min(0f)] private float exitPadding = 0.5f;
        [SerializeField] private UnityEvent onChickenAccepted = new UnityEvent();
        [SerializeField] private UnityEvent onFilled = new UnityEvent();
        private GameManager gameManager;
        private Camera worldCamera;
        public int CurrentCount { get; private set; }
        public int Capacity => Mathf.Max(1, capacity);
        public bool IsFull => CurrentCount >= Capacity;
        public bool IsLeaving { get; private set; }

        public void Initialize(GameManager manager, Camera camera)
        {
            gameManager = manager;
            worldCamera = camera;
        }

        /// <summary>小雞切換 Nested 狀態後才計數，多個 Collider／Stay 回呼不會重複接收。</summary>
        public bool TryAcceptChicken(ChickenController chicken)
        {
            if (!isActiveAndEnabled || gameManager == null || !gameManager.IsPlaying ||
                gameManager.ActiveNest != this || IsLeaving || IsFull || chicken == null) return false;
            if (!chicken.TryEnterNest(transform)) return false;
            CurrentCount++;
            if (IsFull)
            {
                IsLeaving = true; // 先上鎖再派發事件，防止同一物理幀超收或重複完成。
                gameManager.BeginNestCompletion(this);
            }
            onChickenAccepted.Invoke();
            if (IsFull) onFilled.Invoke();
            return true;
        }

        /// <summary>由管理器執行 Coroutine。計入已收納小雞的顯示範圍，確保整窩離開鏡頭。</summary>
        public IEnumerator SlideOut()
        {
            Vector3 start = transform.position;
            float rightEdge = start.x;
            foreach (Renderer visual in GetComponentsInChildren<Renderer>())
                rightEdge = Mathf.Max(rightEdge, visual.bounds.max.x);
            foreach (Collider2D collider in GetComponentsInChildren<Collider2D>())
                rightEdge = Mathf.Max(rightEdge, collider.bounds.max.x);
            float leftEdge = worldCamera.ViewportToWorldPoint(new Vector3(0f, 0.5f,
                start.z - worldCamera.transform.position.z)).x;
            float endX = Mathf.Min(start.x, leftEdge - (rightEdge - start.x) - Mathf.Max(0f, exitPadding));
            Vector3 end = new Vector3(endX, start.y, start.z);
            float elapsed = 0f;
            float duration = Mathf.Max(0.05f, slideDuration);
            while (elapsed < duration)
            {
                yield return null;
                if (this == null || gameManager == null) yield break;
                if (!gameManager.IsPlaying) continue;
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.position = Vector3.Lerp(start, end, t * t * (3f - 2f * t));
            }
            transform.position = end;
        }
    }
}
