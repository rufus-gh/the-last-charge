using System.Collections.Generic;
using UnityEngine;





[RequireComponent(typeof(MazeGenerator))]
public class PropScatter : MonoBehaviour
{
    [Tooltip("Narrowest gap props may leave for walking through a cell")]
    [SerializeField] float minLaneWidth = 1.8f;
    [SerializeField] float maxYawJitter = 6f;
    [Tooltip("Room furniture is kept this far from the player spawn")]
    [SerializeField] float spawnClearance = 2.5f;

    const float WallGap = 0.03f;

    public void Scatter(MazeGenerator maze)
    {
        Transform parent = maze.ResetGeneratedChild("Props");
        LevelTheme theme = maze.Theme;
        if (theme == null) return;

        var rng = new System.Random(maze.Seed + 1);
        var furnished = new HashSet<Vector2Int>();
        
        
        
        var intrusions = new Dictionary<Vector2Int, float[]>();

        if (maze.StartRoom.width > 0) Furnish(maze, maze.StartRoom, theme.startRoomFurnishing, parent, rng, furnished);
        for (int i = 0; i < maze.Rooms.Count && i < theme.featureRooms.Count; i++)
            Furnish(maze, maze.Rooms[i], theme.featureRooms[i], parent, rng, furnished);

        var walls = new List<int>(4);
        for (int x = 0; x < maze.GridSize.x; x++)
        for (int z = 0; z < maze.GridSize.y; z++)
        {
            var cell = new Vector2Int(x, z);
            if (cell == maze.StartCell || cell == maze.ExitCell) continue;

            if (!furnished.Contains(cell))
            {
                List<PropEntry> props = maze.IsRoomCell(cell) && theme.roomWallProps.Count > 0 ? theme.roomWallProps : theme.wallProps;

                var intrusion = new float[4];
                intrusions[cell] = intrusion;
                walls.Clear();
                for (int d = 0; d < 4; d++) if (maze.HasWall(cell, d) && !maze.IsWindow(cell, d)) walls.Add(d);
                Shuffle(walls, rng);

                int placed = 0;
                foreach (int dir in walls)
                {
                    if (placed >= theme.maxWallPropsPerCell) break;
                    if (rng.NextDouble() >= theme.wallPropChance) continue;
                    PropEntry entry = Pick(props, rng);
                    if (entry != null && TryPlaceAgainstWall(maze, cell, dir, entry, intrusion, OppositeIntrusion(maze, cell, dir, intrusions), parent, rng)) placed++;
                }
            }

            int clutter = rng.Next(theme.maxClutterPerCell + 1);
            for (int i = 0; i < clutter; i++) PlaceClutter(maze, cell, Pick(theme.clutter, rng), parent, rng);
        }

        PlaceCeilingProps(maze, theme, rng);
    }

    
    void PlaceCeilingProps(MazeGenerator maze, LevelTheme theme, System.Random rng)
    {
        Transform parent = maze.ResetGeneratedChild("Ceiling");
        if (theme.ceilingProps.Count == 0 || theme.ceilingPropChance <= 0f) return;

        for (int x = 0; x < maze.GridSize.x; x++)
        for (int z = 0; z < maze.GridSize.y; z++)
        {
            var cell = new Vector2Int(x, z);
            if (rng.NextDouble() >= theme.ceilingPropChance) continue;
            PropEntry entry = Pick(theme.ceilingProps, rng);
            if (entry == null) continue;

            bool openNorthSouth = !maze.HasWall(cell, 0) || !maze.HasWall(cell, 2);
            bool openEastWest = !maze.HasWall(cell, 1) || !maze.HasWall(cell, 3);
            Quaternion along = Quaternion.LookRotation(maze.DirToWorld(openNorthSouth && !openEastWest ? 1 : 0));
            GameObject prop = MazeGenerator.SpawnPrefab(entry.prefab, parent);
            prop.transform.SetPositionAndRotation(maze.CellCenter(cell) + Vector3.up * maze.WallHeight, along);
            SetStaticRecursive(prop);
        }
    }

