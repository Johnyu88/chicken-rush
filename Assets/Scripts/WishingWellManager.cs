using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenRush
{
    public enum WishCurrency { Coins, Feathers }
    public enum WishTheme { WishingWell, Santa }
    public enum WishResult { Success, InsufficientCurrency, AllCollected, InvalidConfiguration }

    [DisallowMultipleComponent]
    public sealed class WishingWellManager : MonoBehaviour
    {
        [SerializeField] private InventoryManager inventory;
        [SerializeField, Min(0)] private int coinPrice = 100;
        [SerializeField, Min(0)] private int featherPrice = 10;
        [SerializeField] private WishTheme theme = WishTheme.WishingWell;
        public int CoinPrice => coinPrice;
        public int FeatherPrice => featherPrice;
        public WishTheme Theme => theme;
        public void Initialize(InventoryManager manager) { inventory = manager; }
        public void SetTheme(WishTheme value) { theme = value; }

        public WishResult Wish(WishCurrency currency, out ItemDataSO reward)
        {
            reward = null;
            if (inventory == null || coinPrice < 0 || featherPrice < 0 ||
                !Enum.IsDefined(typeof(WishCurrency), currency) || !Enum.IsDefined(typeof(WishTheme), theme))
                return WishResult.InvalidConfiguration;
            var remaining = new List<ItemDataSO>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in Resources.LoadAll<ItemDataSO>("Items"))
            {
                if (item == null || item.itemType != ItemType.Costume || !item.HasValidId) continue;
                // Ambiguous IDs must not silently bias the random draw or resolve a different overlay.
                if (!ids.Add(item.itemId)) return WishResult.InvalidConfiguration;
                if (!inventory.OwnsItem(item)) remaining.Add(item);
            }
            if (ids.Count == 0) return WishResult.InvalidConfiguration;
            if (remaining.Count == 0) return WishResult.AllCollected;
            int coins = currency == WishCurrency.Coins ? coinPrice : 0;
            int feathers = currency == WishCurrency.Feathers ? featherPrice : 0;
            if (inventory.Coins < coins || inventory.Feathers < feathers) return WishResult.InsufficientCurrency;
            var chosen = remaining[UnityEngine.Random.Range(0, remaining.Count)];
            if (!inventory.TryUnlockFromWish(chosen, coins, feathers)) return WishResult.InvalidConfiguration;
            reward = chosen;
            return WishResult.Success;
        }
    }
}
