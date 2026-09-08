using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace ChickenRush
{
    [RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
    [RequireComponent(typeof(WishingWellManager))]
    public sealed class MainMenuCanvas : MonoBehaviour
    {
        [SerializeField] private GameDifficultyManager difficultyManager;
        [SerializeField] private Font chineseFont;
        private Font runtimeFont;
        [SerializeField] private InventoryManager inventoryManager;
        private Text coinsLabel;
        private Text feathersLabel;
        private WishingWellCanvas wishingWell;

        private void Awake()
        {
            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(720, 960);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            if (EventSystem.current == null)
            {
                var events = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
                events.AddComponent<InputSystemUIInputModule>();
#else
                events.AddComponent<StandaloneInputModule>();
#endif
            }
            runtimeFont = chineseFont != null ? chineseFont : Font.CreateDynamicFontFromOSFont(
                new[] { "Microsoft JhengHei", "PingFang TC", "Noto Sans CJK TC", "Arial Unicode MS" }, 48);
            var background = Element("Background", transform, Vector2.zero, Vector2.zero);
            background.anchorMin = Vector2.zero; background.anchorMax = Vector2.one;
            background.offsetMin = background.offsetMax = Vector2.zero;
            background.gameObject.AddComponent<Image>().color = new Color(0.045f, 0.09f, 0.14f, 0.98f);
            coinsLabel = Label("CoinsLabel", transform, "", 28, Vector2.zero, new Vector2(340, 56), new Color(1f, 0.82f, 0.25f));
            var coinRect = coinsLabel.rectTransform;
            coinRect.anchorMin = coinRect.anchorMax = coinRect.pivot = Vector2.one;
            coinRect.anchoredPosition = new Vector2(-24, -24);
            coinsLabel.alignment = TextAnchor.MiddleRight;
            RefreshCoins();
            feathersLabel = Label("FeathersLabel", transform, "", 28, Vector2.zero, new Vector2(300, 56), Color.white);
            feathersLabel.rectTransform.anchorMin = feathersLabel.rectTransform.anchorMax = feathersLabel.rectTransform.pivot = new Vector2(0, 1);
            feathersLabel.rectTransform.anchoredPosition = new Vector2(24, -24);
            feathersLabel.alignment = TextAnchor.MiddleLeft;
            RefreshCoins();
            var panel = Element("Menu", transform, new Vector2(600, 760), Vector2.zero);
            Label("Title", panel, "小雞衝衝衝", 58, new Vector2(0, 290), new Vector2(580, 90), new Color(1f, 0.82f, 0.25f));
            Label("Subtitle", panel, "選擇難度，開始接住小雞！", 26, new Vector2(0, 208), new Vector2(580, 60), Color.white);
            string[] names = { "悠閒", "地獄", "惡魔", "變態" };
            Color[] colors = { new Color(0.22f, 0.58f, 0.43f), new Color(0.68f, 0.39f, 0.17f),
                new Color(0.48f, 0.3f, 0.65f), new Color(0.73f, 0.23f, 0.3f) };
            for (int i = 0; i < names.Length; i++)
            {
                var difficulty = (GameDifficulty)i;
                var rect = Element(names[i] + " Button", panel, new Vector2(500, 88), new Vector2(0, 102 - 110 * i));
                var image = rect.gameObject.AddComponent<Image>(); image.color = colors[i];
                var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
                button.onClick.AddListener(() => difficultyManager.SelectDifficulty(difficulty));
                Label("Label", rect, names[i], 34, Vector2.zero, rect.sizeDelta, Color.white);
            }
            var wishingWellButton = RuntimeUI.Button("WishButton", panel, runtimeFont, "✨ 許願池", new Vector2(500, 66), new Vector2(0, -319));
            var wishingWellObject = new GameObject("WishingWellCanvas", typeof(RectTransform));
            wishingWellObject.SetActive(false);
            wishingWellObject.transform.SetParent(transform, false);
            wishingWell = wishingWellObject.AddComponent<WishingWellCanvas>();
            var wishManager = GetComponent<WishingWellManager>();
            if (wishManager == null) wishManager = gameObject.AddComponent<WishingWellManager>();
            wishManager.Initialize(inventoryManager);
            wishingWell.Initialize(inventoryManager, wishManager, runtimeFont);
            wishingWellButton.onClick.AddListener(wishingWell.Open);
            Label("Hint", panel, "按住螢幕落雞 · 左右拖動調整方向", 23,
                new Vector2(0, -382), new Vector2(600, 55), new Color(0.7f, 0.8f, 0.87f));
        }

        private static RectTransform Element(string name, Transform parent, Vector2 size, Vector2 position)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size; rect.anchoredPosition = position;
            return rect;
        }

        private Text Label(string name, Transform parent, string value, int size, Vector2 position, Vector2 dimensions, Color color)
        {
            var text = Element(name, parent, dimensions, position).gameObject.AddComponent<Text>();
            text.font = runtimeFont; text.text = value; text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter; text.color = color; text.raycastTarget = false;
            return text;
        }

        private void OnEnable()
        {
            if (inventoryManager != null) inventoryManager.OnCurrencyChanged += RefreshCoins;
            RefreshCoins();
        }

        private void OnDisable()
        {
            if (wishingWell != null) wishingWell.Close();
            if (inventoryManager != null) inventoryManager.OnCurrencyChanged -= RefreshCoins;
        }

        private void RefreshCoins()
        {
            if (feathersLabel != null) feathersLabel.text = "羽毛  " + (inventoryManager != null ? inventoryManager.Feathers : 0);
            if (coinsLabel != null) coinsLabel.text = "金幣  " + (inventoryManager != null ? inventoryManager.Coins : 0);
        }

        private void OnDestroy()
        {
            if (chineseFont == null && runtimeFont != null) Destroy(runtimeFont);
        }
    }
}
