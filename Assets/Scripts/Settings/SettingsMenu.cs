using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;





public class SettingsMenu : MonoBehaviour
{
    [Tooltip("Player scripts switched off while open (movement, weapons)")]
    [SerializeField] Behaviour[] disableWhileOpen;
    [SerializeField] string mainMenuScene = "MainMenu";

    public bool IsOpen { get; private set; }

    static readonly Color Background = new Color(0.02f, 0.02f, 0.03f, 1f);
    static readonly Color Control = new Color(0.12f, 0.12f, 0.14f, 1f);
    static readonly Color Accent = new Color(1f, 0.82f, 0.25f);

    Font font;
    GameObject panel;
    GameObject openButton;
    PlayerHealth playerHealth;
    float previousTimeScale = 1f;
    
    
    int closedFrame = -1;
    bool resumePending;
    Action refreshControls;

    void Awake()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        playerHealth = FindAnyObjectByType<PlayerHealth>();
        BuildOpenButton();
        BuildPanel();
        panel.SetActive(false);
    }

    void Update()
    {
        if (resumePending && Time.frameCount > closedFrame)
        {
            resumePending = false;
            
            foreach (Behaviour b in disableWhileOpen) if (b != null) b.enabled = true;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            if (IsOpen) Close();
            else TryOpen();
        }
        openButton.SetActive(!IsOpen);
    }

    public void TryOpen()
    {
        
        
        if (IsOpen || Time.timeScale == 0f || (playerHealth != null && playerHealth.IsDead)) return;

        IsOpen = true;
        resumePending = false;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        foreach (Behaviour b in disableWhileOpen) if (b != null) b.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        refreshControls?.Invoke();
        panel.SetActive(true);
    }

    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        panel.SetActive(false);
        GameSettings.Save();
        Time.timeScale = previousTimeScale;
        AudioListener.pause = false;
        closedFrame = Time.frameCount;
        resumePending = true;
    }

    void QuitToMenu()
    {
        GameSettings.Save();
        RunState.Reset();
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(mainMenuScene);
    }

    

    void BuildOpenButton()
    {
        Button button = MakeButton(transform, "PAUSE  [ESC]", 20, new Vector2(200f, 42f), out RectTransform rect);
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one;
        
        rect.anchoredPosition = new Vector2(-290f, -30f);
        button.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
        button.onClick.AddListener(TryOpen);
        openButton = button.gameObject;
    }

    void BuildPanel()
    {
        panel = new GameObject("Settings Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);
        var rect = (RectTransform)panel.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        
        panel.GetComponent<Image>().color = Background;

        Label(panel.transform, "PAUSED", 72, Color.white, new Vector2(0f, 330f), new Vector2(800f, 90f), TextAnchor.MiddleCenter);

        float y = 220f;
        Slider look = SliderRow("Look sensitivity", 1f, v => GameSettings.LookSensitivity = SensitivityFromSlider(v), ref y);
        Toggle invert = ToggleRow("Invert look up / down", v => GameSettings.InvertY = v, ref y);
        
        Slider master = SliderRow("Master volume", 0f, v => GameSettings.MasterVolume = v / 100f, ref y);
        Slider music = SliderRow("Music volume", 0f, v => GameSettings.MusicVolume = v / 100f, ref y);
        Slider sfx = SliderRow("Sound effects volume", 0f, v => GameSettings.SfxVolume = v / 100f, ref y);

        
        void Show(Slider s, float v)
        {
            s.SetValueWithoutNotify(v);
            s.onValueChanged.Invoke(s.value);
        }
        refreshControls = () =>
        {
            Show(look, SensitivityToSlider(GameSettings.LookSensitivity));
            invert.SetIsOnWithoutNotify(GameSettings.InvertY);
            Show(master, GameSettings.MasterVolume * 100f);
            Show(music, GameSettings.MusicVolume * 100f);
            Show(sfx, GameSettings.SfxVolume * 100f);
        };

        y -= 40f;
        Button resume = MakeButton(panel.transform, "RESUME", 34, new Vector2(360f, 70f), out RectTransform resumeRect);
        resumeRect.anchoredPosition = new Vector2(0f, y);
        resume.GetComponent<Image>().color = new Color(0.12f, 0.32f, 0.18f, 1f);
        resume.onClick.AddListener(Close);

        Button quit = MakeButton(panel.transform, "QUIT TO MAIN MENU", 24, new Vector2(360f, 54f), out RectTransform quitRect);
        quitRect.anchoredPosition = new Vector2(0f, y - 80f);
        quit.GetComponent<Image>().color = new Color(0.32f, 0.1f, 0.1f, 1f);
        quit.onClick.AddListener(QuitToMenu);
    }

    
    
    static float SensitivityFromSlider(float v) => v <= 50f ? Mathf.Lerp(0.1f, 1f, (v - 1f) / 49f) : Mathf.Lerp(1f, 3f, (v - 50f) / 50f);
    static float SensitivityToSlider(float m) => m <= 1f ? 1f + Mathf.InverseLerp(0.1f, 1f, m) * 49f : 50f + Mathf.InverseLerp(1f, 3f, m) * 50f;

    Slider SliderRow(string label, float min, Action<float> onChange, ref float y)
    {
        const float max = 100f;
        Label(panel.transform, label, 24, Color.white, new Vector2(-300f, y), new Vector2(560f, 34f), TextAnchor.MiddleLeft);
        Slider slider = MakeSlider(panel.transform, new Vector2(220f, y), new Vector2(420f, 24f));
        
        Text value = Label(panel.transform, "", 22, new Color(0.8f, 0.8f, 0.85f), new Vector2(520f, y), new Vector2(140f, 34f), TextAnchor.MiddleLeft);
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = true;
        slider.onValueChanged.AddListener(v =>
        {
            onChange(v);
            value.text = $"{v:0}";
        });
        y -= 54f;
        return slider;
    }

    Toggle ToggleRow(string label, Action<bool> onChange, ref float y)
    {
        Label(panel.transform, label, 24, Color.white, new Vector2(-300f, y), new Vector2(560f, 34f), TextAnchor.MiddleLeft);
        Toggle toggle = MakeToggle(panel.transform, new Vector2(30f, y));
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

    Button MakeButton(Transform parent, string text, int fontSize, Vector2 size, out RectTransform rect)
    {
        var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        rect = (RectTransform)go.transform;
        rect.sizeDelta = size;
        go.GetComponent<Image>().color = Control;
        var button = go.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1.4f, 1.4f, 1.4f, 1f);
        button.colors = colors;
        Text label = Label(go.transform, text, fontSize, Color.white, Vector2.zero, size, TextAnchor.MiddleCenter);
        label.raycastTarget = false;
        return button;
    }

    Text Label(Transform parent, string text, int size, Color color, Vector2 position, Vector2 box, TextAnchor alignment)
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
