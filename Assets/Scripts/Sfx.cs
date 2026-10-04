using System.Collections.Generic;
using UnityEngine;



public static class Sfx
{
    static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

    public static AudioClip Clip(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (!clips.TryGetValue(name, out AudioClip clip))
        {
            clip = Resources.Load<AudioClip>("Sfx/" + name);
            if (clip == null) Debug.LogWarning("No sound effect called " + name + " in Resources/Sfx");
            clips[name] = clip;
        }
        return clip;
    }

    
    
    public static AudioSource Play(string name, float volume = 1f, float pitch = 1f, bool ui = false, float delay = 0f) =>
        Start(name, false, Vector3.zero, volume, pitch, ui, delay);

    
    public static AudioSource PlayAt(string name, Vector3 position, float volume = 1f, float pitch = 1f) =>
        Start(name, true, position, volume, pitch, false, 0f);

    static AudioSource Start(string name, bool positional, Vector3 position, float volume, float pitch, bool ui, float delay)
    {
        AudioClip clip = Clip(name);
        if (clip == null) return null;

        var go = new GameObject("SFX " + name);
        go.transform.position = position;
        var source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume * GameSettings.SfxVolume;
        source.pitch = pitch;
        source.spatialBlend = positional ? 1f : 0f;
        source.minDistance = 4f;
        source.maxDistance = 60f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.ignoreListenerPause = ui;
        if (delay > 0f) source.PlayDelayed(delay); else source.Play();
        Object.Destroy(go, delay + clip.length / Mathf.Max(0.1f, pitch) + 0.2f);
        return source;
    }
}
