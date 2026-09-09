using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenRush
{
    /// <summary>One scene-owned inventory; scene reloads read the same persisted player data.</summary>
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public sealed class InventoryManager : MonoBehaviour
    {
        [SerializeField] private string playerPrefsKey = "ChickenRush.PlayerData.v1";
        [SerializeField, Min(1)] private int homeFurnitureLimit = 24;
        public int HomeFurnitureLimit => Mathf.Max(1, homeFurnitureLimit);
        private PlayerData data;
        private ItemDataSO[] itemCatalog;
        public event Action Changed;
        public event Action OnCurrencyChanged;
        public int Feathers { get { EnsureLoaded(); return data.feathers; } }
        public bool OwnsItem(ItemDataSO item) { EnsureLoaded(); return item != null && data.ownedItemIds.Contains(item.itemId); }
        public int Coins { get { EnsureLoaded(); return data.coins; } }
        public IReadOnlyList<string> OwnedItemIds { get { EnsureLoaded(); return data.ownedItemIds.AsReadOnly(); } }
        public PlayerData GetSnapshot() { EnsureLoaded(); return data.Copy(); }
        public string GetEquippedItemId(ItemType type) { EnsureLoaded(); return data.GetEquippedItemId(type); }
        public ItemDataSO GetEquippedItem(ItemType type)
        {
            string id = GetEquippedItemId(type);
            if (string.IsNullOrEmpty(id)) return null;
            if (itemCatalog == null) itemCatalog = Resources.LoadAll<ItemDataSO>("Items");
            foreach (var item in itemCatalog)
                if (item != null && item.IsValid && item.itemType == type && item.itemId == id && OwnsItem(item)) return item;
            return null; // Deleted or unresolvable assets never become a visible costume.
        }
        private void Awake() { EnsureLoaded(); }

        private void EnsureLoaded()
        {
            if (data != null) return;
            if (TryRead(playerPrefsKey, out data)) return;
            if (TryRead(playerPrefsKey + ".backup", out data))
                Debug.LogWarning("玩家存檔無法讀取，已載入上一份備份。", this);
            else data = new PlayerData();
        }

        private static bool TryRead(string key, out PlayerData loaded)
        {
            loaded = null;
            if (!PlayerPrefs.HasKey(key)) return false;
            try
            {
                string json = PlayerPrefs.GetString(key);
                if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{")) return false;
                loaded = JsonUtility.FromJson<PlayerData>(json);
                if (loaded == null) return false;
                loaded.Normalize();
                return true;
            }
            catch (ArgumentException) { loaded = null; return false; }
        }

        public bool AddCoins(int amount)
        {
            EnsureLoaded();
            if (amount <= 0 || amount > int.MaxValue - data.coins) return false;
            var next = data.Copy(); next.coins += amount;
            return Commit(next);
        }

        public bool AddFeathers(int amount)
        {
            EnsureLoaded();
            if (amount <= 0 || amount > int.MaxValue - data.feathers) return false;
            var next = data.Copy(); next.feathers += amount;
            return Commit(next);
        }

        // A wish spends and unlocks together; no observable or persisted intermediate debit.
        public bool TryUnlockFromWish(ItemDataSO item, int coinCost, int featherCost)
        {
            EnsureLoaded();
            var next = data.Copy();
            if (item == null || !item.HasValidId || item.itemType != ItemType.Costume ||
                coinCost < 0 || featherCost < 0 || next.ownedItemIds.Contains(item.itemId) ||
                next.coins < coinCost || next.feathers < featherCost) return false;
            next.coins -= coinCost;
            next.feathers -= featherCost;
            next.ownedItemIds.Add(item.itemId);
            return Commit(next);
        }

        // Legacy compatibility API for old saves/tests; not a player-facing purchase entry.
        public bool BuyItem(ItemDataSO item)
        {
            EnsureLoaded();
            if (item == null || !item.IsValid || item.price < 0 || data.ownedItemIds.Contains(item.itemId) || data.coins < item.price)
                return false;
            var next = data.Copy();
            next.coins -= item.price;
            next.ownedItemIds.Add(item.itemId);
            return Commit(next);
        }

        public bool EquipItem(ItemDataSO item)
        {
            EnsureLoaded();
            if (item == null || !item.IsValid || !data.ownedItemIds.Contains(item.itemId)) return false;
            if (data.GetEquippedItemId(item.itemType) == item.itemId) return true;
            var next = data.Copy(); next.SetEquippedItemId(item.itemType, item.itemId);
            return Commit(next);
        }

        // Home configuration references existing ownership; it never grants assets.
        private ItemDataSO ResolveOwnedHomeItem(string id, ItemType type)
        {
            EnsureLoaded();
            if (string.IsNullOrWhiteSpace(id) || !data.ownedItemIds.Contains(id)) return null;
            ItemDataSO found = null;
            foreach (var item in Resources.LoadAll<ItemDataSO>("Items"))
            {
                if (item == null || item.itemId != id) continue;
                if (found != null || !item.IsValid || item.itemType != type) return null;
                found = item;
            }
            return found;
        }
        public IReadOnlyList<PlacedHomeItemData> GetActiveHomeFurniture()
        {
            EnsureLoaded();
            var active = new List<PlacedHomeItemData>(); var seen = new HashSet<string>();
            if (data.homeNest.version != HomeNestData.CurrentVersion) return active.AsReadOnly();
            foreach (var item in data.homeNest.placedFurniture)
                if (item.IsValid && ResolveOwnedHomeItem(item.itemId, ItemType.Furniture) != null && seen.Add(item.itemId)) active.Add(item.Copy());
            return active.AsReadOnly();
        }
        public ItemDataSO GetActiveHomeButler()
        {
            EnsureLoaded();
            return data.homeNest.version == HomeNestData.CurrentVersion ? ResolveOwnedHomeItem(data.homeNest.activeButlerId, ItemType.Butler) : null;
        }
        public bool TryPlaceHomeFurniture(PlacedHomeItemData placement)
        {
            EnsureLoaded();
            if (placement == null || !placement.IsValid || data.homeNest.version != HomeNestData.CurrentVersion ||
                ResolveOwnedHomeItem(placement.itemId, ItemType.Furniture) == null) return false;
            var next = data.Copy();
            // One placement per owned ID until a future explicit quantity model exists.
            next.homeNest.placedFurniture.RemoveAll(item => item.itemId == placement.itemId);
            next.homeNest.placedFurniture.Add(placement.Copy());
            return Commit(next);
        }
        public bool TrySetHomeButler(string itemId)
        {
            EnsureLoaded();
            if (data.homeNest.version != HomeNestData.CurrentVersion || itemId == null ||
                (itemId.Length > 0 && ResolveOwnedHomeItem(itemId, ItemType.Butler) == null)) return false;
            var next = data.Copy(); next.homeNest.activeButlerId = itemId;
            return Commit(next);
        }

        public bool IsOwnedFurniture(string id) => ResolveOwnedHomeItem(id, ItemType.Furniture) != null;

        // Revalidate the same commands against current ownership and reject stale home sessions.
        internal bool TryCommitFurnitureCommands(string expectedHome, IReadOnlyList<PlacementCommand> commands)
        {
            EnsureLoaded();
            if (expectedHome != JsonUtility.ToJson(data.homeNest) || commands == null || commands.Count == 0 ||
                !PlacementRules.TryApply(data.homeNest, commands, IsOwnedFurniture, HomeFurnitureLimit, out var home)) return false;
            var next = data.Copy(); next.homeNest = home;
            return Commit(next);
        }
        private bool Commit(PlayerData next)
        {
            string previous = JsonUtility.ToJson(data);
            try
            {
                PlayerPrefs.SetString(playerPrefsKey + ".backup", previous);
                PlayerPrefs.SetString(playerPrefsKey, JsonUtility.ToJson(next));
                PlayerPrefs.Save();
            }
            catch (PlayerPrefsException ex)
            {
                // Keep the in-memory transaction unchanged if the storage write fails.
                try { PlayerPrefs.SetString(playerPrefsKey, previous); }
                catch (PlayerPrefsException) { /* The backup key remains the recovery path. */ }
                Debug.LogError("玩家存檔失敗：" + ex.Message, this);
                return false;
            }
            bool currencyChanged = data.coins != next.coins || data.feathers != next.feathers;
            data = next;
            if (currencyChanged) OnCurrencyChanged?.Invoke();
            Changed?.Invoke();
            return true;
        }
    }
}
