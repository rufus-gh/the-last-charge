using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;




[RequireComponent(typeof(MazeGenerator))]
public class GrimeDecals : MonoBehaviour
{
    [Header("Materials (one is picked at random per decal)")]
    [SerializeField] Material[] puddleMaterials;
    [SerializeField] Material[] floorBloodMaterials;
    [SerializeField] Material[] wallBloodMaterials;

    [Header("Puddles")]
    [SerializeField] int puddleCount = 14;
    [SerializeField] Vector2 puddleSize = new Vector2(1.2f, 2.6f);
    [SerializeField] float puddleSpacing = 5f;

    [Header("Floor Blood")]
    [SerializeField] int floorBloodCount = 12;
    [SerializeField] Vector2 floorBloodSize = new Vector2(0.7f, 1.6f);
    [SerializeField] float floorBloodSpacing = 4f;

    [Header("Wall Blood")]
    [SerializeField] int wallBloodCount = 16;
    [SerializeField] Vector2 wallBloodWidth = new Vector2(0.7f, 1.3f);
    [SerializeField] float wallBloodSpacing = 4f;

    const float PuddleLift = 0.004f;
    const float FloorBloodLift = 0.006f;
    const float WallOffset = 0.004f;

    public void Place(MazeGenerator maze)
    {
        Transform parent = maze.ResetGeneratedChild("Grime");
        Transform structure = maze.transform.Find("Generated/Structure");
        var rng = new System.Random(maze.Seed + 4);

        int puddles = ScatterFloor(maze, parent, rng, puddleMaterials, puddleCount, puddleSize, puddleSpacing, PuddleLift, "Puddle");
        int floorBlood = ScatterFloor(maze, parent, rng, floorBloodMaterials, floorBloodCount, floorBloodSize, floorBloodSpacing, FloorBloodLift, "Blood (Floor)");
        int wallBlood = ScatterWalls(maze, structure, parent, rng);
        Debug.Log($"Grime: {puddles} puddles, {floorBlood} floor blood, {wallBlood} wall blood.", this);
    }

    int ScatterFloor(MazeGenerator maze, Transform parent, System.Random rng, Material[] materials, int count,
                     Vector2 sizeRange, float spacing, float lift, string label)
    {
        if (materials == null || materials.Length == 0) return 0;

        var placed = new List<Vector3>();
        float floorY = maze.transform.position.y;
        for (int attempt = 0; attempt < count * 20 && placed.Count < count; attempt++)
        {
            var cell = new Vector2Int(rng.Next(maze.GridSize.x), rng.Next(maze.GridSize.y));
            if (cell == maze.StartCell) continue;

            float jitter = maze.CellSize * 0.3f;
            Vector3 guess = maze.CellCenter(cell) + new Vector3(Range(rng, -jitter, jitter), 0f, Range(rng, -jitter, jitter));
            if (!NavMesh.SamplePosition(guess, out NavMeshHit hit, 1f, NavMesh.AllAreas)) continue;
            Vector3 p = new Vector3(hit.position.x, floorY + lift, hit.position.z);
            if (placed.Exists(q => (q - p).sqrMagnitude < spacing * spacing)) continue;

            float size = Range(rng, sizeRange.x, sizeRange.y);
            var scale = new Vector3(size, size * Range(rng, 0.65f, 1f), 1f);
            
            var rotation = Quaternion.Euler(90f, Range(rng, 0f, 360f), 0f);
            CreateQuad(label, parent, p, rotation, scale, Pick(materials, rng));
            placed.Add(p);
        }
        return placed.Count;
    }

    int ScatterWalls(MazeGenerator maze, Transform structure, Transform parent, System.Random rng)
    {
        if (wallBloodMaterials == null || wallBloodMaterials.Length == 0 || structure == null) return 0;
        
        Physics.SyncTransforms();

        var placed = new List<Vector3>();
        var dirs = new List<int>(4);
        for (int attempt = 0; attempt < wallBloodCount * 25 && placed.Count < wallBloodCount; attempt++)
        {
            var cell = new Vector2Int(rng.Next(maze.GridSize.x), rng.Next(maze.GridSize.y));
            dirs.Clear();
            for (int d = 0; d < 4; d++) if (maze.HasWall(cell, d)) dirs.Add(d);
            if (dirs.Count == 0) continue;

            int dir = dirs[rng.Next(dirs.Count)];
            Vector3 toWall = maze.DirToWorld(dir);
            Vector3 along = maze.DirToWorld((dir + 1) % 4);
            float height = Range(rng, 0.9f, Mathf.Min(1.9f, maze.WallHeight - 0.8f));
            Vector3 origin = maze.CellCenter(cell) + along * Range(rng, -1.2f, 1.2f) + Vector3.up * height;

            
            if (!Physics.Raycast(origin, toWall, out RaycastHit hit, maze.CellSize, ~0, QueryTriggerInteraction.Ignore)) continue;
            if (!hit.collider.transform.IsChildOf(structure) || Mathf.Abs(hit.normal.y) > 0.1f) continue;
            if (placed.Exists(q => (q - hit.point).sqrMagnitude < wallBloodSpacing * wallBloodSpacing)) continue;

            float width = Range(rng, wallBloodWidth.x, wallBloodWidth.y);
            float tall = width * 1.8f;
            
            Vector3 p = hit.point + hit.normal * WallOffset;
            p.y = Mathf.Max(p.y, maze.transform.position.y + tall * 0.5f + 0.05f);

            
            var rotation = Quaternion.LookRotation(-hit.normal, Vector3.up);
            CreateQuad("Blood (Wall)", parent, p, rotation, new Vector3(width, tall, 1f), Pick(wallBloodMaterials, rng));
            placed.Add(hit.point);
        }
        return placed.Count;
    }

    static void CreateQuad(string name, Transform parent, Vector3 position, Quaternion rotation, Vector3 scale, Material material)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = name;
        MazeGenerator.DestroySafe(quad.GetComponent<Collider>());
        quad.transform.SetParent(parent, false);
        quad.transform.SetPositionAndRotation(position, rotation);
        quad.transform.localScale = scale;
        quad.isStatic = true;
        var r = quad.GetComponent<MeshRenderer>();
        r.sharedMaterial = material;
        r.shadowCastingMode = ShadowCastingMode.Off;
    }

    static Material Pick(Material[] materials, System.Random rng) => materials[rng.Next(materials.Length)];

    static float Range(System.Random rng, float min, float max) => min + (float)rng.NextDouble() * (max - min);
}