    void Furnish(MazeGenerator maze, RectInt room, RoomFurnishing furnishing, Transform parent, System.Random rng, HashSet<Vector2Int> furnished)
    {
        if (furnishing == null || furnishing.prefab == null) return;
        foreach (Vector2Int c in room.allPositionsWithin) furnished.Add(c);

        float cs = maze.CellSize;
        Quaternion baseRotation = maze.transform.rotation;

        if (furnishing.layout == RoomLayout.Center)
        {
            
            Quaternion rotation = baseRotation * Quaternion.Euler(0f, (room.height > room.width ? 90f : 0f) + (rng.NextDouble() < 0.5 ? 180f : 0f), 0f);
            GameObject prop = MazeGenerator.SpawnPrefab(furnishing.prefab, parent);
            prop.transform.SetPositionAndRotation(maze.RoomCenter(room), rotation);
            SetStaticRecursive(prop);
            return;
        }

        
        float inset = maze.WallThickness / 2f + furnishing.edgeMargin;
        Vector2 min = new Vector2(room.xMin * cs + inset, room.yMin * cs + inset);
        Vector2 size = new Vector2(room.width * cs, room.height * cs) - Vector2.one * inset * 2f;
        int nx = Mathf.Max(1, Mathf.FloorToInt(size.x / furnishing.spacing.x));
        int nz = Mathf.Max(1, Mathf.FloorToInt(size.y / furnishing.spacing.y));
        Vector2 offset = min + (size - new Vector2(nx * furnishing.spacing.x, nz * furnishing.spacing.y)) / 2f;
        Vector3 spawn = maze.SpawnPoint != null ? maze.SpawnPoint.position : Vector3.positiveInfinity;

        for (int ix = 0; ix < nx; ix++)
        for (int iz = 0; iz < nz; iz++)
        {
            var local = new Vector3(offset.x + (ix + 0.5f) * furnishing.spacing.x, 0f, offset.y + (iz + 0.5f) * furnishing.spacing.y);
            Vector3 position = maze.transform.TransformPoint(local);
            if (Vector3.Distance(Flat(position), Flat(spawn)) < spawnClearance) continue;

            GameObject prop = MazeGenerator.SpawnPrefab(furnishing.prefab, parent);
            prop.transform.SetPositionAndRotation(position, baseRotation * Quaternion.Euler(0f, Range(rng, -2f, 2f), 0f));
            SetStaticRecursive(prop);
        }
    }

    
    static float OppositeIntrusion(MazeGenerator maze, Vector2Int cell, int dir, Dictionary<Vector2Int, float[]> intrusions)
    {
        int opposite = (dir + 2) % 4;
        float deepest = intrusions.TryGetValue(cell, out float[] own) ? own[opposite] : 0f;
        foreach (int side in new[] { (dir + 1) % 4, (dir + 3) % 4 })
        {
            if (maze.HasWall(cell, side)) continue;
            if (intrusions.TryGetValue(cell + MazeGenerator.Directions[side], out float[] next)) deepest = Mathf.Max(deepest, next[opposite]);
        }
        return deepest;
    }

