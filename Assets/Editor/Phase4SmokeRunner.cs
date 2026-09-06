using System;
using ChickenRush;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Isolated test saves only; run in a project copy, without -quit.</summary>
[InitializeOnLoad]
public static class Phase4SmokeRunner
{
    private const string TestKey = "ChickenRush.Tests.Phase4";
    private static int stage, frames;
    private static double deadline;
    private static GameManager game;
    private static InventoryManager inventory;
    static Phase4SmokeRunner()
    {
        if (SessionState.GetBool("ChickenRushPhase4Smoke", false)) EditorApplication.update += Tick;
    }
    public static void Run()
    {
        MvpSceneBuilder.Build();
        var manager = Object.FindFirstObjectByType<InventoryManager>();
        var serialized = new SerializedObject(manager);
        serialized.FindProperty("playerPrefsKey").stringValue = TestKey;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        PlayerPrefs.DeleteKey(TestKey);
        PlayerPrefs.DeleteKey(TestKey + ".backup");
        PlayerPrefs.Save();
        SessionState.SetBool("ChickenRushPhase4Smoke", true);
        EditorApplication.EnterPlaymode();
    }
    private static void Check(bool condition, string message)
    { if (!condition) throw new Exception(message); }
    public static void VerifySaved()
    {
        // A separate Unity process verifies that PlayerPrefs.Save reached persistent storage.
        string primary = PlayerPrefs.GetString(TestKey);
        string backup = PlayerPrefs.GetString(TestKey + ".backup");
        GameObject probe = null;
        try
        {
            var manager = LoadProbe(out probe);
            Check(manager.Coins == 60 && manager.OwnedItemIds.Count == 2, "Saved data did not survive process exit");
            Check(manager.GetEquippedItemId(ItemType.Title) == "test.title" &&
                manager.GetEquippedItemId(ItemType.Costume) == "test.costume", "Saved equipment");
            var snapshot = manager.GetSnapshot(); snapshot.coins = 0; snapshot.ownedItemIds.Clear();
            Check(manager.Coins == 60 && manager.OwnedItemIds.Count == 2, "Snapshot can mutate live data");
            var furniture = Item("test.furniture", ItemType.Furniture, 0);
            var butler = Item("test.butler", ItemType.Butler, 0);
            Check(manager.BuyItem(furniture) && manager.EquipItem(furniture) &&
                manager.BuyItem(butler) && manager.EquipItem(butler), "Free furniture/butler items");
            Check(manager.GetEquippedItemId(ItemType.Furniture) == furniture.itemId &&
                manager.GetEquippedItemId(ItemType.Butler) == butler.itemId &&
                manager.GetEquippedItemId(ItemType.Title) == "test.title", "Four independent slots");
            Object.DestroyImmediate(furniture); Object.DestroyImmediate(butler); Object.DestroyImmediate(probe);
            PlayerPrefs.SetString(TestKey + ".backup", primary);
            PlayerPrefs.SetString(TestKey, "{ broken json");
            manager = LoadProbe(out probe);
            Check(manager.Coins == 60, "Malformed save did not recover from backup");
            Object.DestroyImmediate(probe);
            PlayerPrefs.SetString(TestKey,
                "{\"coins\":-5,\"ownedItemIds\":[\"a\",\"a\",\"\",null],\"equippedTitleId\":\"missing\"}");
            manager = LoadProbe(out probe);
            Check(manager.Coins == 0 && manager.OwnedItemIds.Count == 1 && manager.GetEquippedItemId(ItemType.Title) == "",
                "Loaded data normalization");
            Check(Resources.LoadAll<ItemDataSO>("Items").Length >= 4, "Missing example item assets");
            Debug.Log("CHICKEN_RUSH_PHASE4_PERSISTENCE_PASS: process restart, four slots, free items, backup, normalization, snapshots");
        }
        catch (Exception ex) { Debug.LogException(ex); EditorApplication.Exit(1); return; }
        finally
        {
            if (probe != null) Object.DestroyImmediate(probe);
            PlayerPrefs.SetString(TestKey, primary);
            PlayerPrefs.SetString(TestKey + ".backup", backup);
            PlayerPrefs.Save();
        }
        EditorApplication.Exit(0);
    }

