using System.Collections.Generic;
using UnityEngine;





public class MazeGenerator : MonoBehaviour
{
    
    public static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

    const string GeneratedRootName = "Generated";

    [SerializeField] LevelTheme theme;

    [Header("Layout")]
    [SerializeField] int width = 19;
    [SerializeField] int depth = 19;
    [SerializeField] float cellSize = 4f;
    [SerializeField] float wallThickness = 0.2f;
    [SerializeField] int seed = 1;
    [Tooltip("Chance to knock out each extra wall, creating loops so there is more than one route")]
    [Range(0f, 1f)] [SerializeField] float loopChance = 0.08f;

    [Header("Pacing")]
    [Tooltip("Walking distance from spawn to exit along the shortest route, cutting corners like a player does. " +
             "Player walks 5 m/s, so 175 m is about 35 s")]
    [SerializeField] float targetWalkLength = 175f;
    [Tooltip("If the route misses the target by more than this, try the next seed (up to 30 tries)")]
    [SerializeField] float routeTolerance = 12f;

    [Tooltip("Scene the exit door loads (the next level). Leave empty to only fire the exit event.")]
    [SerializeField] string nextScene;
    [Tooltip("Zombie pressure for this level. 1 = baseline (level 5); ZombieSpawner scales its spawn rate and cap by it.")]
    [SerializeField] float difficulty = 1f;

    [Header("Zombie spawn points")]
    [SerializeField] int zombieSpawnCount = 8;
    [SerializeField] float minSpawnDistanceFromStart = 24f;

    
    [SerializeField, HideInInspector] Vector2Int gridSize;
    [SerializeField, HideInInspector] bool[] northWalls; 
    [SerializeField, HideInInspector] bool[] eastWalls;  
    [SerializeField, HideInInspector] bool[] roomCells;
    [SerializeField, HideInInspector] RectInt startRoom;
    [SerializeField, HideInInspector] RectInt goalRoom;
    
    [SerializeField, HideInInspector] List<Vector3Int> windowSegments = new List<Vector3Int>();
    [SerializeField, HideInInspector] List<RectInt> rooms = new List<RectInt>();
    [SerializeField, HideInInspector] Vector2Int startCell;
    [SerializeField, HideInInspector] Vector2Int exitCell;
    [SerializeField, HideInInspector] List<Vector3> solutionPath = new List<Vector3>();
    [SerializeField, HideInInspector] Transform spawnPoint;
    [SerializeField, HideInInspector] List<Transform> zombieSpawnPoints = new List<Transform>();

    public LevelTheme Theme { get => theme; set => theme = value; }
    public string NextScene { get => nextScene; set => nextScene = value; }
    public float Difficulty => difficulty;
    public int Seed { get => seed; set => seed = value; }
    public Vector2Int GridSize => gridSize;
    public float CellSize => cellSize;
    public float WallHeight => theme != null ? theme.wallHeight : 3f;
    
    public RectInt StartRoom => startRoom;
    
    public RectInt GoalRoom => goalRoom;
    public IReadOnlyList<RectInt> Rooms => rooms;
    public float WallThickness => wallThickness;
    public Vector2Int StartCell => startCell;
    public Vector2Int ExitCell => exitCell;
    public Transform SpawnPoint => spawnPoint;
    public IReadOnlyList<Transform> ZombieSpawnPoints => zombieSpawnPoints;
    
    public IReadOnlyList<Vector3> SolutionPath => solutionPath;
    public bool IsGenerated => northWalls != null && northWalls.Length == gridSize.x * gridSize.y && gridSize.x > 0;

    
    public float RouteLength
    {
        get
        {
            float length = 0f;
            for (int i = 1; i < solutionPath.Count; i++) length += Vector3.Distance(solutionPath[i - 1], solutionPath[i]);
            return length;
        }
    }

    
    [SerializeField, HideInInspector] float walkLength;
    public float WalkLength => walkLength;

    
    const float BodyRadius = 0.5f;