    bool TryPlaceAgainstWall(MazeGenerator maze, Vector2Int cell, int dir, PropEntry entry, float[] intrusion, float oppositeIntrusion,
        Transform parent, System.Random rng)
    {
        Vector3 toWall = maze.DirToWorld(dir);
        Vector3 lateral = maze.DirToWorld((dir + 1) % 4);
        Vector3 center = maze.CellCenter(cell);
        float half = maze.CellSize / 2f - maze.WallThickness / 2f;

        GameObject prop = MazeGenerator.SpawnPrefab(entry.prefab, parent);
        Quaternion rotation = Quaternion.LookRotation(-toWall) * Quaternion.Euler(0f, Range(rng, -maxYawJitter, maxYawJitter), 0f);
        if (rng.NextDouble() < entry.tipOverChance)
            rotation = Quaternion.AngleAxis(rng.NextDouble() < 0.5 ? 90f : -90f, rng.NextDouble() < 0.5 ? lateral : toWall) * rotation;
        prop.transform.SetPositionAndRotation(center, rotation);

        
        Vector3 wallFace = center + toWall * half;
        Bounds b = WorldBounds(prop);
        float reach = Vector3.Dot(b.center - wallFace, toWall) + Extent(b, toWall);
        prop.transform.position += -toWall * (reach + WallGap) + Vector3.up * (center.y - b.min.y);

        
        b = WorldBounds(prop);
        float depth = Extent(b, toWall) * 2f + WallGap;
        if (depth + oppositeIntrusion > maze.CellSize - maze.WallThickness - minLaneWidth)
        {
            MazeGenerator.DestroySafe(prop);
            return false;
        }

        
        float halfWidth = Extent(b, lateral);
        float min = -(half - intrusion[(dir + 3) % 4]) + halfWidth;
        float max = (half - intrusion[(dir + 1) % 4]) - halfWidth;
        if (max < min)
        {
            MazeGenerator.DestroySafe(prop);
            return false;
        }
        float current = Vector3.Dot(b.center - center, lateral);
        prop.transform.position += lateral * (Range(rng, min, max) - current);

        intrusion[dir] = Mathf.Max(intrusion[dir], depth);
        SetStaticRecursive(prop);
        return true;
    }

    void PlaceClutter(MazeGenerator maze, Vector2Int cell, PropEntry entry, Transform parent, System.Random rng)
    {
        if (entry == null) return;
        Vector3 center = maze.CellCenter(cell);
        float half = maze.CellSize / 2f - maze.WallThickness / 2f - 0.05f;

        GameObject item = MazeGenerator.SpawnPrefab(entry.prefab, parent);
        Vector3 offset = new Vector3(Range(rng, -half, half), 0f, Range(rng, -half, half));
        item.transform.SetPositionAndRotation(center + offset, Quaternion.Euler(0f, Range(rng, 0f, 360f), 0f));

        
        Bounds b = WorldBounds(item);
        Vector3 shift = Vector3.zero;
        if (b.max.x > center.x + half) shift.x = center.x + half - b.max.x;
        if (b.min.x + shift.x < center.x - half) shift.x = center.x - half - b.min.x;
        if (b.max.z > center.z + half) shift.z = center.z + half - b.max.z;
        if (b.min.z + shift.z < center.z - half) shift.z = center.z - half - b.min.z;
        item.transform.position += shift;
        SetStaticRecursive(item);
    }

    static PropEntry Pick(List<PropEntry> entries, System.Random rng)
    {
        float total = 0f;
        foreach (PropEntry e in entries) if (e.prefab != null) total += Mathf.Max(0f, e.weight);
        if (total <= 0f) return null;

        float roll = (float)rng.NextDouble() * total;
        foreach (PropEntry e in entries)
        {
            if (e.prefab == null) continue;
            roll -= Mathf.Max(0f, e.weight);
            if (roll <= 0f) return e;
        }
        return null;
    }

    static Bounds WorldBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
        return b;
    }

    
    static float Extent(Bounds b, Vector3 dir) => Mathf.Abs(b.extents.x * dir.x) + Mathf.Abs(b.extents.z * dir.z);

    static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

    static float Range(System.Random rng, float min, float max) => min + (float)rng.NextDouble() * (max - min);

    static void Shuffle(List<int> list, System.Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    
    
    static void SetStaticRecursive(GameObject go)
    {
        foreach (Transform t in go.GetComponentsInChildren<Transform>())
            if (!IsLive(t, go.transform)) t.gameObject.isStatic = true;
    }

    static bool IsLive(Transform t, Transform root)
    {
        for (; t != null && t != root; t = t.parent)
            if (t.name == "Live") return true;
        return false;
    }
}
