using UnityEngine;



[RequireComponent(typeof(AudioSource))]
public class SettingsAudio : MonoBehaviour
{
    public enum Channel { Music, Sfx }

    [SerializeField] Channel channel = Channel.Sfx;

    AudioSource source;
    float baseVolume;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        baseVolume = source.volume;
        
        if (channel == Channel.Music) source.ignoreListenerPause = true;
    }

    void OnEnable()
    {
        GameSettings.Changed += Apply;
        Apply();
    }

    void OnDisable() => GameSettings.Changed -= Apply;

    void Apply() => source.volume = baseVolume * (channel == Channel.Music ? GameSettings.MusicVolume : GameSettings.SfxVolume);
}
