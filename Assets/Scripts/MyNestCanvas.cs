using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenRush
{
    public sealed class MyNestCanvas : MonoBehaviour
    {
        private InventoryManager inventory;
        private Text summary, selection, message;
        private int selectedIndex=-1;
        public HomeNestPrototype Prototype { get; private set; }
        public string Summary => summary != null ? summary.text : "";
        public void Initialize(InventoryManager owner, Font font)
        {
            inventory=owner;
            var canvas=gameObject.AddComponent<Canvas>(); canvas.overrideSorting=true; canvas.sortingOrder=110;
            gameObject.AddComponent<GraphicRaycaster>();
            var rect=(RectTransform)transform; rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one; rect.offsetMin=rect.offsetMax=Vector2.zero;
            gameObject.AddComponent<Image>().color=new Color(.045f,.09f,.14f);
            RuntimeUI.Label("Title",transform,font,"🏡 我的雞窩",34,new Vector2(680,55),new Vector2(0,420));
            RuntimeUI.Label("ButlerGreeting",transform,font,"🐧 歡迎回家！選家具，再點地板擺放。",23,new Vector2(680,45),new Vector2(0,370));
            var viewport=RuntimeUI.Rect("NestViewport",transform,new Vector2(680,430),new Vector2(0,120)).gameObject.AddComponent<RawImage>();
            Prototype=gameObject.AddComponent<HomeNestPrototype>(); Prototype.Initialize(owner,viewport);
            viewport.gameObject.AddComponent<HomeNestPointerInput>().Home=Prototype;
            Prototype.Changed+=RefreshSelection;
            AddButton("OrbitLeft","視角 ←",-255,-135,()=>Prototype.Orbit(-15));
            AddButton("OrbitRight","視角 →",-85,-135,()=>Prototype.Orbit(15));
            AddButton("ZoomIn","放大",85,-135,()=>Prototype.Zoom(-1));
            AddButton("ZoomOut","縮小",255,-135,()=>Prototype.Zoom(1));
            selection=RuntimeUI.Label("Selection",transform,font,"",23,new Vector2(680,45),new Vector2(0,-195));
            AddButton("PreviousFurniture","上一件",-255,-250,()=>SelectNext(-1));
            AddButton("NextFurniture","下一件",-85,-250,()=>SelectNext(1));
            AddButton("RotateFurniture","旋轉 45°",85,-250,()=>Prototype.RotateSelected(45));
            AddButton("SaveFurniture","儲存",255,-250,()=>{ message.text=Prototype.SaveSelected()?"家具已儲存！":"請先選擇已擁有家具；無法儲存時請重新進入。"; });
            message=RuntimeUI.Label("Status",transform,font,"",21,new Vector2(680,40),new Vector2(0,-302));
            summary=RuntimeUI.Label("AssetSummary",transform,font,"",20,new Vector2(680,65),new Vector2(0,-353));
            RuntimeUI.Button("BackButton",transform,font,"返回主選單",new Vector2(420,60),new Vector2(0,-425)).onClick.AddListener(Close);
            void AddButton(string name,string caption,float x,float y,UnityEngine.Events.UnityAction action)
            { RuntimeUI.Button(name,transform,font,caption,new Vector2(155,60),new Vector2(x,y)).onClick.AddListener(action); }
        }
        public void Open()
        {
            gameObject.SetActive(true); Prototype.Enter(); selectedIndex=-1;
            message.text="點擊／拖曳地板移動家具；離開會放棄未儲存變更。"; SelectNext(1); Refresh();
        }
        public void Close() { gameObject.SetActive(false); }
        private void OnEnable() { if(inventory!=null) inventory.Changed+=Refresh; Refresh(); }
        private void OnDisable() { if(inventory!=null) inventory.Changed-=Refresh; }
        private void SelectNext(int direction)
        {
            var items=Prototype.OwnedFurniture;
            if(items.Count==0) { selection.text="尚未擁有家具；擁有後即可在這裡擺放。"; return; }
            selectedIndex=(selectedIndex+direction+items.Count)%items.Count;
            if(!Prototype.SelectFurniture(items[selectedIndex].itemId)) message.text="無法選取：請檢查所有權或家具數量上限。";
        }
        private void RefreshSelection()
        {
            if(selection==null) return;
            var draft=Prototype.Draft;
            foreach(var item in Prototype.OwnedFurniture)
                if(draft!=null && item.itemId==draft.itemId) { selection.text=item.itemName+(Prototype.HasUnsavedChanges?" · 未儲存":" · 已儲存")+"（點地板移動）"; return; }
        }
        private void Refresh()
        {
            if(inventory==null || summary==null) return;
            int costumes=0,furniture=0,butlers=0; var seen=new HashSet<string>();
            foreach(var item in Resources.LoadAll<ItemDataSO>("Items"))
            {
                if(item==null || !item.IsValid || !inventory.OwnsItem(item) || !seen.Add(item.itemId)) continue;
                if(item.itemType==ItemType.Costume) costumes++; else if(item.itemType==ItemType.Furniture) furniture++; else if(item.itemType==ItemType.Butler) butlers++;
            }
            var butler=inventory.GetActiveHomeButler();
            summary.text="飾品 "+costumes+"　家具 "+furniture+"　管家 "+butlers+"　已放置 "+inventory.GetActiveHomeFurniture().Count+"\n啟用管家："+(butler==null?"基礎企鵝嚮導":butler.itemName);
        }
    }
}
