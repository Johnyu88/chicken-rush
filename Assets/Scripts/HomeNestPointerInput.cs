using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ChickenRush
{
    // All coordinates are viewport-normalized. EventSystem pointer IDs support mouse and touch.
    public sealed class HomeNestPointerInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IScrollHandler
    {
        public HomeNestPrototype Home { private get; set; }
        private readonly Dictionary<int,Vector2> pointers=new Dictionary<int,Vector2>();
        private bool moving, pinched;
        public void BeginPointer(int id,Vector2 uv)
        {
            if(!Valid(uv) || pointers.ContainsKey(id) || pointers.Count>=2) return;
            pointers[id]=uv;
            if(pointers.Count==2) { pinched=true; return; }
            string selected=Home?.PickFurniture(uv);
            moving=selected!=null && Home.SelectFurniture(selected);
        }
        public void MovePointer(int id,Vector2 uv)
        {
            if(!Valid(uv) || !pointers.TryGetValue(id,out var old)) return;
            if(pointers.Count==2)
            {
                var other=pointers.First(x=>x.Key!=id).Value;
                Home?.Zoom(-(Vector2.Distance(uv,other)-Vector2.Distance(old,other))*12);
            }
            else if(!pinched)
            {
                if(moving) Home?.PlaceAtViewport(uv);
                else Home?.Orbit((uv.x-old.x)*120);
            }
            pointers[id]=uv;
        }
        public void EndPointer(int id) { pointers.Remove(id); if(pointers.Count==0) { moving=false; pinched=false; } }
        private void OnDisable() { pointers.Clear(); moving=false; pinched=false; }
        private static bool Valid(Vector2 uv) => PlacementRules.Finite(uv.x) && PlacementRules.Finite(uv.y);
        private Vector2 UV(PointerEventData e)
        {
            var rect=(RectTransform)transform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,e.position,e.pressEventCamera,out var p);
            return new Vector2((p.x-rect.rect.xMin)/rect.rect.width,(p.y-rect.rect.yMin)/rect.rect.height);
        }
        public void OnPointerDown(PointerEventData e) => BeginPointer(e.pointerId,UV(e));
        public void OnPointerUp(PointerEventData e) => EndPointer(e.pointerId);
        public void OnDrag(PointerEventData e) => MovePointer(e.pointerId,UV(e));
        public void OnScroll(PointerEventData e) { Home?.Zoom(-e.scrollDelta.y*.4f); }
    }
}
