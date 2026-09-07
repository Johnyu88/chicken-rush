using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenRush
{
    public sealed class ShopCanvas : MonoBehaviour
    {
        private InventoryManager inventory;
        private Font font;
        private ShopItemCard itemCardPrefab;
        private RectTransform grid;
        private ScrollRect scroll;
        private Text currency, message;
        private ItemDataSO[] items;
        private readonly List<Button> tabs = new List<Button>();
        private ItemType category;
        public ItemType Category => category;

        public void Initialize(InventoryManager manager, Font uiFont)
        {
            inventory = manager; font = uiFont;
            var canvas = gameObject.AddComponent<Canvas>(); canvas.overrideSorting = true; canvas.sortingOrder = 110;
            gameObject.AddComponent<GraphicRaycaster>();
            var rect = (RectTransform)transform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            gameObject.AddComponent<Image>().color = new Color(.045f, .09f, .14f, 1);
            var header = ShopUI.Rect("Header", transform, new Vector2(660, 0), Vector2.zero);
            header.anchorMin = header.anchorMax = new Vector2(.5f, 1); header.pivot = new Vector2(.5f, 1);
            currency = ShopUI.Label("ShopCurrency", header, font, "", 25, new Vector2(460, 50), new Vector2(-90, -35));
            ShopUI.Button("CloseButton", header, font, "關閉", new Vector2(130, 52), new Vector2(260, -35)).onClick.AddListener(Close);
            string[] titles = { "稱號", "飾品", "家具", "管家" };
            for (int i = 0; i < titles.Length; i++)
            {
                var type = (ItemType)i;
                var tab = ShopUI.Button("Tab" + type, header, font, titles[i], new Vector2(150, 60), new Vector2(-243 + i * 162, -108));
                tab.onClick.AddListener(() => SelectCategory(type)); tabs.Add(tab);
            }
            var viewport = ShopUI.Rect("Viewport", transform, Vector2.zero, Vector2.zero);
            viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one;
            viewport.offsetMin = new Vector2(30, 90); viewport.offsetMax = new Vector2(-30, -160);
            viewport.gameObject.AddComponent<Image>().color = new Color(.06f, .12f, .18f);
            viewport.gameObject.AddComponent<RectMask2D>();
            scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped; scroll.viewport = viewport;
            grid = ShopUI.Rect("ItemGridContainer", viewport, Vector2.zero, Vector2.zero);
            grid.anchorMin = new Vector2(0, 1); grid.anchorMax = Vector2.one; grid.pivot = new Vector2(.5f, 1);
            var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(300, 260); layout.spacing = new Vector2(20, 20);
            layout.padding = new RectOffset(10, 10, 10, 10); layout.childAlignment = TextAnchor.UpperCenter;
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount; layout.constraintCount = 2;
            grid.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = grid;
            message = ShopUI.Label("ShopMessage", transform, font, "", 24, new Vector2(660, 60), Vector2.zero);
            message.rectTransform.anchorMin = message.rectTransform.anchorMax = new Vector2(.5f, 0);
            message.rectTransform.anchoredPosition = new Vector2(0, 42);
            itemCardPrefab = Resources.Load<ShopItemCard>("UI/ItemCardPrefab");
            if (itemCardPrefab == null)
            {
                itemCardPrefab = ShopItemCard.CreateTemplate(transform, font);
                itemCardPrefab.gameObject.SetActive(false);
            }
            items = Resources.LoadAll<ItemDataSO>("Items");
            System.Array.Sort(items, (a, b) => string.CompareOrdinal(a.itemId, b.itemId));
        }
        public void Open() { gameObject.SetActive(true); SelectCategory(category); }
        public void Close() { gameObject.SetActive(false); }
        private void OnEnable()
        {
            if (inventory != null) inventory.OnCurrencyChanged += RefreshCurrency;
            RefreshCurrency();
        }
        private void OnDisable() { if (inventory != null) inventory.OnCurrencyChanged -= RefreshCurrency; }
        private void RefreshCurrency()
        {
            if (currency != null && inventory != null) currency.text = "金幣 " + inventory.Coins + "    羽毛 " + inventory.Feathers;
        }
        public void SelectCategory(ItemType type)
        {
            category = type;
            foreach (Transform child in grid) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            var seen = new HashSet<string>(); int count = 0;
            foreach (var item in items)
            {
                if (item == null || !item.IsValid || item.itemType != type || !seen.Add(item.itemId)) continue;
                var card = Instantiate(itemCardPrefab, grid);
                card.Bind(item, inventory, font, value => message.text = value);
                card.gameObject.SetActive(true); count++;
            }
            for (int i = 0; i < tabs.Count; i++) tabs[i].interactable = i != (int)type;
            message.text = count == 0 ? "此分類尚無道具" : "購買後可裝備，每個分類可裝備一件";
            scroll.StopMovement(); scroll.verticalNormalizedPosition = 1;
            RefreshCurrency();
        }
    }
}

