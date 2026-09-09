using System;
using System.Linq;
using System.Reflection;
using ChickenRush;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class Phase8CSmokeRunner
{
    const string Key = "ChickenRush.Tests.Phase8C";
    static int frames;
    static Phase8CSmokeRunner() { if (SessionState.GetBool(Key, false)) EditorApplication.update += Tick; }
    public static void Run()
    {
        MvpSceneBuilder.Build();
        var serialized = new SerializedObject(Object.FindFirstObjectByType<InventoryManager>());
        serialized.FindProperty("playerPrefsKey").stringValue = Key; serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        PlayerPrefs.DeleteKey(Key + ".backup");
        PlayerPrefs.SetString(Key, "{\"coins\":2000,\"ownedItemIds\":[\"legacy.missing\",\"legacy.missing\"]}"); PlayerPrefs.Save();
        SessionState.SetBool(Key, true); EditorApplication.EnterPlaymode();
    }
    static void Check(bool value, string message) { if (!value) throw new Exception(message); }
    static InventoryManager Reload(string key)
    {
        var go = new GameObject("ReloadInventory"); go.SetActive(false); var inventory = go.AddComponent<InventoryManager>();
        typeof(InventoryManager).GetField("playerPrefsKey", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(inventory, key);
        go.SetActive(true); return inventory;
    }
    static void Tick()
    {
        if (!EditorApplication.isPlaying || ++frames < 8) return;
        try
        {
            var inventory = Object.FindFirstObjectByType<InventoryManager>(); var menu = Object.FindFirstObjectByType<MainMenuCanvas>();
            var homeUI = menu.GetComponentInChildren<MyNestCanvas>(true);
            Check(homeUI != null && !homeUI.gameObject.activeSelf, "Home default closed");
            Check(inventory.Coins == 2000 && inventory.Feathers == 0 && inventory.OwnedItemIds.Count == 1, "Legacy currency/ownership migration");
            Check(inventory.GetSnapshot().homeNest.version == 1 && inventory.GetSnapshot().homeNest.placedFurniture.Count == 0, "Legacy home default");
            menu.transform.Find("Menu/MyNestButton").GetComponent<Button>().onClick.Invoke(); Check(homeUI.gameObject.activeSelf, "Home entry");
            var furniture = Resources.LoadAll<ItemDataSO>("Items").First(x => x.itemType == ItemType.Furniture && x.IsValid);
            var butler = Resources.LoadAll<ItemDataSO>("Items").First(x => x.itemType == ItemType.Butler && x.IsValid);
            var costume = Resources.LoadAll<ItemDataSO>("Items").First(x => x.itemType == ItemType.Costume && x.IsValid);
            var placement = new PlacedHomeItemData { itemId = furniture.itemId, position = new Vector3(1, 2, 3), rotation = new Vector3(0, 45, 0), scale = Vector3.one * 2 };
            string before = JsonUtility.ToJson(inventory.GetSnapshot());
            Check(!inventory.TryPlaceHomeFurniture(placement) && !inventory.TrySetHomeButler(butler.itemId), "Unowned home grant");
            Check(before == JsonUtility.ToJson(inventory.GetSnapshot()), "Rejected home mutated save");
            Check(inventory.BuyItem(furniture) && inventory.BuyItem(butler), "Legacy fixture unlock");
            int ownedCount = inventory.OwnedItemIds.Count; int balance = inventory.Coins;
            Check(inventory.TryPlaceHomeFurniture(placement) && inventory.TrySetHomeButler(butler.itemId), "Home save");
            placement.position = Vector3.zero;
            Check(inventory.GetActiveHomeFurniture()[0].position == new Vector3(1, 2, 3), "Placement input aliased");
            var copy = inventory.GetSnapshot(); copy.homeNest.placedFurniture[0].position = Vector3.zero; copy.homeNest.activeButlerId = "";
            Check(inventory.GetActiveHomeFurniture()[0].position == new Vector3(1, 2, 3) && inventory.GetActiveHomeButler() == butler, "Snapshot not deep copied");
            var activeCopy = inventory.GetActiveHomeFurniture()[0]; activeCopy.itemId = "tampered";
            Check(inventory.GetActiveHomeFurniture()[0].itemId == furniture.itemId, "Active view aliased");
            Check(inventory.TryPlaceHomeFurniture(placement) && inventory.GetSnapshot().homeNest.placedFurniture.Count == 1, "Duplicate placement");
            Check(inventory.OwnedItemIds.Count == ownedCount && inventory.Coins == balance && !inventory.BuyItem(furniture), "Placement duplicated ownership or debit");
            Check(furniture.model3DPrefab == null && furniture.IsValid, "Missing 3D invalidated asset");
            var model = new GameObject("RepresentationFixture"); furniture.model3DPrefab = model;
            Check(inventory.OwnsItem(furniture) && inventory.OwnedItemIds.Count == ownedCount, "Representation ownership split"); furniture.model3DPrefab = null; Object.Destroy(model);
            Check(!inventory.TryPlaceHomeFurniture(new PlacedHomeItemData { itemId = costume.itemId }), "Costume accepted as furniture");
            Check(!inventory.TryPlaceHomeFurniture(new PlacedHomeItemData { itemId = furniture.itemId, position = new Vector3(float.NaN, 0, 0) }), "Invalid transform");
            Check(!inventory.TryPlaceHomeFurniture(new PlacedHomeItemData { itemId = furniture.itemId, scale = Vector3.zero }), "Invalid scale");
            var loaded = Reload(Key);
            Check(loaded.GetSnapshot().homeNest.placedFurniture[0].itemId == furniture.itemId && loaded.GetActiveHomeFurniture()[0].rotation.y == 45 && loaded.GetActiveHomeFurniture()[0].scale.x == 2 && loaded.GetActiveHomeButler() == butler, "Home round trip");
            Check(!PlayerPrefs.GetString(Key).Contains("model3DPrefab"), "Saved complete asset instead of ID"); Object.Destroy(loaded.gameObject);
            var unknown = new PlayerData(); unknown.ownedItemIds.Add("missing.furniture");
            unknown.homeNest.placedFurniture.Add(new PlacedHomeItemData { itemId = "missing.furniture" });
            unknown.homeNest.placedFurniture.Add(new PlacedHomeItemData { itemId = furniture.itemId }); // Catalog exists, ownership absent.
            unknown.homeNest.activeButlerId = butler.itemId;
            unknown.homeNest.decorations.Add(new PlacedHomeItemData { itemId = "future.decoration" });
            unknown.homeNest.exteriorItemId = "future.exterior";
            PlayerPrefs.SetString(Key + ".unknown", JsonUtility.ToJson(unknown)); PlayerPrefs.Save();
            var unresolved = Reload(Key + ".unknown");
            Check(unresolved.GetActiveHomeFurniture().Count == 0 && unresolved.GetActiveHomeButler() == null, "Unknown/unowned configuration became active");
            Check(unresolved.AddCoins(1) && unresolved.GetSnapshot().homeNest.placedFurniture.Count == 2 && unresolved.GetSnapshot().homeNest.decorations[0].itemId == "future.decoration", "Recoverable home lost on unrelated commit");
            var recovered = Reload(Key + ".unknown"); Check(recovered.GetSnapshot().homeNest.exteriorItemId == "future.exterior", "Reserved fields not persisted");
            Object.Destroy(recovered.gameObject); Object.Destroy(unresolved.gameObject);
            Check(homeUI.Summary.Contains("家具 1") && homeUI.Summary.Contains(butler.itemName), "Home summary stale");
            Capture(menu);
            homeUI.transform.Find("BackButton").GetComponent<Button>().onClick.Invoke(); Check(!homeUI.gameObject.activeSelf, "Home back");
            homeUI.Open(); menu.gameObject.SetActive(false); Check(!homeUI.gameObject.activeSelf, "Home must close with menu");
            Debug.Log("PHASE8C_SMOKE_PASS: legacy migration, UI, safe placement, deep copies, persistence, unresolved recovery, shared ownership");
            SessionState.SetBool(Key, false); EditorApplication.Exit(0);
        }
        catch (Exception ex) { Debug.LogException(ex); SessionState.SetBool(Key, false); EditorApplication.Exit(1); }
    }
    static void Capture(MainMenuCanvas menu)
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
        var canvas = menu.GetComponent<Canvas>(); var camera = Camera.main; var target = new RenderTexture(720, 960, 24);
        camera.targetTexture = target; canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
        Canvas.ForceUpdateCanvases(); camera.Render(); var old = RenderTexture.active; RenderTexture.active = target;
        var image = new Texture2D(720, 960, TextureFormat.RGB24, false); image.ReadPixels(new Rect(0, 0, 720, 960), 0, 0); image.Apply();
        System.IO.File.WriteAllBytes("Phase8CMyNest.png", image.EncodeToPNG()); RenderTexture.active = old; camera.targetTexture = null; canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        Object.Destroy(image); Object.Destroy(target);
    }
}
