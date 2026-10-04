using UnityEngine;





public class ZombieCrowd : MonoBehaviour
{
    [Tooltip("One is picked at random per zombie")]
    [SerializeField] GameObject[] zombiePrefabs;
    [SerializeField] int count = 55;
    [Tooltip("Crowd radius in metres")]
    [SerializeField] float radius = 16f;
    [SerializeField] Vector2 speedRange = new Vector2(0.25f, 0.9f);
    [Tooltip("Degrees per second each one wanders off its heading")]
    [SerializeField] float wander = 25f;
    [Tooltip("Animation is frozen beyond this distance from the camera")]
    [SerializeField] float animateDistance = 70f;
    [SerializeField] Vector2 scaleRange = new Vector2(0.92f, 1.08f);
    [SerializeField] int seed = 1;

    Transform[] zombies;
    float[] heading;
    float[] speed;
    Animator[] animators;
    float nextDistanceCheck;

    void Start()
    {
        if (zombiePrefabs == null || zombiePrefabs.Length == 0) return;

        var rng = new System.Random(seed);
        zombies = new Transform[count];
        heading = new float[count];
        speed = new float[count];
        animators = new Animator[count];

        for (int i = 0; i < count; i++)
        {
            
            float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
            float distance = Mathf.Sqrt((float)rng.NextDouble()) * radius;
            var position = new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);

            heading[i] = (float)rng.NextDouble() * 360f;
            speed[i] = Mathf.Lerp(speedRange.x, speedRange.y, (float)rng.NextDouble());

            GameObject prefab = zombiePrefabs[rng.Next(zombiePrefabs.Length)];
            GameObject zombie = Instantiate(prefab, transform.TransformPoint(position), Quaternion.Euler(0f, heading[i], 0f), transform);
            float scale = Mathf.Lerp(scaleRange.x, scaleRange.y, (float)rng.NextDouble());
            zombie.transform.localScale = Vector3.one * scale;
            zombies[i] = zombie.transform;

            
            animators[i] = zombie.GetComponent<Animator>();
            if (animators[i] != null)
            {
                animators[i].Play(0, 0, (float)rng.NextDouble());
                animators[i].speed = Mathf.Clamp(speed[i] / speedRange.y, 0.35f, 1f) * 1.1f;
            }
        }
    }

    void Update()
    {
        if (zombies == null) return;
        float dt = Time.deltaTime;
        FreezeDistantZombies();

        for (int i = 0; i < zombies.Length; i++)
        {
            Transform z = zombies[i];
            if (z == null) continue;

            
            Vector3 local = transform.InverseTransformPoint(z.position);
            float drift = (Mathf.PerlinNoise(i * 0.37f, Time.time * 0.12f) - 0.5f) * 2f;
            heading[i] += drift * wander * dt;
            if (local.magnitude > radius)
            {
                float inward = Mathf.Atan2(-local.x, -local.z) * Mathf.Rad2Deg;
                heading[i] = Mathf.MoveTowardsAngle(heading[i], inward, 60f * dt);
            }

            Quaternion rotation = Quaternion.Euler(0f, heading[i], 0f);
            z.position += rotation * Vector3.forward * speed[i] * dt;
            z.rotation = rotation;
        }
    }

    
    void FreezeDistantZombies()
    {
        if (Time.time < nextDistanceCheck) return;
        nextDistanceCheck = Time.time + 0.5f;
        Camera camera = Camera.main;
        foreach (Camera c in Camera.allCameras) if (c.enabled && (camera == null || c.depth > camera.depth)) camera = c;
        if (camera == null) return;

        Vector3 eye = camera.transform.position;
        float limit = animateDistance * animateDistance;
        for (int i = 0; i < zombies.Length; i++)
        {
            if (zombies[i] == null || animators[i] == null) continue;
            animators[i].enabled = (zombies[i].position - eye).sqrMagnitude < limit;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.8f, 0.2f, 0.2f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
