using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] RectTransform fill;
    [SerializeField] Image fillImage;
    [SerializeField] Text label;
    [SerializeField] Gradient colorByHealth;

    void OnEnable()
    {
        if (playerHealth != null) playerHealth.HealthChanged += Refresh;
    }

    void OnDisable()
    {
        if (playerHealth != null) playerHealth.HealthChanged -= Refresh;
    }

    void Refresh(float current, float max)
    {
        float t = max > 0f ? current / max : 0f;

        
        fill.anchorMax = new Vector2(t, fill.anchorMax.y);
        if (fillImage != null) fillImage.color = colorByHealth.Evaluate(t);
        if (label != null) label.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }
}
