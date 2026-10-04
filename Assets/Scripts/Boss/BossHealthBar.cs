using UnityEngine;
using UnityEngine.UI;



public class BossHealthBar : MonoBehaviour
{
    const float Width = 900f;

    BossZombie boss;
    CanvasGroup group;
    RectTransform fill;
    RectTransform lag;
    Text title;
    float shown;
    float lagFraction = 1f;

    public static BossHealthBar Create(BossZombie boss)
    {
        Canvas canvas = RuntimeUI.Canvas("Boss Health Bar", 5);
        var bar = canvas.gameObject.AddComponent<BossHealthBar>();
        bar.Build(canvas.transform, boss);
        return bar;
    }

    void Build(Transform root, BossZombie target)
    {
        boss = target;
        group = root.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;

        var top = new Vector2(0.5f, 1f);
        title = RuntimeUI.Text(root, "Name", boss.DisplayName, 30, new Color(0.85f, 1f, 0.6f), top, new Vector2(0f, -40f), new Vector2(Width, 40f));
        Image back = RuntimeUI.Image(root, "Bar", new Color(0f, 0f, 0f, 0.7f), top, new Vector2(0f, -78f), new Vector2(Width, 26f));
        lag = RuntimeUI.Image(back.transform, "Lag", new Color(1f, 0.85f, 0.75f, 0.8f), new Vector2(0f, 0.5f), new Vector2(3f, 0f), new Vector2(Width - 6f, 20f)).rectTransform;
        fill = RuntimeUI.Image(back.transform, "Fill", new Color(0.55f, 0.85f, 0.2f), new Vector2(0f, 0.5f), new Vector2(3f, 0f), new Vector2(Width - 6f, 20f)).rectTransform;
    }

    void Update()
    {
        if (boss == null || boss.Health.IsDead)
        {
            
            shown = Mathf.MoveTowards(shown, 0f, Time.unscaledDeltaTime * 1.5f);
            group.alpha = shown;
            fill.sizeDelta = new Vector2(0f, fill.sizeDelta.y);
            if (shown <= 0f) Destroy(gameObject);
            return;
        }

        shown = Mathf.MoveTowards(shown, boss.Engaged ? 1f : 0f, Time.unscaledDeltaTime * 2f);
        group.alpha = shown;
        float fraction = boss.Health.MaxHealth > 0f ? boss.Health.CurrentHealth / boss.Health.MaxHealth : 0f;
        
        lagFraction = Mathf.Max(fraction, Mathf.MoveTowards(lagFraction, fraction, Time.deltaTime * 0.35f));
        float full = Width - 6f;
        fill.sizeDelta = new Vector2(full * fraction, fill.sizeDelta.y);
        lag.sizeDelta = new Vector2(full * lagFraction, lag.sizeDelta.y);
        title.text = boss.Enraged ? boss.DisplayName + "  (ENRAGED)" : boss.DisplayName;
        title.color = boss.Enraged ? new Color(1f, 0.4f, 0.3f) : new Color(0.85f, 1f, 0.6f);
    }
}
