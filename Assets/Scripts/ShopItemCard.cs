using UnityEngine;
using UnityEngine.UI;

namespace ChickenRush
{
    public sealed class ShopItemCard : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text priceLabel;
        [SerializeField] private Button actionButton;
        [SerializeField] private Text actionLabel;
        private InventoryManager inventory;
        private ItemDataSO item;
        private System.Action<string> showMessage;
        public ItemDataSO Item => item;
        public Button ActionButton => actionButton;

        public void Bind(ItemDataSO value, InventoryManager manager, Font font, System.Action<string> message)
        {
            Unsubscribe();
            item = value; inventory = manager; showMessage = message;
            foreach (var label in GetComponentsInChildren<Text>(true)) label.font = font;
            actionButton.onClick.RemoveListener(Act);
            actionButton.onClick.AddListener(Act);
            if (isActiveAndEnabled && inventory != null) inventory.Changed += Refresh;
            Refresh();
        }
        private void OnEnable() { if (inventory != null) inventory.Changed += Refresh; Refresh(); }
        private void OnDisable() { Unsubscribe(); }
        private void Unsubscribe() { if (inventory != null) inventory.Changed -= Refresh; }
        private void Refresh()
        {
            if (item == null || inventory == null || actionButton == null) return;
            icon.sprite = item.icon; icon.enabled = item.icon != null;
            nameLabel.text = item.itemName;
            priceLabel.text = item.price == 0 ? "免費" : item.price + " 金幣";
            bool owned = inventory.OwnsItem(item);
            bool equipped = owned && inventory.GetEquippedItemId(item.itemType) == item.itemId;
            actionLabel.text = equipped ? "已裝備" : owned ? "裝備" : "購買";
            actionButton.interactable = item.IsValid && !equipped;
        }
        private void Act()
        {
            if (item == null || inventory == null) return;
            bool owned = inventory.OwnsItem(item);
            bool success = owned ? inventory.EquipItem(item) : inventory.BuyItem(item);
            showMessage?.Invoke(success ? (owned ? "已裝備：" : "已購買：") + item.itemName :
                (!owned && inventory.Coins < item.price ? "金幣不足" : "操作未成功，請重試"));
            Refresh();
        }
        public static ShopItemCard CreateTemplate(Transform parent, Font font)
        {
            var root = ShopUI.Rect("ItemCardPrefab", parent, new Vector2(300, 260), Vector2.zero);
            root.gameObject.AddComponent<Image>().color = new Color(.12f, .22f, .29f);
            var card = root.gameObject.AddComponent<ShopItemCard>();
            card.icon = ShopUI.Rect("Icon", root, new Vector2(74, 74), new Vector2(0, 78)).gameObject.AddComponent<Image>();
            card.icon.preserveAspect = true; card.icon.raycastTarget = false;
            card.nameLabel = ShopUI.Label("ItemName", root, font, "", 26, new Vector2(280, 40), new Vector2(0, 18));
            card.priceLabel = ShopUI.Label("Price", root, font, "", 23, new Vector2(280, 35), new Vector2(0, -23));
            card.actionButton = ShopUI.Button("ActionButton", root, font, "購買", new Vector2(250, 56), new Vector2(0, -82));
            card.actionLabel = card.actionButton.GetComponentInChildren<Text>();
            return card;
        }
    }
}
