using UnityEngine;




public class ZombieVoice : MonoBehaviour
{
    public const int GroanCount = 7;

    
    public static bool Silenced { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetSilence()
    {
        Silenced = false;
        
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) => Silenced = false;
    }

    public static void SilenceAll()
    {
        Silenced = true;
        foreach (ZombieVoice voice in FindObjectsByType<ZombieVoice>(FindObjectsInactive.Include)) voice.source.Stop();
    }

    [SerializeField] Vector2 groanInterval = new Vector2(4f, 10f);
    [Tooltip("Lower for something bigger")]
    [SerializeField] float pitch = 1f;
    [SerializeField] float volume = 0.7f;
    [SerializeField] float maxDistance = 28f;

    AudioSource source;
    float nextGroan;

    
    public static ZombieVoice Add(GameObject zombie, float pitch = 1f, float volume = 0.7f)
    {
        var voice = zombie.GetComponent<ZombieVoice>();
        if (voice == null) voice = zombie.AddComponent<ZombieVoice>();
        voice.pitch = pitch;
        voice.volume = volume;
        return voice;
    }

    void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 1f;
        source.minDistance = 3f;
        source.maxDistance = maxDistance;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.playOnAwake = false;
        
        nextGroan = Time.time + Random.Range(0f, groanInterval.y);
    }

    void Update()
    {
        if (Silenced || Time.time < nextGroan) return;
        nextGroan = Time.time + Random.Range(groanInterval.x, groanInterval.y);
        Groan();
    }

    public void Groan() => Play("zombie-groan-" + Random.Range(1, GroanCount + 1), 1f);

    public void Snarl() => Play("zombie-snarl", 1.2f);

    void Play(string clipName, float loudness)
    {
        AudioClip clip = Sfx.Clip(clipName);
        if (Silenced || clip == null || source == null) return;
        source.pitch = pitch * Random.Range(0.92f, 1.08f);
        source.PlayOneShot(clip, volume * loudness * GameSettings.SfxVolume);
    }
}
