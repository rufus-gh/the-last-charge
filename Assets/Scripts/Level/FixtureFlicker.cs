using UnityEngine;



public class FixtureFlicker : MonoBehaviour
{
    [Tooltip("Leave empty to use every light in children")]
    [SerializeField] Light[] lights;
    [Tooltip("Leave empty to use every emissive renderer in children")]
    [SerializeField] Renderer[] emissiveRenderers;

    [Tooltip("Seconds the light stays steady between flicker bursts")]
    [SerializeField] Vector2 steadyTime = new Vector2(1.5f, 6f);
    [Tooltip("Seconds each flicker burst lasts")]
    [SerializeField] Vector2 burstTime = new Vector2(0.2f, 1.2f);
    [SerializeField] Vector2 blinkInterval = new Vector2(0.03f, 0.12f);
    [Range(0f, 1f)] [SerializeField] float dimLevel = 0.05f;

    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    float[] baseIntensities;
    Color[] baseEmission;
    MaterialPropertyBlock block;
    bool inBurst;
    float burstEnd;
    float nextChange;

    void Awake()
    {
        if (lights == null || lights.Length == 0) lights = GetComponentsInChildren<Light>();
        if (emissiveRenderers == null || emissiveRenderers.Length == 0)
            emissiveRenderers = System.Array.FindAll(GetComponentsInChildren<Renderer>(),
                r => r.sharedMaterial != null && r.sharedMaterial.IsKeywordEnabled("_EMISSION"));

        baseIntensities = new float[lights.Length];
        for (int i = 0; i < lights.Length; i++) baseIntensities[i] = lights[i].intensity;

        baseEmission = new Color[emissiveRenderers.Length];
        for (int i = 0; i < emissiveRenderers.Length; i++)
            baseEmission[i] = emissiveRenderers[i].sharedMaterial.GetColor(EmissionColorId);

        block = new MaterialPropertyBlock();
        
        nextChange = Time.time + Random.Range(0f, steadyTime.y);
    }

    void OnDisable() => SetLevel(1f);

    void Update()
    {
        if (Time.time < nextChange) return;

        if (!inBurst)
        {
            inBurst = true;
            burstEnd = Time.time + Random.Range(burstTime.x, burstTime.y);
        }

        if (Time.time >= burstEnd)
        {
            inBurst = false;
            SetLevel(1f);
            nextChange = Time.time + Random.Range(steadyTime.x, steadyTime.y);
            return;
        }

        SetLevel(Random.value < 0.5f ? dimLevel : Random.Range(0.4f, 1f));
        nextChange = Time.time + Random.Range(blinkInterval.x, blinkInterval.y);
    }

    void SetLevel(float level)
    {
        if (baseIntensities == null) return;
        
        if (block == null) block = new MaterialPropertyBlock();
        for (int i = 0; i < lights.Length; i++)
            if (lights[i] != null) lights[i].intensity = baseIntensities[i] * level;

        for (int i = 0; i < emissiveRenderers.Length; i++)
        {
            if (emissiveRenderers[i] == null) continue;
            emissiveRenderers[i].GetPropertyBlock(block);
            block.SetColor(EmissionColorId, baseEmission[i] * level);
            emissiveRenderers[i].SetPropertyBlock(block);
        }
    }
}
