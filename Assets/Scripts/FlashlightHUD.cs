using UnityEngine;
using UnityEngine.UI;


public class FlashlightHUD : MonoBehaviour
{
    [SerializeField] Flashlight flashlight;

    [Header("Meter")]
    [SerializeField] RectTransform fill;
    [SerializeField] Image fillImage;
    [SerializeField] Text label;
    [SerializeField] Gradient colorByCharge;
    [SerializeField] float lowBlinkSpeed = 4f;

    [Header("Pickup Popup")]
    [SerializeField] Text popup;
    [SerializeField] float popupDuration = 2f;
    [SerializeField] float popupRise = 30f;

    int lastPercent = -1;
    float popupStartTime = float.NegativeInfinity;
    Vector2 popupStartPosition;
    Color popupColor;

    void Awake()
    {
        if (popup != null)
        {
            popupStartPosition = popup.rectTransform.anchoredPosition;
            popupColor = popup.color;
            popup.enabled = false;
        }
    }

    void OnEnable()
    {
        if (flashlight != null) flashlight.BatteryAdded += ShowPopup;
    }

    void OnDisable()
    {
        if (flashlight != null) flashlight.BatteryAdded -= ShowPopup;
    }

    void Update()
    {
        if (flashlight == null) return;

        float charge = flashlight.Charge01;
        fill.anchorMax = new Vector2(charge, fill.anchorMax.y);

        Color color = colorByCharge.Evaluate(charge);
        if (flashlight.IsLow) color.a = Mathf.Lerp(0.35f, 1f, Mathf.PingPong(Time.time * lowBlinkSpeed, 1f));
        fillImage.color = color;

        int percent = Mathf.CeilToInt(charge * 100f);
        if (percent != lastPercent)
        {
            lastPercent = percent;
            label.text = $"FLASHLIGHT  {percent}%";
        }

        UpdatePopup();
    }

    void ShowPopup(float amount)
    {
        if (popup == null) return;
        popup.text = $"+{Mathf.RoundToInt(amount)} flashlight battery";
        popupStartTime = Time.time;
        popup.enabled = true;
    }

    void UpdatePopup()
    {
        if (popup == null || !popup.enabled) return;

        float t = (Time.time - popupStartTime) / popupDuration;
        if (t >= 1f)
        {
            popup.enabled = false;
            return;
        }

        popup.rectTransform.anchoredPosition = popupStartPosition + Vector2.up * popupRise * t;
        Color c = popupColor;
        c.a = 1f - t * t;
        popup.color = c;
    }
}
