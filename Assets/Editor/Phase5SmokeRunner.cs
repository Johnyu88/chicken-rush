using System;
using ChickenRush;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class Phase5SmokeRunner
{
    private const string Key = "ChickenRush.Tests.Phase5";
    private static int frames;
    static Phase5SmokeRunner()
    {
        if (SessionState.GetBool(Key, false)) EditorApplication.update += Tick;
    }
    public static void Run()
    {
        MvpSceneBuilder.Build();
        var manager = Object.FindFirstObjectByType<InventoryManager>();
        var serialized = new SerializedObject(manager);
        serialized.FindProperty("playerPrefsKey").stringValue = Key;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        PlayerPrefs.DeleteKey(Key); PlayerPrefs.DeleteKey(Key + ".backup"); PlayerPrefs.Save();
        SessionState.SetBool(Key, true);
        EditorApplication.EnterPlaymode();
    }
    private static void Check(bool result, string message) { if (!result) throw new Exception(message); }
    private static ShopItemCard Card(ShopCanvas shop)
    {
        foreach (var card in shop.GetComponentsInChildren<ShopItemCard>())
            if (card.Item != null) return card;
        throw new Exception("No visible card");
    }
    private static string Caption(ShopItemCard card) => card.ActionButton.GetComponentInChildren<Text>().text;
    private static void CaptureShop(MainMenuCanvas menu, ShopCanvas shop)
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
        shop.SelectCategory(ItemType.Title);
        var canvas = menu.GetComponent<Canvas>();
        var camera = Camera.main;
        var target = new RenderTexture(720, 960, 24);
        camera.targetTexture = target;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera; canvas.planeDistance = 1;
        Canvas.ForceUpdateCanvases();
        camera.Render();
        var previous = RenderTexture.active; RenderTexture.active = target;
        var image = new Texture2D(720, 960, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 720, 960), 0, 0); image.Apply();
        System.IO.File.WriteAllBytes("Phase5Shop.png", image.EncodeToPNG());
        RenderTexture.active = previous; camera.targetTexture = null;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        Object.Destroy(image); Object.Destroy(target);
    }
    private static void Tick()
    {
        if (!EditorApplication.isPlaying || ++frames < 8) return;
        try
        {
            var inventory = Object.FindFirstObjectByType<InventoryManager>();
            var menu = Object.FindFirstObjectByType<MainMenuCanvas>();
            var shop = menu.GetComponentInChildren<ShopCanvas>(true);
            Check(shop != null && !shop.gameObject.activeSelf, "Shop should start closed");
            menu.transform.Find("Menu/ShopButton").GetComponent<Button>().onClick.Invoke();
            Check(shop.gameObject.activeSelf, "Menu button did not open shop");
            var card = Card(shop);
            Check(Caption(card) == "購買", "Unowned caption");
            card.ActionButton.onClick.Invoke();
            Check(inventory.Coins == 0 && !inventory.OwnsItem(card.Item), "Insufficient purchase changed inventory");
            Check(shop.transform.Find("ShopMessage").GetComponent<Text>().text == "金幣不足", "Missing insufficient funds feedback");
            int currencyEvents = 0;
            inventory.OnCurrencyChanged += () => currencyEvents++;
            Check(inventory.AddCoins(2000) && inventory.AddFeathers(17), "Currency grants");
            Check(currencyEvents == 2 && !inventory.AddFeathers(-1) && !inventory.AddFeathers(int.MaxValue), "Currency event/validation");
            Check(menu.transform.Find("FeathersLabel").GetComponent<Text>().text.Contains("17"), "Feather UI stale");
            int price = card.Item.price;
            card.ActionButton.onClick.Invoke();
            Check(inventory.Coins == 2000 - price && Caption(card) == "裝備", "Purchase transition/debit");
            int afterPurchase = currencyEvents;
            card.ActionButton.onClick.Invoke();
            Check(Caption(card) == "已裝備" && !card.ActionButton.interactable, "Equip state");
            Check(currencyEvents == afterPurchase, "Equip emitted currency event");
            Check(!inventory.BuyItem(card.Item) && inventory.Coins == 2000 - price, "Duplicate debit");
            var alternate = ScriptableObject.CreateInstance<ItemDataSO>();
            alternate.itemId = "phase5.alternate"; alternate.itemType = ItemType.Title; alternate.price = 0;
            Check(inventory.BuyItem(alternate) && inventory.EquipItem(alternate), "Alternative equipment");
            Check(Caption(card) == "裝備" && card.ActionButton.interactable, "Old equipped card stale");
            foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
            {
                shop.SelectCategory(type);
                foreach (var visible in shop.GetComponentsInChildren<ShopItemCard>())
                    Check(visible.Item.itemType == type, "Wrong category visible");
                var visibleCard = Card(shop);
                if (!inventory.OwnsItem(visibleCard.Item)) visibleCard.ActionButton.onClick.Invoke();
                visibleCard.ActionButton.onClick.Invoke();
                Check(inventory.GetEquippedItemId(type) == visibleCard.Item.itemId, "Category equip");
            }
            shop.transform.Find("Header/CloseButton").GetComponent<Button>().onClick.Invoke();
            Check(!shop.gameObject.activeSelf, "Close button");
            inventory.AddFeathers(3); shop.Open();
            Check(shop.transform.Find("Header/ShopCurrency").GetComponent<Text>().text.Contains("20"), "Reopen currency stale");
            Check(!Card(shop).ActionButton.interactable, "Reopen equipment stale");
            var saved = JsonUtility.FromJson<PlayerData>(PlayerPrefs.GetString(Key));
            Check(saved.feathers == 20 && saved.coins == inventory.Coins, "Currency persistence");
            var legacy = JsonUtility.FromJson<PlayerData>("{\"coins\":42}"); legacy.Normalize();
            Check(legacy.feathers == 0 && legacy.coins == 42, "Legacy save migration");
            CaptureShop(menu, shop);
            menu.gameObject.SetActive(false);
            Check(!shop.gameObject.activeSelf, "Shop should close with main menu");
            Object.Destroy(alternate);
            Debug.Log("PHASE5_SMOKE_PASS");
            SessionState.SetBool(Key, false); EditorApplication.Exit(0);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex); SessionState.SetBool(Key, false); EditorApplication.Exit(1);
        }
    }
}

