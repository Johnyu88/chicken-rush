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
        public Sprite icon;
        [Min(0)] public int price;

        public bool IsValid => !string.IsNullOrWhiteSpace(itemId) && itemId == itemId.Trim() &&
            price >= 0 && Enum.IsDefined(typeof(ItemType), itemType);
    }
}
