using UnityEngine;





public class HelicopterRig : MonoBehaviour
{
    [Tooltip("Flight path. The helicopter hovers at the last one.")]
    [SerializeField] Transform[] waypoints;
    [Tooltip("Where the cinematic camera rides: out in front")]
    [SerializeField] Transform doorMount;
    [Tooltip("How far along the path (in waypoints) it looks to decide which way to face: more is smoother")]
    [SerializeField] float headingWindow = 1.3f;

    public Transform DoorMount => doorMount != null ? doorMount : transform;
    public float Progress { get; private set; }

    [Header("Sound")]
    [Tooltip("File name in Resources/Sfx; loops from the start of the flight")]
    [SerializeField] string soundName = "helicopter";
    [SerializeField] float soundVolume = 0.9f;
    [SerializeField] float soundFadeIn = 1.5f;

    float yaw;
    bool hasYaw;
    AudioSource engine;
    float engineLevel;
    float engineTarget = 1f;
    float engineFadeSeconds;

    void Start()
    {
        if (waypoints != null && waypoints.Length > 0) SetProgress(0f);

        
        AudioClip clip = Sfx.Clip(soundName);
        if (clip == null) return;
        engine = gameObject.AddComponent<AudioSource>();
        engine.clip = clip;
        engine.loop = true;
        engine.spatialBlend = 0f;
        engine.volume = 0f;
        engine.Play();
        engineFadeSeconds = soundFadeIn;
    }

    void Update()
    {
        if (engine == null) return;
        engineLevel = Mathf.MoveTowards(engineLevel, engineTarget, Time.unscaledDeltaTime / Mathf.Max(0.05f, engineFadeSeconds));
        engine.volume = engineLevel * soundVolume * GameSettings.SfxVolume;
        if (engineLevel <= 0f && engineTarget <= 0f) engine.Stop();
    }

    
    public void FadeOutSound(float seconds)
    {
        engineTarget = 0f;
        engineFadeSeconds = seconds;
    }

    
    public void SetProgress(float t)
    {
        Progress = Mathf.Clamp01(t);
        if (waypoints == null || waypoints.Length < 2) return;

        float last = waypoints.Length - 1;
        float u = Progress * last;
        Vector3 position = Point(u);

        
        
        Vector3 along = Point(Mathf.Min(u + headingWindow, last)) - Point(Mathf.Max(u - headingWindow, 0f));
        along.y = 0f;
        if (along.sqrMagnitude > 0.01f)
        {
            yaw = Mathf.Atan2(along.x, along.z) * Mathf.Rad2Deg;
            hasYaw = true;
        }
        else if (!hasYaw)
        {
            yaw = transform.eulerAngles.y;
        }

        transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
    }

    
    Vector3 Point(float u)
    {
        u = Mathf.Clamp(u, 0f, waypoints.Length - 1);
        int i = Mathf.Min((int)u, waypoints.Length - 2);
        return CatmullRom(At(i - 1), At(i), At(i + 1), At(i + 2), u - i);
    }

    Vector3 At(int i) => waypoints[Mathf.Clamp(i, 0, waypoints.Length - 1)].position;

    static Vector3 CatmullRom(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        float t2 = t * t, t3 = t2 * t;
        return 0.5f * ((2f * b) + (-a + c) * t + (2f * a - 5f * b + 4f * c - d) * t2 + (-a + 3f * b - 3f * c + d) * t3);
    }

    void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length < 2) return;
        Gizmos.color = new Color(0.3f, 0.9f, 1f, 0.8f);
        Vector3 previous = Point(0f);
        for (int i = 1; i <= 120; i++)
        {
            Vector3 next = Point(i / 120f * (waypoints.Length - 1));
            Gizmos.DrawLine(previous, next);
            previous = next;
        }
    }
}