    public void Generate()
    {
        int[] distances, parents;
        System.Random rng;
        int attempts = 0, bestSeed = seed;
        float bestMiss = float.MaxValue;
        while (true)
        {
            rng = new System.Random(seed);
            distances = GenerateLayout(rng, out parents);
            float miss = Mathf.Abs(WalkLength - targetWalkLength);
            if (miss < bestMiss) { bestMiss = miss; bestSeed = seed; }
            if (miss <= routeTolerance) break;
            if (++attempts >= 30)
            {
                
                seed = bestSeed;
                rng = new System.Random(seed);
                distances = GenerateLayout(rng, out parents);
                break;
            }
            seed++;
        }

        Clear();
        BuildStructure();
        PlaceSpawnPoint();
        PlaceExit(parents);
        PlaceZombieSpawnPoints(distances, rng);

        float walk = WalkLength;
        Debug.Log($"Maze generated (seed {seed}): shortest walk about {walk:0} m, {walk / 5f:0} s at walk speed.", this);
        if (Mathf.Abs(walk - targetWalkLength) > routeTolerance)
            Debug.LogWarning($"Walk is {walk:0} m but target is {targetWalkLength:0} m. Try a bigger or smaller grid.", this);
    }

    
    int[] GenerateLayout(System.Random rng, out int[] parents)
    {
        gridSize = new Vector2Int(Mathf.Max(2, width), Mathf.Max(2, depth));
        int count = gridSize.x * gridSize.y;
        northWalls = new bool[count];
        eastWalls = new bool[count];
        roomCells = new bool[count];
        for (int i = 0; i < count; i++) northWalls[i] = eastWalls[i] = true;
        startCell = Vector2Int.zero;
        rooms.Clear();

        CarvePassages(rng);
        CarveStartRoom();
        CarveGoalRoom();
        CarveRooms(rng);
        AddLoops(rng);

        int[] distances = DistancesFrom(startCell, out parents);
        exitCell = PickExitCell(distances, parents);
        BuildSolutionPath(parents);
        walkLength = EstimateWalk(CellPath(Index(exitCell), parents));
        return distances;
    }

    public void Clear()
    {
        Transform root = transform.Find(GeneratedRootName);
        if (root != null) DestroySafe(root.gameObject);
        spawnPoint = null;
        zombieSpawnPoints.Clear();
    }

    

    public bool InBounds(Vector2Int c) => c.x >= 0 && c.y >= 0 && c.x < gridSize.x && c.y < gridSize.y;

    public bool IsRoomCell(Vector2Int c) => InBounds(c) && roomCells[Index(c)];

    public bool HasWall(Vector2Int c, int dir)
    {
        Vector2Int n = c + Directions[dir];
        if (!InBounds(n)) return true;
        switch (dir)
        {
            case 0: return northWalls[Index(c)];
            case 1: return eastWalls[Index(c)];
            case 2: return northWalls[Index(n)];
            default: return eastWalls[Index(n)];
        }
    }

    
    public bool IsWindow(Vector2Int c, int dir)
    {
        switch (dir)
        {
            case 0: return windowSegments.Contains(new Vector3Int(0, c.y + 1, c.x));
            case 2: return windowSegments.Contains(new Vector3Int(0, c.y, c.x));
            case 1: return windowSegments.Contains(new Vector3Int(1, c.x + 1, c.y));
            default: return windowSegments.Contains(new Vector3Int(1, c.x, c.y));
        }
    }

    public int WallCount(Vector2Int c)
    {
        int walls = 0;
        for (int d = 0; d < 4; d++) if (HasWall(c, d)) walls++;
        return walls;
    }

    public Vector3 CellCenter(Vector2Int c) =>
        transform.TransformPoint(new Vector3((c.x + 0.5f) * cellSize, 0f, (c.y + 0.5f) * cellSize));

    public Vector3 DirToWorld(int dir) =>
        transform.TransformDirection(new Vector3(Directions[dir].x, 0f, Directions[dir].y));

    
    public Transform ResetGeneratedChild(string childName)
    {
        Transform root = transform.Find(GeneratedRootName);
        if (root == null)
        {
            root = new GameObject(GeneratedRootName).transform;
            root.SetParent(transform, false);
        }
        Transform old = root.Find(childName);
        if (old != null) DestroySafe(old.gameObject);
        var child = new GameObject(childName).transform;
        child.SetParent(root, false);
        return child;
    }

    

