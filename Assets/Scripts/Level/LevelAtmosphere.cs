using UnityEngine;
using UnityEngine.Rendering;



[ExecuteAlways]
public class LevelAtmosphere : MonoBehaviour
{
    [Tooltip("Light that reaches everywhere. Keep a little so full darkness isn't unplayable.")]
    [SerializeField] Color ambientColor = new Color(0.035f, 0.06f, 0.06f);
    [SerializeField] bool fog = true;
    [SerializeField] Color fogColor = new Color(0.015f, 0.04f, 0.04f);
    [Range(0f, 0.2f)] [SerializeField] float fogDensity = 0.06f;
    [Range(0f, 1f)] [SerializeField] float reflectionIntensity = 0.15f;
    [Tooltip("Sky seen through windows. Leave empty for underground levels.")]
    [SerializeField] Material skybox;

    void OnEnable() => Apply();

    
    void Start()
    {
        if (!Application.isPlaying || skybox == null) return;
        foreach (Camera cam in FindObjectsByType<Camera>()) cam.clearFlags = CameraClearFlags.Skybox;
    }

    void OnValidate() => Apply();

    void Apply()
    {
        RenderSettings.skybox = skybox;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor;
        RenderSettings.reflectionIntensity = reflectionIntensity;
        RenderSettings.fog = fog;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
    }
}
