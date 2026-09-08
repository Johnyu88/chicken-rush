using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenRush
{
    public sealed class WishingWellCanvas : MonoBehaviour
    {
        private InventoryManager inventory;
        private WishingWellManager manager;
        private Text title, currency, message, rewardName;
        private Button coins, feathers, equip, previous, next;
        private CanvasGroup reveal;
        private Image icon;
        private ItemDataSO displayed;
        private bool busy;
        public bool IsBusy => busy;
        public ItemDataSO DisplayedItem => displayed;
        public WishResult LastResult { get; private set; }
        public string Message => message.text;

        public void Initialize(InventoryManager owner, WishingWellManager wishing, Font font)
        {
            inventory = owner; manager = wishing;
            var canvas = gameObject.AddComponent<Canvas>(); canvas.overrideSorting = true; canvas.sortingOrder = 110;
            gameObject.AddComponent<GraphicRaycaster>();
            var rect = (RectTransform)transform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            gameObject.AddComponent<Image>().color = new Color(.045f, .09f, .14f);
            title = RuntimeUI.Label("Title", transform, font, "", 38, new Vector2(650, 70), new Vector2(0, 355));
            currency = RuntimeUI.Label("Currency", transform, font, "", 26, new Vector2(650, 55), new Vector2(0, 288));
            RuntimeUI.Button("CloseButton", transform, font, "關閉", new Vector2(160, 55), new Vector2(0, -390)).onClick.AddListener(Close);
            coins = RuntimeUI.Button("CoinsWishButton", transform, font, "", new Vector2(290, 65), new Vector2(-155, 204));
            feathers = RuntimeUI.Button("FeathersWishButton", transform, font, "", new Vector2(290, 65), new Vector2(155, 204));
            coins.onClick.AddListener(() => BeginWish(WishCurrency.Coins));
            feathers.onClick.AddListener(() => BeginWish(WishCurrency.Feathers));
            message = RuntimeUI.Label("Message", transform, font, "", 26, new Vector2(660, 85), new Vector2(0, 112));
            var panel = RuntimeUI.Rect("RevealPanel", transform, new Vector2(600, 345), new Vector2(0, -108));
            panel.gameObject.AddComponent<Image>().color = new Color(.09f, .18f, .24f);
            reveal = panel.gameObject.AddComponent<CanvasGroup>();
            icon = RuntimeUI.Rect("ItemIcon", panel, new Vector2(150, 150), new Vector2(0, 72)).gameObject.AddComponent<Image>();
            icon.preserveAspect = true; icon.raycastTarget = false;
            rewardName = RuntimeUI.Label("ItemName", panel, font, "", 26, new Vector2(570, 60), new Vector2(0, -34));
            equip = RuntimeUI.Button("EquipButton", panel, font, "裝備", new Vector2(230, 58), new Vector2(0, -111));
            equip.onClick.AddListener(() => { if (!busy && displayed != null) { inventory.EquipItem(displayed); Refresh(); } });
            previous = RuntimeUI.Button("PreviousOwned", transform, font, "上一件", new Vector2(200, 50), new Vector2(-135, -326));
            next = RuntimeUI.Button("NextOwned", transform, font, "下一件", new Vector2(200, 50), new Vector2(135, -326));
            previous.onClick.AddListener(() => Browse(-1)); next.onClick.AddListener(() => Browse(1));
            reveal.gameObject.SetActive(false);
        }
        public void Open() { gameObject.SetActive(true); Refresh(); if (displayed == null) Browse(0); else Display(displayed); }
        public void Close() { gameObject.SetActive(false); }
        private void OnEnable()
        {
            if (inventory != null) { inventory.OnCurrencyChanged += Refresh; inventory.Changed += Refresh; }
            Refresh();
        }
        private void OnDisable()
        {
            StopAllCoroutines(); busy = false;
            if (inventory != null) { inventory.OnCurrencyChanged -= Refresh; inventory.Changed -= Refresh; }
            if (reveal != null) { reveal.alpha = 1; reveal.transform.localScale = Vector3.one; }
            if (message != null) message.text = "";
        }
        private void Refresh()
        {
            if (manager == null || currency == null || inventory == null) return;
            title.text = manager.Theme == WishTheme.Santa ? "🎅 Santa 聖誕許願" : "✨ 許願池";
            currency.text = "金幣 " + inventory.Coins + "    羽毛 " + inventory.Feathers;
            coins.GetComponentInChildren<Text>().text = "金幣許願 · " + manager.CoinPrice;
            feathers.GetComponentInChildren<Text>().text = "羽毛許願 · " + manager.FeatherPrice;
            coins.interactable = feathers.interactable = previous.interactable = next.interactable = !busy;
            bool equipped = displayed != null && inventory.GetEquippedItemId(ItemType.Costume) == displayed.itemId;
            equip.interactable = !busy && displayed != null && inventory.OwnsItem(displayed) && !equipped;
            equip.GetComponentInChildren<Text>().text = equipped ? "已裝備" : "裝備";
        }
        private void Browse(int direction)
        {
            if (busy) return;
            var owned = new List<ItemDataSO>(); var ids = new HashSet<string>();
            foreach (var item in Resources.LoadAll<ItemDataSO>("Items"))
                if (item != null && item.HasValidId && item.itemType == ItemType.Costume && inventory.OwnsItem(item) && ids.Add(item.itemId)) owned.Add(item);
            owned.Sort((a, b) => string.CompareOrdinal(a.itemId, b.itemId));
            if (owned.Count == 0) return;
            int index = owned.FindIndex(item => item == displayed);
            index = index < 0 ? 0 : (index + direction + owned.Count) % owned.Count;
            Display(owned[index]);
        }
        private void Display(ItemDataSO item)
        {
            displayed = item; icon.sprite = item.icon; icon.enabled = item.icon != null;
            rewardName.text = string.IsNullOrWhiteSpace(item.itemName) ? item.itemId : item.itemName;
            reveal.gameObject.SetActive(true); reveal.alpha = 1; reveal.transform.localScale = Vector3.one; Refresh();
        }
        public void BeginWish(WishCurrency choice)
        {
            if (busy || !isActiveAndEnabled) return;
            busy = true; Refresh(); StartCoroutine(PerformWish(choice));
        }
        private IEnumerator PerformWish(WishCurrency choice)
        {
            reveal.gameObject.SetActive(false);
            message.text = manager.Theme == WishTheme.Santa ? "✨ 願望正在飛向 Santa…" : "✨ 願望正在飛向天空…";
            yield return new WaitForSecondsRealtime(.65f);
            LastResult = manager.Wish(choice, out var reward);
            if (LastResult == WishResult.Success)
            {
                Display(reward); message.text = "🎁 願望實現：" + rewardName.text + "！";
                for (float elapsed = 0; elapsed < .3f; elapsed += Time.unscaledDeltaTime)
                {
                    float t = Mathf.Clamp01(elapsed / .3f); reveal.alpha = t;
                    reveal.transform.localScale = Vector3.one * Mathf.Lerp(.8f, 1, t); yield return null;
                }
                reveal.alpha = 1; reveal.transform.localScale = Vector3.one;
            }
            else if (LastResult == WishResult.AllCollected) message.text = "✨ 目前所有願望都實現了！";
            else if (LastResult == WishResult.InsufficientCurrency) message.text = choice == WishCurrency.Coins ? "金幣不夠喔！" : "羽毛不夠喔！";
            else message.text = "許願暫時無法進行，請稍後再試。";
            busy = false; Refresh();
        }
    }
}