    int Index(Vector2Int c) => c.y * gridSize.x + c.x;
    Vector2Int CellAt(int index) => new Vector2Int(index % gridSize.x, index / gridSize.x);

    void SetWall(Vector2Int c, int dir, bool value)
    {
        Vector2Int n = c + Directions[dir];
        if (!InBounds(n)) return;
        switch (dir)
        {
            case 0: northWalls[Index(c)] = value; break;
            case 1: eastWalls[Index(c)] = value; break;
            case 2: northWalls[Index(n)] = value; break;
            default: eastWalls[Index(n)] = value; break;
        }
    }

    
    void CarvePassages(System.Random rng)
    {
        var visited = new bool[gridSize.x * gridSize.y];
        var stack = new Stack<Vector2Int>();
        var options = new List<int>(4);
        visited[Index(startCell)] = true;
        stack.Push(startCell);

        while (stack.Count > 0)
        {
            Vector2Int c = stack.Peek();
            options.Clear();
            for (int d = 0; d < 4; d++)
            {
                Vector2Int n = c + Directions[d];
                if (InBounds(n) && !visited[Index(n)]) options.Add(d);
            }
            if (options.Count == 0) { stack.Pop(); continue; }

            int dir = options[rng.Next(options.Count)];
            Vector2Int next = c + Directions[dir];
            SetWall(c, dir, false);
            visited[Index(next)] = true;
            stack.Push(next);
        }
    }

    
    void CarveStartRoom()
    {
        Vector2Int size = theme != null ? theme.startRoomSize : Vector2Int.zero;
        size = Vector2Int.Min(size, gridSize - Vector2Int.one * 2);
        startRoom = size.x > 0 && size.y > 0 ? new RectInt(0, 0, size.x, size.y) : new RectInt();
        if (startRoom.width > 0) OpenRoom(startRoom);
    }

    
    void CarveGoalRoom()
    {
        Vector2Int size = theme != null && theme.goalPrefab != null ? theme.goalRoomSize : Vector2Int.zero;
        size = Vector2Int.Min(size, gridSize - Vector2Int.one * 2);
        goalRoom = size.x > 0 && size.y > 0 ? new RectInt(gridSize.x - size.x, gridSize.y - size.y, size.x, size.y) : new RectInt();
        if (goalRoom.width > 0) OpenRoom(goalRoom);
    }

    void CarveRooms(System.Random rng)
    {
        int roomCount = theme != null ? theme.roomCount : 4;
        Vector2Int sizeRange = theme != null ? theme.roomSizeRange : new Vector2Int(2, 3);
        for (int i = 0; i < roomCount; i++)
        {
            
            for (int attempt = 0; attempt < 20; attempt++)
            {
                int sx = Mathf.Min(rng.Next(sizeRange.x, sizeRange.y + 1), gridSize.x - 1);
                int sz = Mathf.Min(rng.Next(sizeRange.x, sizeRange.y + 1), gridSize.y - 1);
                
                var rect = new RectInt(rng.Next(1, gridSize.x - sx + 1), rng.Next(1, gridSize.y - sz + 1), sx, sz);
                if (Overlaps(rect)) continue;

                OpenRoom(rect);
                rooms.Add(rect);
                break;
            }
        }
    }

    
    bool Overlaps(RectInt rect)
    {
        var padded = new RectInt(rect.x - 1, rect.y - 1, rect.width + 2, rect.height + 2);
        if (startRoom.width > 0 && padded.Overlaps(startRoom)) return true;
        if (goalRoom.width > 0 && padded.Overlaps(goalRoom)) return true;
        foreach (RectInt other in rooms) if (padded.Overlaps(other)) return true;
        return false;
    }

