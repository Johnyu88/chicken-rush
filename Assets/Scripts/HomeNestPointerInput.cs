using UnityEngine;
using UnityEngine.EventSystems;

namespace ChickenRush
{
    // Pointer events work with mouse and touch EventSystems; no keyboard dependency.
    public sealed class HomeNestPointerInput : MonoBehaviour, IPointerDownHandler, IDragHandler, IScrollHandler
    {
        public HomeNestPrototype Home { private get; set; }
        public void OnPointerDown(PointerEventData e) { Place(e); }
        public void OnDrag(PointerEventData e) { Place(e); }
        public void OnScroll(PointerEventData e) { Home?.Zoom(-e.scrollDelta.y*.4f); }
        private void Place(PointerEventData e)
        {
            var rect=(RectTransform)transform;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,e.position,e.pressEventCamera,out var point))
                Home?.PlaceAtViewport(new Vector2((point.x-rect.rect.xMin)/rect.rect.width,(point.y-rect.rect.yMin)/rect.rect.height));
        }
    }
}
