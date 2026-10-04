using UnityEngine;
using UnityEngine.UI;


public class FloatingHealthBar : MonoBehaviour
{
    [SerializeField] EnemyHealth health;
    [SerializeField] RectTransform fill;
    [SerializeField] Image fillImage;
    [SerializeField] Gradient colorByHealth;

    Camera cam;

    void OnEnable()
    {
        if (health != null) health.HealthChanged += Refresh;
    }

    void OnDisable()
    {
        if (health != null) health.HealthChanged -= Refresh;
    }

    void LateUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (cam != null) transform.rotation = cam.transform.rotation;
    }

    void Refresh(float current, float max)
    {
        float t = max > 0f ? current / max : 0f;
        fill.anchorMax = new Vector2(t, fill.anchorMax.y);
        if (fillImage != null) fillImage.color = colorByHealth.Evaluate(t);
    }
}