    void OpenRoom(RectInt rect)
    {
        for (int x = rect.xMin; x < rect.xMax; x++)
        for (int z = rect.yMin; z < rect.yMax; z++)
        {
            var c = new Vector2Int(x, z);
            roomCells[Index(c)] = true;
            if (x < rect.xMax - 1) SetWall(c, 1, false);
            if (z < rect.yMax - 1) SetWall(c, 0, false);
        }
    }

    
    public Vector3 RoomCenter(RectInt rect) =>
        transform.TransformPoint(new Vector3(rect.center.x * cellSize, 0f, rect.center.y * cellSize));

    void AddLoops(System.Random rng)
    {
        for (int x = 0; x < gridSize.x; x++)
        for (int z = 0; z < gridSize.y; z++)
        {
            var c = new Vector2Int(x, z);
            if (rng.NextDouble() < loopChance) SetWall(c, 0, false);
            if (rng.NextDouble() < loopChance) SetWall(c, 1, false);
        }
    }

    
    int[] DistancesFrom(Vector2Int from, out int[] parents)
    {
        int count = gridSize.x * gridSize.y;
        var dist = new int[count];
        parents = new int[count];
        for (int i = 0; i < count; i++) { dist[i] = -1; parents[i] = -1; }

        var queue = new Queue<Vector2Int>();
        dist[Index(from)] = 0;
        queue.Enqueue(from);
        while (queue.Count > 0)
        {
            Vector2Int c = queue.Dequeue();
            for (int d = 0; d < 4; d++)
            {
                if (HasWall(c, d)) continue;
                Vector2Int n = c + Directions[d];
                if (dist[Index(n)] >= 0) continue;
                dist[Index(n)] = dist[Index(c)] + 1;
                parents[Index(n)] = Index(c);
                queue.Enqueue(n);
            }
        }
        return dist;
    }

    
    
    Vector2Int PickExitCell(int[] distances, int[] parents)
    {
        if (goalRoom.width > 0) return new Vector2Int(goalRoom.x + goalRoom.width / 2, goalRoom.y + goalRoom.height / 2);
        if (theme != null && theme.goalPrefab != null && rooms.Count > 0)
        {
            Vector2Int bestRoom = startCell;
            float bestRoomScore = float.MinValue;
            foreach (RectInt room in rooms)
            {
                var c = new Vector2Int(room.x + room.width / 2, room.y + room.height / 2);
                if (distances[Index(c)] <= 0) continue;
                float score = -Mathf.Abs(EstimateWalk(CellPath(Index(c), parents)) - targetWalkLength);
                if (score > bestRoomScore) { bestRoomScore = score; bestRoom = c; }
            }
            if (bestRoom != startCell) return bestRoom;
        }

        Vector2Int best = startCell;
        float bestScore = float.MinValue;
        for (int i = 0; i < distances.Length; i++)
        {
            Vector2Int c = CellAt(i);
            if (distances[i] <= 0 || roomCells[i]) continue;

            
            if (distances[i] * cellSize < targetWalkLength - routeTolerance) continue;
            float walk = EstimateWalk(CellPath(i, parents));
            float score = -Mathf.Abs(walk - targetWalkLength);
            if (WallCount(c) == 3) score += cellSize;
            if (score > bestScore) { bestScore = score; best = c; }
        }
        
        if (best == startCell)
            for (int i = 0; i < distances.Length; i++)
                if (distances[i] > distances[Index(best)] && !roomCells[i]) best = CellAt(i);
        return best;
    }

    List<Vector2Int> CellPath(int index, int[] parents)
    {
        var cells = new List<Vector2Int>();
        for (int i = index; i >= 0; i = parents[i]) cells.Add(CellAt(i));
        cells.Reverse();
        return cells;
    }

    
    
