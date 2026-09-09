using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenRush
{
    public sealed class MyNestCanvas : MonoBehaviour
    {
        private InventoryManager inventory;
        private Text summary, selection, message;

        public FurnitureInventoryUI FurnitureLibrary { get; private set; }
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
            RuntimeUI.Label("ButlerGreeting",transform,font,"🐧 歡迎回家！要我幫忙整理雞窩嗎？",23,new Vector2(680,45),new Vector2(0,370));
            var viewport=RuntimeUI.Rect("NestViewport",transform,new Vector2(680,310),new Vector2(0,165)).gameObject.AddComponent<RawImage>();
            Prototype=gameObject.AddComponent<HomeNestPrototype>(); Prototype.Initialize(owner,viewport);
            viewport.gameObject.AddComponent<HomeNestPointerInput>().Home=Prototype;
            Prototype.Changed+=RefreshSelection;
            var library=RuntimeUI.Rect("FurnitureInventory",transform,new Vector2(560,70),new Vector2(0,-95));
            FurnitureLibrary=library.gameObject.AddComponent<FurnitureInventoryUI>(); FurnitureLibrary.Initialize(Prototype,font);
            AddButton("OrbitLeft","視角 ←",-255,-25,()=>Prototype.Orbit(-15));
            AddButton("OrbitRight","視角 →",-85,-25,()=>Prototype.Orbit(15));
            AddButton("ZoomIn","放大",85,-25,()=>Prototype.Zoom(-1));
            AddButton("ZoomOut","縮小",255,-25,()=>Prototype.Zoom(1));
            selection=RuntimeUI.Label("Selection",transform,font,"",23,new Vector2(680,45),new Vector2(0,-155));
            AddButton("PreviousFurniture","上一頁",-255,-210,()=>FurnitureLibrary.ChangePage(-1));
            AddButton("NextFurniture","下一頁",-85,-210,()=>FurnitureLibrary.ChangePage(1));
            AddButton("RotateFurniture","旋轉 45°",85,-210,()=>Prototype.RotateSelected(45));
            AddButton("SaveFurniture","✓ Confirm",255,-210,()=>{ message.text=Prototype.SaveSelected()?"變更已確認儲存！":"請先選擇已擁有家具；無法儲存時請重新進入。"; });
            AddButton("RemoveFurniture","收回",-255,-280,()=>Prototype.RemoveSelected());
            AddButton("UndoFurniture","Undo",-85,-280,()=>Prototype.UndoEdit());
            AddButton("CancelFurniture","✕ Cancel",85,-280,()=>Prototype.CancelEdit());
            AddButton("PenguinPreview","企鵝方案",255,-280,ShowPenguinPreview);
            message=RuntimeUI.Label("Status",transform,font,"",21,new Vector2(680,40),new Vector2(0,-335));
            summary=RuntimeUI.Label("AssetSummary",transform,font,"",20,new Vector2(680,65),new Vector2(0,-385));
            RuntimeUI.Button("BackButton",transform,font,"返回主選單",new Vector2(420,55),new Vector2(0,-445)).onClick.AddListener(Close);
            void AddButton(string name,string caption,float x,float y,UnityEngine.Events.UnityAction action)
            { RuntimeUI.Button(name,transform,font,caption,new Vector2(155,60),new Vector2(x,y)).onClick.AddListener(action); }
        }
        public void Open()
        {
            gameObject.SetActive(true); Prototype.Enter();
            message.text="拖家具移動／空白轉視角；只有 Confirm 會儲存。";
            foreach(var item in Prototype.OwnedFurniture) if(Prototype.IsFurnitureSaved(item.itemId)) { Prototype.SelectFurniture(item.itemId); break; }
            Refresh();
        }
        public void Close() { gameObject.SetActive(false); }
        private void OnEnable() { if(inventory!=null) inventory.Changed+=Refresh; Refresh(); }
        private void OnDisable() { if(inventory!=null) inventory.Changed-=Refresh; }
        private void RefreshSelection()
        {
            FurnitureLibrary?.Refresh();
            if(message!=null) message.text=Prototype.HasUnsavedChanges ? "預覽中：Confirm 儲存，Cancel 放棄。" : "無待確認變更；拖家具移動／空白轉視角。";
            if(selection==null) return;
            selection.text=Prototype.HasUnsavedChanges ? "方案預覽中 · 尚未確認" : "請從家具庫選擇";
            var draft=Prototype.Draft;
            foreach(var item in Prototype.OwnedFurniture)
                if(draft!=null && item.itemId==draft.itemId) { selection.text=item.itemName+(Prototype.HasUnsavedChanges?" · 未儲存":" · 已儲存")+"（拖曳移動）"; return; }
        }
        private void ShowPenguinPreview()
        {
            var item=Prototype.OwnedFurniture.Count>0?Prototype.OwnedFurniture[0]:null;
            if(item==null) { message.text="🐧 要我幫忙整理雞窩嗎？先擁有一件家具吧！"; return; }
            bool exists=Prototype.PreviewState.placedFurniture.Exists(x=>x.itemId==item.itemId);
            bool accepted=Prototype.AgentPreview.Preview(new[]{new PlacementCommand {
                operation=exists?PlacementOperation.MoveFurniture:PlacementOperation.PlaceFurniture,
                itemId=item.itemId, position=new Vector3(-1.5f,0,0) }});
            message.text=accepted?"🐧 整理方案預覽（規則示範）：請 Confirm 或 Cancel。":"🐧 方案未通過驗證，請取消後重試。";
        }
        private void Refresh()
        {
            if(inventory==null || summary==null) return;
            FurnitureLibrary?.Refresh();
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
