using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;




public class MainMenu : MonoBehaviour
{
    [Tooltip("First scene of a run: the helicopter intro, which drops the player into Level_01")]
    [SerializeField] string gameScene = "Intro";

    static readonly Color Panel = new Color(0.03f, 0.03f, 0.04f, 0.96f);
    static readonly Color Control = new Color(0.12f, 0.12f, 0.14f, 0.92f);
    static readonly Color Accent = new Color(1f, 0.82f, 0.25f);
    static readonly Color PlayColor = new Color(0.14f, 0.3f, 0.18f, 0.92f);

    Font font;
    GameObject settingsPanel;
    Action refreshSettings;

    void Awake()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildMenu();
    }

    void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void StartGame()
    {
        RunState.Reset();
        SceneManager.LoadScene(gameScene);
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    

    void BuildMenu()
    {
        var canvasGo = new GameObject("Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        Transform root = canvasGo.transform;

        Label(root, "THE LAST CHARGE", 92, Color.white, new Vector2(0f, 300f), new Vector2(1400f, 120f), TextAnchor.MiddleCenter, FontStyle.Bold);
        Label(root, "Keep the light alive. Escape the floors below.", 26, new Color(0.75f, 0.76f, 0.8f),
            new Vector2(0f, 238f), new Vector2(1200f, 40f), TextAnchor.MiddleCenter);

        Button play = MakeButton(root, "PLAY", 40, new Vector2(520f, 92f), out RectTransform playRect, PlayColor);
        playRect.anchoredPosition = new Vector2(0f, 90f);
        play.onClick.AddListener(StartGame);

        SmallButton(root, "SETTINGS", Control, new Vector2(0f, -20f), () => Show(settingsPanel));
        SmallButton(root, "QUIT", new Color(0.2f, 0.06f, 0.06f, 0.92f), new Vector2(0f, -84f), Quit);

        BuildSettingsPanel(root);
    }

    void SmallButton(Transform parent, string text, Color color, Vector2 position, Action onClick)
    {
        Button button = MakeButton(parent, text, 26, new Vector2(520f, 54f), out RectTransform rect, color);
        rect.anchoredPosition = position;
        button.onClick.AddListener(() => onClick());
    }

    void BuildSettingsPanel(Transform parent)
    {
        settingsPanel = NewPanel(parent, "Settings Panel");
        Label(settingsPanel.transform, "SETTINGS", 58, Color.white, new Vector2(0f, 340f), new Vector2(800f, 74f), TextAnchor.MiddleCenter, FontStyle.Bold);

        float y = 230f;
        Header("AUDIO", ref y);
        Slider master = SliderRow("Master volume", 0f, v => GameSettings.MasterVolume = v / 100f, ref y);
        Slider music = SliderRow("Music volume", 0f, v => GameSettings.MusicVolume = v / 100f, ref y);
        Slider sfx = SliderRow("Sound effects volume", 0f, v => GameSettings.SfxVolume = v / 100f, ref y);
        y -= 20f;
        Header("CONTROLS", ref y);
        Slider look = SliderRow("Look sensitivity", 1f, v => GameSettings.LookSensitivity = SensitivityFromSlider(v), ref y);
        Toggle invert = ToggleRow("Invert look up / down", v => GameSettings.InvertY = v, ref y);

        void Show(Slider s, float v) { s.SetValueWithoutNotify(v); s.onValueChanged.Invoke(s.value); }
        refreshSettings = () =>
        {
            Show(master, GameSettings.MasterVolume * 100f);
            Show(music, GameSettings.MusicVolume * 100f);
            Show(sfx, GameSettings.SfxVolume * 100f);
            Show(look, SensitivityToSlider(GameSettings.LookSensitivity));
            invert.SetIsOnWithoutNotify(GameSettings.InvertY);
        };

        Button reset = MakeButton(settingsPanel.transform, "RESET TO DEFAULTS", 22, new Vector2(300f, 54f), out RectTransform resetRect, Control);
        resetRect.anchoredPosition = new Vector2(-180f, -300f);
        reset.onClick.AddListener(() => { GameSettings.ResetToDefaults(); refreshSettings(); });

        Button back = MakeButton(settingsPanel.transform, "BACK", 28, new Vector2(300f, 54f), out RectTransform backRect, PlayColor);
        backRect.anchoredPosition = new Vector2(180f, -300f);
        back.onClick.AddListener(() => { GameSettings.Save(); Hide(settingsPanel); });

        settingsPanel.SetActive(false);
    }

    void Show(GameObject panel)
    {
        if (panel == settingsPanel) refreshSettings?.Invoke();
        panel.SetActive(true);
        panel.transform.SetAsLastSibling();
    }

    void Hide(GameObject panel) => panel.SetActive(false);

    GameObject NewPanel(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = Panel;
        return go;
    }

    void Header(string text, ref float y)
    {
        Label(settingsPanel.transform, text, 24, Accent, new Vector2(-150f, y), new Vector2(900f, 34f), TextAnchor.MiddleLeft);
        y -= 46f;
    }

    static float SensitivityFromSlider(float v) => v <= 50f ? Mathf.Lerp(0.1f, 1f, (v - 1f) / 49f) : Mathf.Lerp(1f, 3f, (v - 50f) / 50f);
    static float SensitivityToSlider(float m) => m <= 1f ? 1f + Mathf.InverseLerp(0.1f, 1f, m) * 49f : 50f + Mathf.InverseLerp(1f, 3f, m) * 50f;

    Slider SliderRow(string label, float min, Action<float> onChange, ref float y)
    {
        Label(settingsPanel.transform, label, 24, Color.white, new Vector2(-300f, y), new Vector2(560f, 34f), TextAnchor.MiddleLeft);
        Slider slider = MakeSlider(settingsPanel.transform, new Vector2(220f, y), new Vector2(420f, 24f));
        Text value = Label(settingsPanel.transform, "", 22, new Color(0.8f, 0.8f, 0.85f), new Vector2(520f, y), new Vector2(140f, 34f), TextAnchor.MiddleLeft);
        slider.minValue = min;
        slider.maxValue = 100f;
        slider.wholeNumbers = true;
        slider.onValueChanged.AddListener(v => { onChange(v); value.text = $"{v:0}"; });
        y -= 54f;
        return slider;
    }

    Toggle ToggleRow(string label, Action<bool> onChange, ref float y)
    {
        Label(settingsPanel.transform, label, 24, Color.white, new Vector2(-300f, y), new Vector2(560f, 34f), TextAnchor.MiddleLeft);
        Toggle toggle = MakeToggle(settingsPanel.transform, new Vector2(30f, y));
        toggle.onValueChanged.AddListener(v => onChange(v));
        y -= 54f;
        return toggle;
    }

    Slider MakeSlider(Transform parent, Vector2 position, Vector2 size)
    {
        var root = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
        root.transform.SetParent(parent, false);
        var rect = (RectTransform)root.transform;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image background = Child(root.transform, "Background", Control);
        Stretch(background.rectTransform, Vector2.zero, Vector2.zero);

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(root.transform, false);
        Stretch((RectTransform)fillArea.transform, new Vector2(4f, 4f), new Vector2(-4f, -4f));
        Image fill = Child(fillArea.transform, "Fill", Accent);
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = new Vector2(0f, 1f);
        fill.rectTransform.sizeDelta = Vector2.zero;

        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(root.transform, false);
        Stretch((RectTransform)handleArea.transform, new Vector2(10f, 0f), new Vector2(-10f, 0f));
        Image handle = Child(handleArea.transform, "Handle", Color.white);
        handle.rectTransform.anchorMin = Vector2.zero;
        handle.rectTransform.anchorMax = new Vector2(0f, 1f);
        handle.rectTransform.sizeDelta = new Vector2(18f, 12f);

        var slider = root.GetComponent<Slider>();
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;
        return slider;
    }

    Toggle MakeToggle(Transform parent, Vector2 position)
    {
        var root = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle));
        root.transform.SetParent(parent, false);
        var rect = (RectTransform)root.transform;
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(34f, 34f);
        Image box = Child(root.transform, "Box", Control);
        Stretch(box.rectTransform, Vector2.zero, Vector2.zero);
        Image check = Child(box.transform, "Check", Accent);
        Stretch(check.rectTransform, new Vector2(7f, 7f), new Vector2(-7f, -7f));
        var toggle = root.GetComponent<Toggle>();
        toggle.targetGraphic = box;
        toggle.graphic = check;
        return toggle;
    }

    Image Child(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        return image;
    }

    Button MakeButton(Transform parent, string text, int fontSize, Vector2 size, out RectTransform rect, Color color)
    {
        var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        rect = (RectTransform)go.transform;
        rect.sizeDelta = size;
        go.GetComponent<Image>().color = color;
        var button = go.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f);
        colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        button.colors = colors;
        if (!string.IsNullOrEmpty(text))
        {
            Text label = Label(go.transform, text, fontSize, Color.white, Vector2.zero, size, TextAnchor.MiddleCenter, FontStyle.Bold);
            label.raycastTarget = false;
        }
        return button;
    }

    Text Label(Transform parent, string text, int size, Color color, Vector2 position, Vector2 box, TextAnchor alignment, FontStyle style = FontStyle.Normal)
    {
        var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.anchoredPosition = position;
        rect.sizeDelta = box;
        var t = go.GetComponent<Text>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = alignment;
        t.fontStyle = style;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}