    float EstimateWalk(List<Vector2Int> cells)
    {
        int n = cells.Count;
        if (n < 2) return 0f;
        var lefts = new Vector2[n + 1];
        var rights = new Vector2[n + 1];
        Vector2 start = CellPoint(cells[0]);
        Vector2 end = CellPoint(cells[n - 1]);
        lefts[0] = rights[0] = start;
        float halfGap = cellSize / 2f - wallThickness / 2f - BodyRadius;
        for (int i = 1; i < n; i++)
        {
            Vector2 step = cells[i] - cells[i - 1];
            Vector2 mid = (CellPoint(cells[i]) + CellPoint(cells[i - 1])) / 2f;
            var left = new Vector2(-step.y, step.x);
            lefts[i] = mid + left * halfGap;
            rights[i] = mid - left * halfGap;
        }
        lefts[n] = rights[n] = end;

        float length = 0f;
        Vector2 apex = start, portalLeft = start, portalRight = start;
        int apexIndex = 0, leftIndex = 0, rightIndex = 0;
        for (int i = 1; i <= n; i++)
        {
            Vector2 l = lefts[i], r = rights[i];

            
            if (Cross(apex, portalRight, r) >= 0f)
            {
                if (apex == portalRight || Cross(apex, portalLeft, r) < 0f) { portalRight = r; rightIndex = i; }
                else
                {
                    
                    length += Vector2.Distance(apex, portalLeft);
                    apex = portalRight = portalLeft;
                    apexIndex = rightIndex = leftIndex;
                    i = apexIndex;
                    continue;
                }
            }

            
            if (Cross(apex, portalLeft, l) <= 0f)
            {
                if (apex == portalLeft || Cross(apex, portalRight, l) > 0f) { portalLeft = l; leftIndex = i; }
                else
                {
                    length += Vector2.Distance(apex, portalRight);
                    apex = portalLeft = portalRight;
                    apexIndex = leftIndex = rightIndex;
                    i = apexIndex;
                    continue;
                }
            }
        }
        return length + Vector2.Distance(apex, end);
    }

    Vector2 CellPoint(Vector2Int c) => new Vector2((c.x + 0.5f) * cellSize, (c.y + 0.5f) * cellSize);

    
    static float Cross(Vector2 a, Vector2 b, Vector2 c) => (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);

    void BuildSolutionPath(int[] parents)
    {
        solutionPath.Clear();
        for (int i = Index(exitCell); i >= 0; i = parents[i]) solutionPath.Add(CellCenter(CellAt(i)));
        solutionPath.Reverse();
    }

    

    void BuildStructure()
    {
        Transform structure = ResetGeneratedChild("Structure");
        float w = gridSize.x * cellSize;
        float d = gridSize.y * cellSize;
        float wallHeight = WallHeight;
        Material wallMaterial = theme != null ? theme.wallMaterial : null;

        CreateBlock("Floor", structure, new Vector3(w / 2f, -0.1f, d / 2f), new Vector3(w, 0.2f, d),
            theme != null ? theme.floorMaterial : null, new Vector2(w, d));
        GameObject ceiling = CreateBlock("Ceiling", structure, new Vector3(w / 2f, wallHeight + 0.1f, d / 2f), new Vector3(w, 0.2f, d),
            theme != null ? theme.ceilingMaterial : null, new Vector2(w, d));
        
        ceiling.AddComponent<Unity.AI.Navigation.NavMeshModifier>().ignoreFromBuild = true;

        HashSet<Vector3Int> windows = PickWindows();

        
        for (int k = 0; k <= gridSize.y; k++)
        {
            int runStart = -1;
            for (int x = 0; x <= gridSize.x; x++)
            {
                bool wall = x < gridSize.x && (k == 0 || k == gridSize.y || northWalls[Index(new Vector2Int(x, k - 1))])
                    && !windows.Contains(new Vector3Int(0, k, x));
                if (wall && runStart < 0) runStart = x;
                else if (!wall && runStart >= 0)
                {
                    float length = (x - runStart) * cellSize + wallThickness;
                    CreateBlock("Wall", structure, new Vector3((runStart + x) * 0.5f * cellSize, wallHeight / 2f, k * cellSize),
                        new Vector3(length, wallHeight, wallThickness), wallMaterial, new Vector2(length, wallHeight));
                    runStart = -1;
                }
            }
        }

        
        for (int k = 0; k <= gridSize.x; k++)
        {
            int runStart = -1;
            for (int z = 0; z <= gridSize.y; z++)
            {
                bool wall = z < gridSize.y && (k == 0 || k == gridSize.x || eastWalls[Index(new Vector2Int(k - 1, z))])
                    && !windows.Contains(new Vector3Int(1, k, z));
                if (wall && runStart < 0) runStart = z;
                else if (!wall && runStart >= 0)
                {
                    float length = (z - runStart) * cellSize + wallThickness;
                    CreateBlock("Wall", structure, new Vector3(k * cellSize, wallHeight / 2f, (runStart + z) * 0.5f * cellSize),
                        new Vector3(wallThickness, wallHeight, length), wallMaterial, new Vector2(length, wallHeight));
                    runStart = -1;
                }
            }
        }
    }

    
    
