using UnityEngine;
using UnityEngine.UI;



public static class RuntimeUI
{
    static Font font;

    public static Font Font => font != null ? font : font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    public static Canvas Canvas(string name, int sortingOrder)
    {
        var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    public static RectTransform Rect(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        return rt;
    }

    public static RectTransform Stretch(Transform parent, string name)
    {
        RectTransform rt = Rect(parent, name, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        return rt;
    }

    public static Image Image(Transform parent, string name, Color color, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var image = Rect(parent, name, anchor, position, size).gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    public static Text Text(Transform parent, string name, string text, int fontSize, Color color, Vector2 anchor, Vector2 position, Vector2 size,
        TextAnchor alignment = TextAnchor.MiddleCenter)
    {
        RectTransform rt = Rect(parent, name, anchor, position, size);
        var label = rt.gameObject.AddComponent<Text>();
        label.font = Font;
        label.text = text;
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = alignment;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.raycastTarget = false;
        var shadow = rt.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        shadow.effectDistance = new Vector2(2f, -2f);
        return label;
    }

    public static Button Button(Transform parent, string name, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        Image background = Image(parent, name, new Color(0.12f, 0.1f, 0.1f, 0.95f), new Vector2(0.5f, 0.5f), position, size);
        background.raycastTarget = true;
        var button = background.gameObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1f, 0.75f, 0.55f);
        colors.pressedColor = new Color(0.8f, 0.5f, 0.3f);
        button.colors = colors;
        button.onClick.AddListener(onClick);
        Text text = Text(background.transform, "Label", label, 28, new Color(0.95f, 0.9f, 0.85f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.sizeDelta = Vector2.zero;
        return button;
    }
}
