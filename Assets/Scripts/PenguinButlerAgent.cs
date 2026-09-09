using UnityEngine;
using UnityEngine.UI;

namespace ChickenRush
{
    public enum PenguinCue { Welcome, Interaction, Waiting, Result }

    // Presentation-only snapshot: deliberately contains no manager or mutable player data.
    public readonly struct PenguinDialogueContext
    {
        public readonly PenguinCue Cue;
        public readonly WishTheme Theme;
        public readonly WishResult Result;
        public readonly WishCurrency Currency;
        public PenguinDialogueContext(PenguinCue cue, WishTheme theme, WishResult result = WishResult.Success, WishCurrency currency = WishCurrency.Coins)
        { Cue = cue; Theme = theme; Result = result; Currency = currency; }
    }

    public interface IPenguinMessageSource
    {
        string GetMessage(PenguinDialogueContext context);
    }

    public sealed class RuleBasedPenguinMessages : IPenguinMessageSource
    {
        public string GetMessage(PenguinDialogueContext context)
        {
            if (context.Cue == PenguinCue.Result)
            {
                switch (context.Result)
                {
                    case WishResult.Success: return "✨ 聽到了！你的願望實現啦！";
                    case WishResult.AllCollected: return "你把目前所有願望都實現了！";
                    case WishResult.InsufficientCurrency: return context.Currency == WishCurrency.Coins
                        ? "再收集一些金幣，我會在這裡等你。" : "羽毛還差一點點喔！";
                    default: return "許願池暫時需要休息，請稍後再試。";
                }
            }
            if (context.Cue == PenguinCue.Waiting) return "我陪你一起等待願望實現！";
            if (context.Theme == WishTheme.Santa) return "🎅 聖誕老公公正在準備禮物，我會幫你留意消息！";
            return context.Cue == PenguinCue.Interaction ? "選擇金幣或羽毛許願；揭曉後可以裝備飾品喔！"
                : "🐧 歡迎來到許願池！要不要許個願？";
        }
    }

    public sealed class PenguinButlerAgent : MonoBehaviour
    {
        private IPenguinMessageSource source = new RuleBasedPenguinMessages();
        private PenguinDialogueContext context;
        private Text bubble;
        public string Message => bubble != null ? bubble.text : "";
        public Button TalkButton { get; private set; }
        public void SetMessageSource(IPenguinMessageSource replacement)
        { source = replacement ?? new RuleBasedPenguinMessages(); Present(context); }
        public void Present(PenguinDialogueContext state)
        { context = state; if (bubble != null) bubble.text = source.GetMessage(state); }

        public void Initialize(Font font, Sprite portrait = null)
        {
            TalkButton = RuntimeUI.Button("TalkButton", transform, font, "", new Vector2(82, 104), new Vector2(-265, 0));
            TalkButton.GetComponent<Image>().color = new Color(.08f, .12f, .18f);
            if (portrait != null)
            {
                var image = Part("Portrait", TalkButton.transform, new Vector2(78, 98), Vector2.zero, Color.white);
                image.sprite = portrait; image.preserveAspect = true;
            }
            else
            {
                // Replaceable geometric penguin placeholder; no external assets required.
                Part("Belly", TalkButton.transform, new Vector2(52, 60), new Vector2(0, -8), Color.white);
                Part("LeftEye", TalkButton.transform, new Vector2(10, 12), new Vector2(-17, 31), Color.white);
                Part("RightEye", TalkButton.transform, new Vector2(10, 12), new Vector2(17, 31), Color.white);
                Part("Beak", TalkButton.transform, new Vector2(18, 10), new Vector2(0, 18), new Color(1, .65f, .12f));
                Part("LeftFoot", TalkButton.transform, new Vector2(23, 9), new Vector2(-17, -45), new Color(1, .65f, .12f));
                Part("RightFoot", TalkButton.transform, new Vector2(23, 9), new Vector2(17, -45), new Color(1, .65f, .12f));
            }
            var panel = RuntimeUI.Rect("DialogueBubble", transform, new Vector2(490, 110), new Vector2(40, 0));
            panel.gameObject.AddComponent<Image>().color = new Color(.13f, .24f, .3f);
            bubble = RuntimeUI.Label("Dialogue", panel, font, "", 23, new Vector2(465, 95), Vector2.zero);
            RuntimeUI.Label("Hint", transform, font, "點企鵝聊聊", 18, new Vector2(180, 28), new Vector2(-245, -69));
            TalkButton.onClick.AddListener(() => Present(new PenguinDialogueContext(PenguinCue.Interaction, context.Theme)));
        }
        private static Image Part(string name, Transform parent, Vector2 size, Vector2 position, Color color)
        {
            var image = RuntimeUI.Rect(name, parent, size, position).gameObject.AddComponent<Image>();
            image.color = color; image.raycastTarget = false; return image;
        }
    }
}
