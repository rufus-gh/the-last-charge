using UnityEngine;




[RequireComponent(typeof(MazeGenerator))]
public class CobwebPlacer : MonoBehaviour
{
    [SerializeField] Vector2 scaleRange = new Vector2(0.7f, 1.6f);

    public void Place(MazeGenerator maze)
    {
        Transform parent = maze.ResetGeneratedChild("Cobwebs");
        LevelTheme theme = maze.Theme;
        if (theme == null || theme.cobwebChance <= 0f || theme.cobwebs.Count == 0) return;

        var rng = new System.Random(maze.Seed + 6);
        float half = maze.CellSize / 2f - maze.WallThickness / 2f;
        int placed = 0;
        for (int x = 0; x < maze.GridSize.x; x++)
        for (int z = 0; z < maze.GridSize.y; z++)
        {
            var cell = new Vector2Int(x, z);
            for (int d = 0; d < 4; d++)
            {
                int e = (d + 1) % 4;
                if (!maze.HasWall(cell, d) || !maze.HasWall(cell, e) || rng.NextDouble() >= theme.cobwebChance) continue;

                GameObject prefab = theme.cobwebs[rng.Next(theme.cobwebs.Count)];
                if (prefab == null) continue;
                Vector3 a = maze.DirToWorld(d), b = maze.DirToWorld(e);
                Vector3 corner = maze.CellCenter(cell) + (a + b) * half + Vector3.up * (maze.WallHeight - 0.01f);

                GameObject web = MazeGenerator.SpawnPrefab(prefab, parent);
                web.transform.SetPositionAndRotation(corner, Quaternion.LookRotation(-(a + b)));
                web.transform.localScale = Vector3.one * Mathf.Lerp(scaleRange.x, scaleRange.y, (float)rng.NextDouble());
                web.isStatic = true;
                placed++;
            }
        }
        Debug.Log($"Cobwebs: {placed}.", this);
    }
}
