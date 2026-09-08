using System;
using UnityEngine;

namespace ChickenRush
{
    public enum ItemType { Title = 0, Costume = 1, Furniture = 2, Butler = 3 }

    [CreateAssetMenu(fileName = "NewItem", menuName = "Chicken Rush/Item")]
    public sealed class ItemDataSO : ScriptableObject
    {
        [Tooltip("永久且唯一的存檔 ID；發布後請勿更改。複製資產時須指定新的 ID。")]
        public string itemId = Guid.NewGuid().ToString("N");
        public string itemName;
        public ItemType itemType;
        [Tooltip("許願揭曉圖示 / Item Icon")]
        public Sprite icon;
        [Header("飾品覆蓋：獨立於Item Icon，座標相對小雞美術根節點")]
        public Sprite costumeSprite;
        public Vector2 costumeOffset = new Vector2(0, .38f);
        public Vector2 costumeScale = new Vector2(.65f, .65f);
        public float costumeRotation;
        [Tooltip("舊 BuyItem 相容價格；許願費用由 WishingWellManager 設定")][Min(0)] public int price;

        public bool HasValidId => !string.IsNullOrWhiteSpace(itemId) && itemId == itemId.Trim();
        public bool IsValid => HasValidId &&
            Enum.IsDefined(typeof(ItemType), itemType);
    }
}
