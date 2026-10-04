using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Graphic))]
public class UIFlicker : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] float minAlpha = 0.7f;
    [SerializeField] float flickerSpeed = 5f;
    [Tooltip("Average dropouts per second")]
    [SerializeField] float dropoutRate = 0.3f;
    [Range(0f, 1f)]
    [SerializeField] float dropoutAlpha = 0.15f;

    Graphic graphic;
    Color baseColor;
    float dropoutUntil;
    float seed;

    void Awake()
    {
        graphic = GetComponent<Graphic>();
        baseColor = graphic.color;
        seed = Random.value * 100f;
    }

    void Update()
    {
        float alpha;
        if (Time.unscaledTime < dropoutUntil)
        {
            alpha = dropoutAlpha;
        }
        else
        {
            if (Random.value < dropoutRate * Time.unscaledDeltaTime)
                dropoutUntil = Time.unscaledTime + Random.Range(0.04f, 0.2f);
            alpha = Mathf.Lerp(minAlpha, 1f, Mathf.PerlinNoise(seed, Time.unscaledTime * flickerSpeed));
        }

        Color c = baseColor;
        c.a *= alpha;
        graphic.color = c;
    }
}
