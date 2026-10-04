using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;




[RequireComponent(typeof(MazeGenerator))]
public class BatteryPlacer : MonoBehaviour
{
    [SerializeField] GameObject batteryPrefab;
    [SerializeField] int batteryCount = 10;
    [Tooltip("Keep batteries away from the spawn so the player has to go looking")]
    [SerializeField] float minDistanceFromStart = 12f;
    [Tooltip("Minimum straight-line gap between batteries")]
    [SerializeField] float minSpacing = 12f;

    public void Place(MazeGenerator maze)
    {
        Transform parent = maze.ResetGeneratedChild("Batteries");
        if (batteryPrefab == null)
        {
            Debug.LogWarning("No battery prefab assigned, batteries were not placed.", this);
            return;
        }

        var rng = new System.Random(maze.Seed + 3);
        var candidates = new List<Vector2Int>();
        for (int x = 0; x < maze.GridSize.x; x++)
        for (int z = 0; z < maze.GridSize.y; z++)
        {
            var c = new Vector2Int(x, z);
            if (c != maze.StartCell && c != maze.ExitCell) candidates.Add(c);
        }

        
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }
        candidates.Sort((a, b) => (maze.WallCount(b) == 3).CompareTo(maze.WallCount(a) == 3));

        Vector3 start = maze.CellCenter(maze.StartCell);
        var placed = new List<Vector3>();
        foreach (Vector2Int cell in candidates)
        {
            if (placed.Count >= batteryCount) break;

            Vector3 center = maze.CellCenter(cell);
            if (Vector3.Distance(center, start) < minDistanceFromStart) continue;
            if (placed.Exists(p => Vector3.Distance(p, center) < minSpacing)) continue;
            if (!NavMesh.SamplePosition(center, out NavMeshHit hit, maze.CellSize * 0.5f, NavMesh.AllAreas)) continue;

            GameObject battery = MazeGenerator.SpawnPrefab(batteryPrefab, parent);
            battery.transform.position = new Vector3(hit.position.x, center.y, hit.position.z);
            placed.Add(center);
        }

        if (placed.Count < batteryCount)
            Debug.LogWarning($"Only room for {placed.Count} of {batteryCount} batteries. Lower Min Spacing to fit more.", this);
    }
}