    HashSet<Vector3Int> PickWindows()
    {
        var windows = new HashSet<Vector3Int>();
        windowSegments.Clear();
        if (theme == null || theme.windowChance <= 0f || theme.windows.Count == 0) return windows;

        var rng = new System.Random(seed + 5);
        void Consider(int axis, int line, int along, Vector2Int inside)
        {
            if (inside == startCell || inside == exitCell) return;
            if (rng.NextDouble() < theme.windowChance) windows.Add(new Vector3Int(axis, line, along));
        }
        for (int x = 0; x < gridSize.x; x++)
        {
            Consider(0, 0, x, new Vector2Int(x, 0));
            Consider(0, gridSize.y, x, new Vector2Int(x, gridSize.y - 1));
        }
        for (int z = 0; z < gridSize.y; z++)
        {
            Consider(1, 0, z, new Vector2Int(0, z));
            Consider(1, gridSize.x, z, new Vector2Int(gridSize.x - 1, z));
        }

        Transform structure = transform.Find(GeneratedRootName + "/Structure");
        Transform frames = ResetGeneratedChild("Windows");
        windowSegments.AddRange(windows);
        foreach (Vector3Int w in windows)
        {
            bool windowBefore = windows.Contains(new Vector3Int(w.x, w.y, w.z - 1));
            bool windowAfter = windows.Contains(new Vector3Int(w.x, w.y, w.z + 1));
            BuildWindow(w, windowBefore, windowAfter, structure, frames, rng);
        }
        return windows;
    }

    
    
    
    void BuildWindow(Vector3Int w, bool windowBefore, bool windowAfter, Transform structure, Transform frames, System.Random rng)
    {
        bool alongX = w.x == 0;
        float wallHeight = WallHeight;
        Material wallMaterial = theme.wallMaterial;
        Vector3 center = alongX ? new Vector3((w.z + 0.5f) * cellSize, 0f, w.y * cellSize) : new Vector3(w.y * cellSize, 0f, (w.z + 0.5f) * cellSize);
        Vector3 along = alongX ? Vector3.right : Vector3.forward;
        Vector3 inward = alongX ? (w.y == 0 ? Vector3.forward : Vector3.back) : (w.y == 0 ? Vector3.right : Vector3.left);

        float open = cellSize * theme.windowWidth;
        float top = wallHeight - theme.windowHeader;
        Vector3 Size(float length, float height) => alongX ? new Vector3(length, height, wallThickness) : new Vector3(wallThickness, height, length);

        foreach (int side in new[] { -1, 1 })
        {
            bool windowNext = side < 0 ? windowBefore : windowAfter;
            float outer = cellSize / 2f - (windowNext ? 0f : wallThickness / 2f);
            float pier = outer - open / 2f;
            if (pier <= 0.001f) continue;
            CreateBlock("Wall", structure, center + along * side * (open / 2f + pier / 2f) + Vector3.up * wallHeight / 2f, Size(pier, wallHeight), wallMaterial, new Vector2(pier, wallHeight));
        }
        CreateBlock("Wall", structure, center + Vector3.up * theme.windowSill / 2f, Size(open, theme.windowSill), wallMaterial, new Vector2(open, theme.windowSill));
        CreateBlock("Wall", structure, center + Vector3.up * (top + theme.windowHeader / 2f), Size(open, theme.windowHeader), wallMaterial, new Vector2(open, theme.windowHeader));

        GameObject prefab = PickWeighted(theme.windows, rng);
        if (prefab == null) return;
        GameObject window = SpawnPrefab(prefab, frames);
        window.transform.SetPositionAndRotation(transform.TransformPoint(center + Vector3.up * theme.windowSill),
            transform.rotation * Quaternion.LookRotation(inward));
    }

