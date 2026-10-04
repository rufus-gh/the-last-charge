using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] GameObject zombiePrefab;
    [SerializeField] PlayerHealth player;

    [Header("Spawn Rate")]
    [SerializeField] float initialDelay = 3f;
    [Tooltip("Seconds between spawns at the start")]
    [SerializeField] float spawnInterval = 6f;
    [Tooltip("Fastest the interval can get as difficulty ramps up")]
    [SerializeField] float minSpawnInterval = 2f;
    [Tooltip("How much the interval shrinks per minute survived")]
    [SerializeField] float intervalDecreasePerMinute = 1f;
    [SerializeField] int zombiesPerSpawn = 1;
    [Tooltip("No more spawns while this many spawned zombies are alive")]
    [SerializeField] int maxAlive = 12;

    [Header("Placement")]
    [Tooltip("Area (relative to this object) where zombies may spawn")]
    [SerializeField] Vector3 areaSize = new Vector3(45f, 4f, 45f);
    [SerializeField] float minDistanceFromPlayer = 10f;
    [SerializeField] float maxDistanceFromPlayer = 40f;
    [Tooltip("Layers that block the player's sight (walls etc.)")]
    [SerializeField] LayerMask sightBlockers = ~0;
    [SerializeField] float playerEyeHeight = 0.7f;
    [SerializeField] int attemptsPerSpawn = 30;
    [Tooltip("Cover the level's MazeGenerator instead of the area set above")]
    [SerializeField] bool fitToMaze = true;
    [Tooltip("New zombies keep at least this far from living ones, so they come from different directions instead of in a lump")]
    [SerializeField] float minDistanceFromOtherZombies = 8f;

    readonly List<GameObject> alive = new List<GameObject>();
    NavMeshPath path;
    float nextSpawnTime;
    float startTime;

    
    static readonly float[] VisibilityCheckHeights = { 0.2f, 1f, 1.8f };

    public float CurrentInterval =>
        Mathf.Max(minSpawnInterval, spawnInterval - intervalDecreasePerMinute * (Time.time - startTime) / 60f);

    void Awake() => path = new NavMeshPath();

    void Start()
    {
        if (player == null) player = FindAnyObjectByType<PlayerHealth>();
        if (fitToMaze) FitToMaze();
        ApplyLevelDifficulty();
        startTime = Time.time;
        nextSpawnTime = Time.time + initialDelay;
    }

    void Update()
    {
        if (zombiePrefab == null || player == null || Time.time < nextSpawnTime) return;
        nextSpawnTime = Time.time + CurrentInterval;

        
        alive.RemoveAll(z => z == null || z.GetComponent<EnemyHealth>().IsDead);
        for (int i = 0; i < zombiesPerSpawn && alive.Count < maxAlive; i++)
        {
            if (!TryFindSpawnPoint(out Vector3 point)) break;
            alive.Add(Instantiate(zombiePrefab, point, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)));
        }
    }

    
    void ApplyLevelDifficulty()
    {
        MazeGenerator maze = FindAnyObjectByType<MazeGenerator>();
        float d = maze != null ? Mathf.Max(0.1f, maze.Difficulty) : 1f;
        initialDelay /= d;
        spawnInterval /= d;
        minSpawnInterval /= d;
        intervalDecreasePerMinute *= d;
        maxAlive = Mathf.Max(1, Mathf.RoundToInt(maxAlive * d));
    }

    void FitToMaze()
    {
        MazeGenerator maze = FindAnyObjectByType<MazeGenerator>();
        if (maze == null || !maze.IsGenerated) return;
        var size = new Vector3(maze.GridSize.x * maze.CellSize, 0f, maze.GridSize.y * maze.CellSize);
        transform.position = maze.transform.TransformPoint(size * 0.5f);
        areaSize = new Vector3(size.x, 4f, size.z);
    }

    bool TryFindSpawnPoint(out Vector3 point)
    {
        Vector3 playerPos = player.transform.position;
        Vector3 eye = playerPos + Vector3.up * playerEyeHeight;

        for (int attempt = 0; attempt < attemptsPerSpawn; attempt++)
        {
            Vector3 random = transform.position + new Vector3(
                Random.Range(-0.5f, 0.5f) * areaSize.x,
                0f,
                Random.Range(-0.5f, 0.5f) * areaSize.z);

            if (!NavMesh.SamplePosition(random, out NavMeshHit hit, 2f, NavMesh.AllAreas)) continue;
            point = hit.position;

            if (Mathf.Abs(point.y - transform.position.y) > areaSize.y * 0.5f) continue;

            float distance = Vector3.Distance(point, playerPos);
            if (distance < minDistanceFromPlayer || distance > maxDistanceFromPlayer) continue;
            if (IsVisibleFrom(eye, point)) continue;
            if (TooCloseToOtherZombies(point)) continue;

            
            if (!NavMesh.CalculatePath(point, playerPos, NavMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete) continue;

            return true;
        }

        point = default;
        return false;
    }

    bool TooCloseToOtherZombies(Vector3 point)
    {
        foreach (GameObject z in alive)
            if (z != null && (z.transform.position - point).sqrMagnitude < minDistanceFromOtherZombies * minDistanceFromOtherZombies)
                return true;
        return false;
    }

    bool IsVisibleFrom(Vector3 eye, Vector3 feet)
    {
        foreach (float h in VisibilityCheckHeights)
        {
            Vector3 target = feet + Vector3.up * h;
            if (!Physics.Linecast(eye, target, sightBlockers, QueryTriggerInteraction.Ignore))
                return true;
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.2f, 0.4f);
        Gizmos.DrawWireCube(transform.position, areaSize);
    }
}
