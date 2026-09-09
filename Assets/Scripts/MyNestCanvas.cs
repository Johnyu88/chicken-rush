using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenRush
{
    public sealed class MyNestCanvas : MonoBehaviour
    {
        private InventoryManager inventory;
        private Text summary;
        public string Summary => summary != null ? summary.text : "";
        public void Initialize(InventoryManager owner, Font font)
        {
            inventory = owner;
            var canvas = gameObject.AddComponent<Canvas>(); canvas.overrideSorting = true; canvas.sortingOrder = 110;
            gameObject.AddComponent<GraphicRaycaster>();
            var rect = (RectTransform)transform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            gameObject.AddComponent<Image>().color = new Color(.045f, .09f, .14f);
            RuntimeUI.Label("Title", transform, font, "🏡 我的雞窩", 38, new Vector2(650, 80), new Vector2(0, 335));
            RuntimeUI.Label("Description", transform, font, "個人雞窩 · 資料基礎版\n3D 家園與家具擺放尚未開放", 25, new Vector2(650, 100), new Vector2(0, 225));
            summary = RuntimeUI.Label("AssetSummary", transform, font, "", 28, new Vector2(650, 320), new Vector2(0, -10));
            RuntimeUI.Button("BackButton", transform, font, "返回主選單", new Vector2(420, 65), new Vector2(0, -335)).onClick.AddListener(Close);
        }
        public void Open() { gameObject.SetActive(true); Refresh(); }
        public void Close() { gameObject.SetActive(false); }
        private void OnEnable() { if (inventory != null) inventory.Changed += Refresh; Refresh(); }
        private void OnDisable() { if (inventory != null) inventory.Changed -= Refresh; }
        private void Refresh()
        {
            if (inventory == null || summary == null) return;
            int costumes = 0, furniture = 0, butlers = 0; var seen = new HashSet<string>();
            foreach (var item in Resources.LoadAll<ItemDataSO>("Items"))
            {
                if (item == null || !item.IsValid || !inventory.OwnsItem(item) || !seen.Add(item.itemId)) continue;
                if (item.itemType == ItemType.Costume) costumes++;
                else if (item.itemType == ItemType.Furniture) furniture++;
                else if (item.itemType == ItemType.Butler) butlers++;
            }
            var home = inventory.GetSnapshot().homeNest; var activeButler = inventory.GetActiveHomeButler();
            summary.text = "已擁有的家園相關資產\n\n飾品 " + costumes + "　家具 " + furniture + "　管家 " + butlers +
                "\n\n家園資料版本：" + home.version + "\n有效家具紀錄：" + inventory.GetActiveHomeFurniture().Count +
                "\n啟用管家：" + (activeButler == null ? "未啟用" : activeButler.itemName);
        }
    }
}
