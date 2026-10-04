using System;
using UnityEngine;




public static class GameSettings
{
    public const float DefaultLookSensitivity = 1f;
    public const float DefaultArrowTurnSpeed = 140f;

    
    public static float LookSensitivity { get => Get("LookSensitivity", DefaultLookSensitivity); set => Set("LookSensitivity", value); }
    public static float AimSensitivity { get => Get("AimSensitivity", 1f); set => Set("AimSensitivity", value); }
    public static bool InvertY { get => Get("InvertY", 0f) > 0.5f; set => Set("InvertY", value ? 1f : 0f); }
    
    public static bool TrackpadMode { get => Get("TrackpadMode", 0f) > 0.5f; set => Set("TrackpadMode", value ? 1f : 0f); }
    
    public static bool ArrowKeyTurning { get => Get("ArrowKeyTurning", 0f) > 0.5f; set => Set("ArrowKeyTurning", value ? 1f : 0f); }
    
    public static float ArrowTurnSpeed { get => Get("ArrowTurnSpeed", DefaultArrowTurnSpeed); set => Set("ArrowTurnSpeed", value); }

    
    public static float MasterVolume { get => Get("MasterVolume", 1f); set { Set("MasterVolume", value); AudioListener.volume = value; } }
    public static float MusicVolume { get => Get("MusicVolume", 0.8f); set => Set("MusicVolume", value); }
    public static float SfxVolume { get => Get("SfxVolume", 1f); set => Set("SfxVolume", value); }

    public static event Action Changed;

    public static void ResetToDefaults()
    {
        foreach (string key in new[] { "LookSensitivity", "AimSensitivity", "InvertY", "TrackpadMode", "ArrowKeyTurning",
                     "ArrowTurnSpeed", "MasterVolume", "MusicVolume", "SfxVolume" })
            PlayerPrefs.DeleteKey(Prefix + key);
        PlayerPrefs.Save();
        AudioListener.volume = MasterVolume;
        Changed?.Invoke();
    }

    public static void Save() => PlayerPrefs.Save();

    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplyOnStart() => AudioListener.volume = MasterVolume;

    const string Prefix = "LosingPower.";

    static float Get(string key, float fallback) => PlayerPrefs.GetFloat(Prefix + key, fallback);

    static void Set(string key, float value)
    {
        PlayerPrefs.SetFloat(Prefix + key, value);
        Changed?.Invoke();
    }
}
