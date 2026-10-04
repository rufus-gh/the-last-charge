using UnityEngine;


[RequireComponent(typeof(Light))]
public class FlickeringLight : MonoBehaviour
{
    [SerializeField] Renderer fixture;
    [ColorUsage(false, true)]
    [SerializeField] Color fixtureColor = new Color(2.5f, 0.45f, 0.05f);

    [Header("Flicker")]
    [Range(0f, 1f)]
    [SerializeField] float minBrightness = 0.55f;
    [SerializeField] float flickerSpeed = 6f;

    [Header("Dropouts")]
    [Tooltip("Average dropouts per second")]
    [SerializeField] float dropoutRate = 0.2f;
    [SerializeField] Vector2 dropoutDuration = new Vector2(0.05f, 0.4f);
    [Range(0f, 1f)]
    [SerializeField] float dropoutBrightness = 0.05f;

    Light lightSource;
    float baseIntensity;
    float noiseSeed;
    float dropoutUntil;
    MaterialPropertyBlock propertyBlock;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    void Awake()
    {
        lightSource = GetComponent<Light>();
        baseIntensity = lightSource.intensity;
        noiseSeed = Random.value * 100f;
        propertyBlock = new MaterialPropertyBlock();
    }

    void Update()
    {
        float brightness;
        if (Time.time < dropoutUntil)
        {
            brightness = dropoutBrightness;
        }
        else
        {
            if (Random.value < dropoutRate * Time.deltaTime)
                dropoutUntil = Time.time + Random.Range(dropoutDuration.x, dropoutDuration.y);

            float noise = Mathf.PerlinNoise(noiseSeed, Time.time * flickerSpeed);
            brightness = Mathf.Lerp(minBrightness, 1f, noise);
        }

        lightSource.intensity = baseIntensity * brightness;

        if (fixture != null)
        {
            propertyBlock.SetColor(BaseColorId, fixtureColor * brightness);
            fixture.SetPropertyBlock(propertyBlock);
        }
    }
}
