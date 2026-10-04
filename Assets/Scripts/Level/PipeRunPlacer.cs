using UnityEngine;






[RequireComponent(typeof(MazeGenerator))]
public class PipeRunPlacer : MonoBehaviour
{
    [Tooltip("How much lower the runs along north-south walls hang")]
    [SerializeField] float crossingDrop = 0.34f;

    public void Place(MazeGenerator maze)
    {
        Transform parent = maze.ResetGeneratedChild("Pipes");
        LevelTheme theme = maze.Theme;
        if (theme == null || theme.pipeChance <= 0f || theme.pipeRuns.Count == 0) return;

        float half = maze.CellSize / 2f - maze.WallThickness / 2f;
        int segments = 0;
        for (int x = 0; x < maze.GridSize.x; x++)
        for (int z = 0; z < maze.GridSize.y; z++)
        {
            var cell = new Vector2Int(x, z);
            for (int dir = 0; dir < 4; dir++)
            {
                PipeRun run = RunAt(maze, theme, cell, dir);
                if (run == null) continue;

                Vector3 toWall = maze.DirToWorld(dir);
                Vector3 along = maze.DirToWorld((dir + 1) % 4);
                float drop = dir % 2 == 1 ? crossingDrop : 0f;
                Vector3 origin = maze.CellCenter(cell) + toWall * half + Vector3.up * (maze.WallHeight - drop);
                
                Quaternion rotation = Quaternion.LookRotation(-toWall);
                Spawn(run.segment, parent, origin, rotation);
                segments++;

                
                for (int side = -1; side <= 1; side += 2)
                {
                    Vector2Int next = cell + MazeGenerator.Directions[(dir + 1) % 4] * side;
                    if (RunAt(maze, theme, next, dir) == run) continue;
                    Spawn(run.end, parent, origin + along * (side * maze.CellSize / 2f), rotation);
                }
            }
        }
        Debug.Log($"Pipes: {segments} segments.", this);
    }

    static void Spawn(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return;
        GameObject go = MazeGenerator.SpawnPrefab(prefab, parent);
        go.transform.SetPositionAndRotation(position, rotation);
        go.isStatic = true;
    }

    
    
    static PipeRun RunAt(MazeGenerator maze, LevelTheme theme, Vector2Int cell, int dir)
    {
        if (!maze.InBounds(cell) || !maze.HasWall(cell, dir) || maze.IsWindow(cell, dir)) return null;

        
        int line, face, axis = dir % 2;
        switch (dir)
        {
            case 0: line = cell.y + 1; face = 0; break;
            case 2: line = cell.y; face = 1; break;
            case 1: line = cell.x + 1; face = 0; break;
            default: line = cell.x; face = 1; break;
        }
        uint h = Hash((uint)maze.Seed, (uint)(line * 4 + face * 2 + axis));
        if ((h & 0xffff) / 65535f >= theme.pipeChance) return null;
        return theme.pipeRuns[(int)((h >> 16) % (uint)theme.pipeRuns.Count)];
    }

    static uint Hash(uint a, uint b)
    {
        uint h = a * 0x9E3779B1u ^ (b + 0x7F4A7C15u) * 0x85EBCA77u;
        h ^= h >> 15;
        h *= 0xC2B2AE3Du;
        h ^= h >> 13;
        return h;
    }
}
