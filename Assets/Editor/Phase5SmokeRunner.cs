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
public static class Phase5SmokeRunner
{
    const string Key = "ChickenRush.Tests.Phase5";
    static int frames, stage;
    static double deadline;
    static InventoryManager inventory;
    static WishingWellManager manager;
    static WishingWellCanvas ui;
    static MainMenuCanvas menu;
    static bool atomicObserved;
    static Phase5SmokeRunner() { if (SessionState.GetBool(Key, false)) EditorApplication.update += Tick; }
    public static void Run()
    {
        MvpSceneBuilder.Build();
        Prepare();
    }
    public static void RunDeliveredScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        Prepare();
    }
    static void Prepare()
    {
        var serialized = new SerializedObject(Object.FindFirstObjectByType<InventoryManager>());
        serialized.FindProperty("playerPrefsKey").stringValue = Key; serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Clear(Key); Clear(Key + ".coins"); Clear(Key + ".legacy");
        SessionState.SetBool(Key, true); EditorApplication.EnterPlaymode();
    }
    static void Clear(string key) { PlayerPrefs.DeleteKey(key); PlayerPrefs.DeleteKey(key + ".backup"); PlayerPrefs.Save(); }
    static void Check(bool value, string reason) { if (!value) throw new Exception(reason); }
    static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    static InventoryManager LoadInventory(string key)
    {
        var go = new GameObject("TestInventory"); go.SetActive(false);
        var result = go.AddComponent<InventoryManager>(); Set(result, "playerPrefsKey", key); go.SetActive(true); return result;
    }
    static Button Button(string path) => ui.transform.Find(path).GetComponent<Button>();
    static void Tick()
    {
        if (!EditorApplication.isPlaying || ++frames < 8) return;
        try
        {
            if (deadline == 0) deadline = EditorApplication.timeSinceStartup + 25;
            Check(EditorApplication.timeSinceStartup < deadline, "Wishing smoke timeout at " + stage);
            if (stage == 0)
            {
                inventory = Object.FindFirstObjectByType<InventoryManager>();
                menu = Object.FindFirstObjectByType<MainMenuCanvas>();
                ui = menu.GetComponentInChildren<WishingWellCanvas>(true); manager = menu.GetComponent<WishingWellManager>();
                Check(ui != null && !ui.gameObject.activeSelf, "Wish UI must start closed");
                menu.transform.Find("Menu/WishButton").GetComponent<Button>().onClick.Invoke();
                Check(ui.gameObject.activeSelf, "Main menu entry");
                Check(ui.transform.Find("PenguinButler") != null, "Penguin NPC missing");
                VerifyPenguin();
                Check(manager.CoinPrice == 100 && manager.FeatherPrice == 10, "Default prices");
                Button("CoinsWishButton").onClick.Invoke(); stage = 1;
            }
            else if (stage == 1 && !ui.IsBusy)
            {
                Check(ui.LastResult == WishResult.InsufficientCurrency && inventory.Coins == 0 && inventory.OwnedItemIds.Count == 0, "Insufficient wish mutated inventory");
                Check(ui.Message == "金幣不夠喔！", "Coin feedback");
                Check(ui.Butler.Message == "再收集一些金幣，我會在這裡等你。", "NPC result integration");
                Check(manager.Wish(WishCurrency.Feathers, out _) == WishResult.InsufficientCurrency && inventory.Feathers == 0, "Insufficient feathers");
                inventory.AddCoins(100); inventory.AddFeathers(10);
                inventory.OnCurrencyChanged += ObserveAtomic;
                Button("FeathersWishButton").onClick.Invoke(); Button("CoinsWishButton").onClick.Invoke();
                Check(ui.IsBusy && !Button("CoinsWishButton").interactable, "Double click guard"); stage = 2;
            }
            else if (stage == 2 && !ui.IsBusy)
            {
                inventory.OnCurrencyChanged -= ObserveAtomic;
                var reward = ui.DisplayedItem;
                Check(ui.Butler.Message == "✨ 聽到了！你的願望實現啦！", "NPC success integration");
                Check(ui.LastResult == WishResult.Success && reward != null && reward.itemType == ItemType.Costume, "Costume result");
                Check(inventory.Feathers == 0 && inventory.Coins == 100 && inventory.OwnsItem(reward) && atomicObserved, "Feather atomic debit/unlock");
                Check(!inventory.TryUnlockFromWish(reward, 100, 0) && inventory.Coins == 100, "Duplicate transaction debited");
                Button("RevealPanel/EquipButton").onClick.Invoke();
                Check(inventory.GetEquippedItemId(ItemType.Costume) == reward.itemId, "Reveal equipment");
                var reread = LoadInventory(Key);
                Check(reread.OwnsItem(reward) && reread.Feathers == 0 && reread.Coins == 100 && reread.GetEquippedItemId(ItemType.Costume) == reward.itemId, "Persisted ownership/equipment");
                Object.Destroy(reread.gameObject);
                VerifyCoinsAndConfiguration();
                Capture();
                var catalog = Resources.LoadAll<ItemDataSO>("Items").Where(x => x != null && x.HasValidId && x.itemType == ItemType.Costume).ToArray();
                foreach (var item in catalog) if (!inventory.OwnsItem(item)) Check(inventory.TryUnlockFromWish(item, 0, 0), "Collect remainder");
                int balance = inventory.Coins;
                Check(manager.Wish(WishCurrency.Coins, out var none) == WishResult.AllCollected && none == null && inventory.Coins == balance, "AllCollected must not charge");
                manager.SetTheme(WishTheme.Santa); ui.Close(); ui.Open();
                Check(ui.Butler.Message == "🎅 聖誕老公公正在準備禮物，我會幫你留意消息！", "NPC Santa integration");
                Check(ui.transform.Find("Title").GetComponent<Text>().text.Contains("Santa"), "Santa UI theme");
                Button("CoinsWishButton").onClick.Invoke(); stage = 3;
            }
            else if (stage == 3 && !ui.IsBusy)
            {
                Check(ui.Message == "✨ 目前所有願望都實現了！", "All collected feedback");
                Check(ui.Butler.Message == "你把目前所有願望都實現了！", "NPC all collected integration");
                int coins = inventory.Coins; Button("CoinsWishButton").onClick.Invoke(); ui.Close(); ui.Open();
                Check(!ui.IsBusy && inventory.Coins == coins && Button("CoinsWishButton").interactable, "Cancel waiting/reopen");
                menu.gameObject.SetActive(false); Check(!ui.gameObject.activeSelf, "Close with menu");
                Debug.Log("PHASE5_WISHING_WELL_SMOKE_PASS: UI, atomic currencies, duplicate/all collected, one-item catalog, reload, migration, theme, cancellation");
                Debug.Log("PHASE8B1_PENGUIN_SMOKE_PASS: visible NPC, dialogue, interaction, themes, source replacement, unchanged assets");
                SessionState.SetBool(Key, false); EditorApplication.Exit(0);
            }
        }
        catch (Exception ex) { Debug.LogException(ex); SessionState.SetBool(Key, false); EditorApplication.Exit(1); }
    }
    private sealed class TestMessages : IPenguinMessageSource
    {
        public string GetMessage(PenguinDialogueContext context) => "替換訊息來源";
    }
    static void VerifyPenguin()
    {
        var npc = ui.Butler;
        Check(npc != null && npc.gameObject.activeInHierarchy, "NPC not visible with wishing well");
        Check(npc.Message == "🐧 歡迎來到許願池！要不要許個願？", "NPC welcome");
        string before = JsonUtility.ToJson(inventory.GetSnapshot());
        string saved = PlayerPrefs.GetString(Key);
        npc.TalkButton.onClick.Invoke();
        Check(npc.Message.Contains("選擇金幣或羽毛"), "NPC interaction");
        npc.Present(new PenguinDialogueContext(PenguinCue.Result, WishTheme.WishingWell, WishResult.InsufficientCurrency, WishCurrency.Feathers));
        Check(npc.Message == "羽毛還差一點點喔！", "NPC feathers");
        npc.Present(new PenguinDialogueContext(PenguinCue.Result, WishTheme.WishingWell, WishResult.InvalidConfiguration));
        Check(npc.Message == "許願池暫時需要休息，請稍後再試。", "NPC invalid config");
        foreach (WishResult result in Enum.GetValues(typeof(WishResult)))
            npc.Present(new PenguinDialogueContext(PenguinCue.Result, WishTheme.Santa, result));
        npc.SetMessageSource(new TestMessages()); Check(npc.Message == "替換訊息來源", "Message source extension");
        npc.SetMessageSource(null);
        Check(JsonUtility.ToJson(inventory.GetSnapshot()) == before && PlayerPrefs.GetString(Key) == saved, "NPC changed player assets");
        npc.Present(new PenguinDialogueContext(PenguinCue.Welcome, WishTheme.WishingWell));
    }
    static void ObserveAtomic()
    {
        var saved = JsonUtility.FromJson<PlayerData>(PlayerPrefs.GetString(Key));
        atomicObserved = inventory.Feathers == 0 && inventory.OwnedItemIds.Count == 1 && saved.feathers == 0 && saved.ownedItemIds.Count == 1;
    }
    static void VerifyCoinsAndConfiguration()
    {
        var separate = LoadInventory(Key + ".coins"); var wish = separate.gameObject.AddComponent<WishingWellManager>(); wish.Initialize(separate);
        separate.AddCoins(150); separate.AddFeathers(12);
        Check(wish.Wish(WishCurrency.Coins, out var prize) == WishResult.Success && separate.Coins == 50 && separate.Feathers == 12 && separate.OwnsItem(prize), "Coin wish independent of single-item feather test");
        string snapshot = JsonUtility.ToJson(separate.GetSnapshot());
        Check(wish.Wish((WishCurrency)99, out _) == WishResult.InvalidConfiguration, "Invalid currency");
        Set(wish, "coinPrice", -1); Check(wish.Wish(WishCurrency.Coins, out _) == WishResult.InvalidConfiguration, "Negative price"); Set(wish, "coinPrice", 100);
        Check(!separate.TryUnlockFromWish(prize, -1, 0), "Negative transaction cost");
        var invalid = ScriptableObject.CreateInstance<ItemDataSO>(); invalid.itemType = ItemType.Costume; invalid.itemId = " ";
        Check(!separate.TryUnlockFromWish(invalid, 1, 0), "Malformed ID"); invalid.itemId = "test.non-costume"; invalid.itemType = ItemType.Title;
        Check(!separate.TryUnlockFromWish(invalid, 1, 0), "Non-costume grant"); Object.Destroy(invalid);
        Check(JsonUtility.ToJson(separate.GetSnapshot()) == snapshot, "Invalid config mutated data");
        var neutral = ScriptableObject.CreateInstance<ItemDataSO>(); neutral.itemId = "test.neutral"; neutral.itemType = ItemType.Costume; neutral.price = -1;
        Check(!separate.BuyItem(neutral), "Legacy negative price accepted");
        Check(separate.TryUnlockFromWish(neutral, 0, 0) && separate.EquipItem(neutral), "Wish validity depends on obsolete item price");
        Object.Destroy(neutral); Object.Destroy(separate.gameObject);
        PlayerPrefs.SetString(Key + ".legacy", "{\"coins\":42,\"ownedItemIds\":[\"legacy.hat\"],\"equippedCostumeId\":\"legacy.hat\"}"); PlayerPrefs.Save();
        var legacy = LoadInventory(Key + ".legacy");
        Check(legacy.Coins == 42 && legacy.Feathers == 0 && legacy.OwnedItemIds.Contains("legacy.hat") && legacy.GetEquippedItemId(ItemType.Costume) == "legacy.hat", "Legacy migration lost inventory"); Object.Destroy(legacy.gameObject);
    }
    static void Capture()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
        var canvas = menu.GetComponent<Canvas>(); var camera = Camera.main; var target = new RenderTexture(720, 960, 24);
        camera.targetTexture = target; canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
        Canvas.ForceUpdateCanvases(); camera.Render(); var previous = RenderTexture.active; RenderTexture.active = target;
        var image = new Texture2D(720, 960, TextureFormat.RGB24, false); image.ReadPixels(new Rect(0, 0, 720, 960), 0, 0); image.Apply();
        System.IO.File.WriteAllBytes("Phase5WishingWell.png", image.EncodeToPNG()); RenderTexture.active = previous; camera.targetTexture = null; canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        Object.Destroy(image); Object.Destroy(target);
    }
}
