using UnityEngine;



public class FireFlicker : MonoBehaviour
{
    [Tooltip("Leave empty to use every light in children")]
    [SerializeField] Light[] lights;
    [SerializeField] Transform[] flames;
    [SerializeField] float speed = 7f;
    [Range(0f, 1f)] [SerializeField] float lightVariation = 0.35f;
    [Tooltip("How much the flames stretch up and down")]
    [SerializeField] float flameStretch = 0.3f;

    float[] baseIntensities;
    Vector3[] baseScales;
    float seed;

    void Awake()
    {
        if (lights == null || lights.Length == 0) lights = GetComponentsInChildren<Light>();
        baseIntensities = new float[lights.Length];
        for (int i = 0; i < lights.Length; i++) baseIntensities[i] = lights[i].intensity;
        baseScales = new Vector3[flames.Length];
        for (int i = 0; i < flames.Length; i++) baseScales[i] = flames[i] != null ? flames[i].localScale : Vector3.one;
        seed = Random.Range(0f, 100f);
    }

    void Update()
    {
        float t = Time.time * speed;
        
        float n = Mathf.PerlinNoise(t * 0.35f, seed) * 0.65f + Mathf.PerlinNoise(t * 1.7f, seed + 7f) * 0.35f;
        float level = 1f - lightVariation + n * lightVariation * 2f;
        for (int i = 0; i < lights.Length; i++)
            if (lights[i] != null) lights[i].intensity = baseIntensities[i] * level;

        for (int i = 0; i < flames.Length; i++)
        {
            if (flames[i] == null) continue;
            float f = Mathf.PerlinNoise(t * 1.3f + i * 3.1f, seed + i) - 0.5f;
            float g = Mathf.PerlinNoise(t * 2.1f + i * 1.7f, seed - i) - 0.5f;
            Vector3 s = baseScales[i];
            flames[i].localScale = new Vector3(s.x * (1f - f * flameStretch * 0.5f), s.y * (1f + f * flameStretch * 2f), s.z * (1f + g * flameStretch * 0.4f));
        }
    }
}
