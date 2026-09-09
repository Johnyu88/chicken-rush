using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenRush
{
    // Paged, owned-only library. Cards show catalog icons and placement state, not a second inventory.
    public sealed class FurnitureInventoryUI : MonoBehaviour
    {
        private HomeNestPrototype home;
        private Font font;
        private int page;
        private string lastSignature;
        private readonly List<GameObject> cards=new List<GameObject>();
        public IReadOnlyList<string> VisibleItemIds { get; private set; } = new List<string>().AsReadOnly();
        public void Initialize(HomeNestPrototype owner,Font textFont) { home=owner; font=textFont; }
        private string Status(string id)
        {
            bool preview=home.PreviewState?.placedFurniture.Any(x=>x.itemId==id)==true;
            bool saved=home.IsFurnitureSaved(id);
            return preview ? (saved?"已擺放":"預覽中") : (saved?"待收回":"可擺放");
        }
        public void ChangePage(int delta) { page+=delta; Refresh(); }
        public void Refresh()
        {
            if(home==null) return;

            var items=home.OwnedFurniture; page=Mathf.Clamp(page,0,Mathf.Max(0,(items.Count-1)/2));
            var visible=items.Skip(page*2).Take(2).ToList(); VisibleItemIds=visible.Select(x=>x.itemId).ToList().AsReadOnly();
            string signature=page+":"+string.Join("|",visible.Select(x=>x.itemId+":"+Status(x.itemId)));
            if(signature==lastSignature) return;
            lastSignature=signature;
            foreach(var card in cards) { card.SetActive(false); Destroy(card); } cards.Clear();
            foreach(var item in visible)
            {
                int index=cards.Count;
                var button=RuntimeUI.Button("Furniture:"+item.itemId,transform,font,"",new Vector2(260,70),new Vector2(index==0?-140:140,0));
                bool placed=home.PreviewState?.placedFurniture.Any(x=>x.itemId==item.itemId)==true;
                var label=button.transform.Find("Label").GetComponent<Text>(); label.text=item.itemName+"\n"+Status(item.itemId); label.fontSize=21;
                label.rectTransform.sizeDelta=new Vector2(195,70); label.rectTransform.anchoredPosition=new Vector2(30,0);
                var icon=RuntimeUI.Rect("Icon",button.transform,new Vector2(48,48),new Vector2(-100,0)).gameObject.AddComponent<Image>();
                icon.sprite=item.icon; icon.preserveAspect=true; icon.raycastTarget=false;
                if(item.icon==null) icon.color=new Color(.6f,.8f,.65f);
                button.onClick.AddListener(()=>home.SelectFurniture(item.itemId)); cards.Add(button.gameObject);
            }
            if(items.Count==0)
            {
                var empty=RuntimeUI.Label("Empty",transform,font,"尚未擁有家具",24,new Vector2(540,60),Vector2.zero); cards.Add(empty.gameObject);
            }
        }
    }
}
