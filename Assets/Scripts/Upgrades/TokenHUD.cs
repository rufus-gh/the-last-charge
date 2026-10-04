using UnityEngine;
using UnityEngine.UI;


public class TokenHUD : MonoBehaviour
{
    [SerializeField] Text label;
    [SerializeField] Text popup;
    [SerializeField] float popupDuration = 1.2f;
    [SerializeField] Color earnColor = new Color(1f, 0.82f, 0.25f);

    int shown = -1;
    float popupStart = float.NegativeInfinity;
    Vector2 popupHome;

    void Awake()
    {
        if (popup != null)
        {
            popupHome = popup.rectTransform.anchoredPosition;
            popup.enabled = false;
        }
    }

    void OnEnable()
    {
        RunState.TokensChanged += OnChanged;
        Refresh(RunState.Tokens);
    }

    void OnDisable() => RunState.TokensChanged -= OnChanged;

    void OnChanged(int tokens)
    {
        int gained = shown >= 0 ? tokens - shown : 0;
        Refresh(tokens);
        if (gained <= 0 || popup == null) return;
        popup.text = $"+{gained}";
        popupStart = Time.unscaledTime;
        popup.enabled = true;
    }

    void Refresh(int tokens)
    {
        shown = tokens;
        if (label != null) label.text = $"TOKENS  {tokens}";
    }

    void Update()
    {
        if (popup == null || !popup.enabled) return;
        float t = (Time.unscaledTime - popupStart) / popupDuration;
        if (t >= 1f) { popup.enabled = false; return; }
        popup.rectTransform.anchoredPosition = popupHome + Vector2.up * 24f * t;
        Color c = earnColor;
        c.a = 1f - t * t;
        popup.color = c;
    }
}