    private static InventoryManager LoadProbe(out GameObject probe)
    {
        probe = new GameObject("Phase 4 Save Probe", typeof(InventoryManager));
        var manager = probe.GetComponent<InventoryManager>();
        var serialized = new SerializedObject(manager);
        serialized.FindProperty("playerPrefsKey").stringValue = TestKey;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return manager;
    }
    private static ItemDataSO Item(string id, ItemType type, int price)
    {
        var item = ScriptableObject.CreateInstance<ItemDataSO>();
        item.itemId = id; item.itemName = id; item.itemType = type; item.price = price;
        return item;
    }
    private static void Tick()
    {
        if (!EditorApplication.isPlaying) return;
        try
        {
            if (deadline == 0) deadline = EditorApplication.timeSinceStartup + 40;
            Check(EditorApplication.timeSinceStartup < deadline, "Phase 4 timeout: " + stage);
            if (stage == 0)
            {
                if (++frames < 5) return;
                game = Object.FindFirstObjectByType<GameManager>();
                inventory = Object.FindFirstObjectByType<InventoryManager>();
                Check(inventory.Coins == 0, "New player must start at zero coins");
                var title = Item("test.title", ItemType.Title, 30);
                Check(!inventory.BuyItem(title) && !inventory.EquipItem(title), "Unfunded/unowned item accepted");
                Check(!inventory.AddCoins(-1) && inventory.Coins == 0, "Negative grant accepted");
                Check(inventory.AddCoins(100), "Coin grant failed");
                Check(inventory.BuyItem(title) && inventory.Coins == 70, "Purchase did not debit coins");
                Check(!inventory.BuyItem(title) && inventory.Coins == 70, "Duplicate purchase charged twice");
                Check(inventory.EquipItem(title), "Owned title not equipped");
                var costume = Item("test.costume", ItemType.Costume, 20);
                Check(inventory.BuyItem(costume) && inventory.EquipItem(costume), "Costume purchase/equip");
                Check(inventory.GetEquippedItemId(ItemType.Title) == title.itemId &&
                    inventory.GetEquippedItemId(ItemType.Costume) == costume.itemId, "Equipment slots interfere");
                var invalid = Item(" ", ItemType.Title, -5);
                Check(!inventory.BuyItem(invalid) && inventory.Coins == 50, "Invalid item accepted");
                Check(!inventory.AddCoins(int.MaxValue) && inventory.Coins == 50, "Coin overflow");
                Check(GameObject.Find("CoinsLabel").GetComponent<Text>().text.Contains("50"), "Menu coins did not refresh");
                Object.Destroy(title); Object.Destroy(costume); Object.Destroy(invalid);
                Object.FindFirstObjectByType<GameDifficultyManager>().SelectDifficulty(GameDifficulty.Relaxed);
                var prefab = AssetDatabase.LoadAssetAtPath<ChickenController>("Assets/Prefabs/Chicken.prefab");
                for (int i = 0; i < game.ActiveNest.Capacity; i++)
                {
                    var chicken = Object.Instantiate(prefab, game.ActiveNest.transform.position, Quaternion.identity);
                    chicken.Initialize(game, Vector2.zero);
                    Check(game.ActiveNest.TryAcceptChicken(chicken), "Nest rejected test chicken");
                }
                game.BeginNestCompletion(game.ActiveNest);
                Check(inventory.Coins == 50, "Coins awarded before settlement");
                stage = 1;
            }
            else if (stage == 1 && !game.IsChangingNest)
            {
                Check(inventory.Coins == 60 && game.Score == 100, "Full nest must award coins exactly once");
                game.RestartGame(); stage = 2; frames = 0;
            }
            else if (stage == 2)
            {
                if (++frames < 5) return;
                inventory = Object.FindFirstObjectByType<InventoryManager>();
                Check(inventory.Coins == 60 && inventory.OwnedItemIds.Count == 2, "Inventory lost on reload");
                Check(inventory.GetEquippedItemId(ItemType.Title) == "test.title" &&
                    inventory.GetEquippedItemId(ItemType.Costume) == "test.costume", "Equipment lost on reload");
                Check(GameObject.Find("CoinsLabel").GetComponent<Text>().text.Contains("60"), "Reloaded menu coin value");
                SessionState.SetBool("ChickenRushPhase4Smoke", false);
                Debug.Log("CHICKEN_RUSH_PHASE4_PASS: purchase, equip, validation, overflow, menu, settlement, scene reload");
                EditorApplication.Exit(0);
            }
        }
        catch (Exception ex)
        {
            SessionState.SetBool("ChickenRushPhase4Smoke", false);
            Debug.LogException(ex); EditorApplication.Exit(1);
        }
    }
}
