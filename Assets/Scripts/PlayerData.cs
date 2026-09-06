using System;
using System.Collections.Generic;

namespace ChickenRush
{
    [Serializable]
    public sealed class PlayerData
    {
        public int coins;
        public List<string> ownedItemIds = new List<string>();
        public string equippedTitleId = "";
        public string equippedCostumeId = "";
        public string equippedFurnitureId = "";
        public string equippedButlerId = "";

        // Old/partial JSON can omit lists and slots. Keep valid ownership, remove malformed entries.
        public void Normalize()
        {
            coins = Math.Max(0, coins);
            if (ownedItemIds == null) ownedItemIds = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            ownedItemIds.RemoveAll(id => string.IsNullOrWhiteSpace(id) || id != id.Trim() || !seen.Add(id));
            equippedTitleId = OwnedOrEmpty(equippedTitleId);
            equippedCostumeId = OwnedOrEmpty(equippedCostumeId);
            equippedFurnitureId = OwnedOrEmpty(equippedFurnitureId);
            equippedButlerId = OwnedOrEmpty(equippedButlerId);
        }

        private string OwnedOrEmpty(string id) => id != null && ownedItemIds.Contains(id) ? id : "";

        public string GetEquippedItemId(ItemType type)
        {
            switch (type)
            {
                case ItemType.Title: return equippedTitleId;
                case ItemType.Costume: return equippedCostumeId;
                case ItemType.Furniture: return equippedFurnitureId;
                case ItemType.Butler: return equippedButlerId;
                default: throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        public void SetEquippedItemId(ItemType type, string id)
        {
            switch (type)
            {
                case ItemType.Title: equippedTitleId = id; break;
                case ItemType.Costume: equippedCostumeId = id; break;
                case ItemType.Furniture: equippedFurnitureId = id; break;
                case ItemType.Butler: equippedButlerId = id; break;
                default: throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        public PlayerData Copy() => new PlayerData
        {
            coins = coins, ownedItemIds = new List<string>(ownedItemIds),
            equippedTitleId = equippedTitleId, equippedCostumeId = equippedCostumeId,
            equippedFurnitureId = equippedFurnitureId, equippedButlerId = equippedButlerId
        };
    }
}