    static GameObject PickWeighted(List<PropEntry> entries, System.Random rng)
    {
        float total = 0f;
        foreach (PropEntry e in entries) if (e.prefab != null) total += Mathf.Max(0f, e.weight);
        float roll = (float)rng.NextDouble() * total;
        foreach (PropEntry e in entries)
        {
            if (e.prefab == null) continue;
            roll -= Mathf.Max(0f, e.weight);
            if (roll <= 0f) return e.prefab;
        }
        return null;
    }

    GameObject CreateBlock(string blockName, Transform parent, Vector3 localPosition, Vector3 size, Material material, Vector2 faceSize)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = blockName;
        block.isStatic = true;
        block.transform.SetParent(parent, false);
        block.transform.localPosition = localPosition;
        block.transform.localScale = size;
        if (material != null) block.GetComponent<Renderer>().sharedMaterial = material;
        block.AddComponent<TiledSurface>().sizeMeters = faceSize;
        return block;
    }

    void PlaceSpawnPoint()
    {
        Transform markers = ResetGeneratedChild("Markers");
        spawnPoint = new GameObject("Spawn Point").transform;
        spawnPoint.SetParent(markers, false);
        spawnPoint.position = CellCenter(startCell);

        
        Vector3 facing = startRoom.width > 0 ? RoomCenter(startRoom) - spawnPoint.position
            : solutionPath.Count > 1 ? solutionPath[1] - solutionPath[0] : transform.forward;
        facing.y = 0f;
        if (facing.sqrMagnitude > 0.001f) spawnPoint.rotation = Quaternion.LookRotation(facing);
    }

    void PlaceExit(int[] parents)
    {
        if (theme != null && theme.goalPrefab != null)
        {
            PlaceGoal(parents);
            return;
        }

        GameObject exitPrefab = theme != null ? theme.exitPrefab : null;
        if (exitPrefab == null)
        {
            Debug.LogWarning("No exit prefab assigned, exit was not placed.", this);
            return;
        }

        
        int doorDir = -1;
        int parent = parents[Index(exitCell)];
        if (parent >= 0)
        {
            Vector2Int entry = exitCell - CellAt(parent);
            for (int d = 0; d < 4; d++)
                if (Directions[d] == entry && HasWall(exitCell, d)) doorDir = d;
        }
        for (int d = 0; doorDir < 0 && d < 4; d++)
            if (HasWall(exitCell, d)) doorDir = d;
        if (doorDir < 0) doorDir = 0;

        Transform parentTransform = ResetGeneratedChild("Exit");
        GameObject exit = SpawnPrefab(exitPrefab, parentTransform);
        Vector3 toWall = DirToWorld(doorDir);
        
        exit.transform.position = CellCenter(exitCell) + toWall * (cellSize / 2f - wallThickness / 2f);
        exit.transform.rotation = Quaternion.LookRotation(-toWall);

        ExitDoor door = exit.GetComponentInChildren<ExitDoor>();
        if (door != null)
        {
            door.NextSceneName = nextScene;
#if UNITY_EDITOR
            
            if (!Application.isPlaying) UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(door);
#endif
        }
    }

    
    void PlaceGoal(int[] parents)
    {
        Transform parentTransform = ResetGeneratedChild("Exit");
        Vector3 goalPos = CellCenter(exitCell);
        Vector3 towardEntrance = transform.forward;
        int parent = parents[Index(exitCell)];
        if (parent >= 0)
        {
            towardEntrance = CellCenter(CellAt(parent)) - goalPos;
            towardEntrance.y = 0f;
            towardEntrance.Normalize();
        }

        
        
        if (TryGetGoalBackPlacement(out Vector3 backPosition, out Vector3 backFacing))
        {
            goalPos = new Vector3(backPosition.x, goalPos.y, backPosition.z);
            towardEntrance = backFacing;
        }

        GameObject goal = SpawnPrefab(theme.goalPrefab, parentTransform);
        goal.transform.SetPositionAndRotation(goalPos, Quaternion.LookRotation(towardEntrance));

        
        if (theme.bossPrefab == null) return;
        
        Transform bossGroup = ResetGeneratedChild("Boss");
        bossGroup.gameObject.AddComponent<Unity.AI.Navigation.NavMeshModifier>().ignoreFromBuild = true;
        GameObject boss = SpawnPrefab(theme.bossPrefab, bossGroup);
        
        if (goalRoom.width > 0 && boss.TryGetComponent(out BossZombie guard))
        {
            var arenaSize = new Vector3(goalRoom.width * cellSize, WallHeight, goalRoom.height * cellSize);
            guard.SetArena(RoomCenter(goalRoom), arenaSize);
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(guard);
#endif
        }
        
        float lift = theme.bossPrefab.TryGetComponent(out UnityEngine.AI.NavMeshAgent agent) ? agent.baseOffset * theme.bossPrefab.transform.localScale.y : 1f;
        boss.transform.SetPositionAndRotation(goalPos + towardEntrance * 3.5f + Vector3.up * lift, Quaternion.LookRotation(towardEntrance));
    }

    
    public const float GoalBackWallClearance = 2.7f;

    
    
    public bool TryGetGoalBackPlacement(out Vector3 position, out Vector3 facing)
    {
        position = default;
        facing = Vector3.forward;
        if (goalRoom.width <= 0) return false;

        Vector3 centre = RoomCenter(goalRoom);
        Vector3 away = centre - CellCenter(startCell);
        away.y = 0f;
        bool alongX = Mathf.Abs(away.x) > Mathf.Abs(away.z);
        Vector3 backDirection = alongX ? new Vector3(Mathf.Sign(away.x), 0f, 0f) : new Vector3(0f, 0f, away.z < 0f ? -1f : 1f);
        float half = (alongX ? goalRoom.width : goalRoom.height) * cellSize * 0.5f;
        position = centre + backDirection * (half - GoalBackWallClearance);
        facing = -backDirection;
        return true;
    }

    void PlaceZombieSpawnPoints(int[] distances, System.Random rng)
    {
        var candidates = new List<Vector2Int>();
        for (int i = 0; i < distances.Length; i++)
        {
            Vector2Int c = CellAt(i);
            if (c == exitCell || distances[i] * cellSize < minSpawnDistanceFromStart) continue;
            candidates.Add(c);
        }

        
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }
        candidates.Sort((a, b) => (WallCount(b) == 3).CompareTo(WallCount(a) == 3));

        Transform markers = transform.Find(GeneratedRootName + "/Markers");
        var group = new GameObject("Zombie Spawn Points").transform;
        group.SetParent(markers, false);
        for (int i = 0; i < Mathf.Min(zombieSpawnCount, candidates.Count); i++)
        {
            var point = new GameObject($"Zombie Spawn {i + 1}").transform;
            point.SetParent(group, false);
            point.position = CellCenter(candidates[i]);
            zombieSpawnPoints.Add(point);
        }
    }

    

    public static GameObject SpawnPrefab(GameObject prefab, Transform parent)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, parent);
#endif
        return Instantiate(prefab, parent);
    }

    public static void DestroySafe(Object obj)
    {
        if (Application.isPlaying) Destroy(obj);
        else DestroyImmediate(obj);
    }

    void OnDrawGizmosSelected()
    {
        if (solutionPath.Count < 2) return;
        Gizmos.color = Color.green;
        for (int i = 1; i < solutionPath.Count; i++)
            Gizmos.DrawLine(solutionPath[i - 1] + Vector3.up * 0.2f, solutionPath[i] + Vector3.up * 0.2f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(solutionPath[solutionPath.Count - 1] + Vector3.up, 0.5f);
    }
}
