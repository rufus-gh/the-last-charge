using UnityEngine;




public class MonitorStatic : MonoBehaviour
{
    [SerializeField] Renderer screen;
    [SerializeField] Light glow;
    [Tooltip("Seconds between static frames")]
    [SerializeField] float frameTime = 0.05f;
    [Tooltip("Seconds the picture stays steady between flicker bursts")]
    [SerializeField] Vector2 steadyTime = new Vector2(0.6f, 4f);
    [SerializeField] Vector2 burstTime = new Vector2(0.15f, 0.9f);
    [Tooltip("Brightness wobble while steady")]
    [Range(0f, 1f)] [SerializeField] float steadyJitter = 0.15f;

    static readonly int BaseMapStId = Shader.PropertyToID("_BaseMap_ST");
    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    MaterialPropertyBlock block;
    Vector4 baseSt;
    Color baseEmission;
    float baseIntensity;
    float nextFrame;
    float burstStart;
    float burstEnd;
    float level = 1f;

    void Awake()
    {
        block = new MaterialPropertyBlock();
        if (screen != null && screen.sharedMaterial != null)
        {
            baseSt = screen.sharedMaterial.GetVector(BaseMapStId);
            baseEmission = screen.sharedMaterial.GetColor(EmissionColorId);
        }
        if (glow != null) baseIntensity = glow.intensity;
        
        burstStart = Time.time + Random.Range(0f, steadyTime.y);
        burstEnd = burstStart + Random.Range(burstTime.x, burstTime.y);
    }

    void Update()
    {
        if (Time.time < nextFrame) return;
        nextFrame = Time.time + frameTime * Random.Range(0.7f, 1.4f);

        if (Time.time >= burstEnd)
        {
            burstStart = Time.time + Random.Range(steadyTime.x, steadyTime.y);
            burstEnd = burstStart + Random.Range(burstTime.x, burstTime.y);
        }
        bool bursting = Time.time >= burstStart;
        level = bursting ? (Random.value < 0.45f ? Random.Range(0f, 0.12f) : Random.Range(0.5f, 1.3f))
            : 1f - Random.value * steadyJitter;

        if (glow != null) glow.intensity = baseIntensity * level;
        if (screen == null) return;
        
        var st = new Vector4(baseSt.x, baseSt.y, Random.value, Random.value);
        screen.GetPropertyBlock(block);
        block.SetVector(BaseMapStId, st);
        block.SetColor(EmissionColorId, baseEmission * level);
        screen.SetPropertyBlock(block);
    }
}
