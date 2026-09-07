using UnityEngine;
using UnityEngine.UI;

namespace ChickenRush
{
    // Shared construction helpers for the runtime menu and the editor-built card prefab.
    public static class ShopUI
    {
        public static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 position)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.sizeDelta = size; rect.anchoredPosition = position;
            return rect;
        }
        public static Text Label(string name, Transform parent, Font font, string value, int size, Vector2 dimensions, Vector2 position)
        {
            var label = Rect(name, parent, dimensions, position).gameObject.AddComponent<Text>();
            label.font = font; label.text = value; label.fontSize = size;
            label.alignment = TextAnchor.MiddleCenter; label.color = Color.white; label.raycastTarget = false;
            return label;
        }
        public static Button Button(string name, Transform parent, Font font, string caption, Vector2 size, Vector2 position)
        {
            var rect = Rect(name, parent, size, position);
            var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(.22f, .48f, .4f);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            Label("Label", rect, font, caption, 25, size, Vector2.zero);
            return button;
        }
    }
}
