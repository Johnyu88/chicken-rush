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
        private PlayerData data;
        public event Action Changed;
        public event Action OnCurrencyChanged;
        public int Feathers { get { EnsureLoaded(); return data.feathers; } }
        public bool OwnsItem(ItemDataSO item) { EnsureLoaded(); return item != null && data.ownedItemIds.Contains(item.itemId); }
        public int Coins { get { EnsureLoaded(); return data.coins; } }
        public IReadOnlyList<string> OwnedItemIds { get { EnsureLoaded(); return data.ownedItemIds.AsReadOnly(); } }
        public PlayerData GetSnapshot() { EnsureLoaded(); return data.Copy(); }
        public string GetEquippedItemId(ItemType type) { EnsureLoaded(); return data.GetEquippedItemId(type); }
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

        public bool BuyItem(ItemDataSO item)
        {
            EnsureLoaded();
            if (item == null || !item.IsValid || data.ownedItemIds.Contains(item.itemId) || data.coins < item.price)
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
