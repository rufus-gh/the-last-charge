using System.Collections.Generic;
using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;


[CustomEditor(typeof(MazeGenerator))]
public class MazeGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var maze = (MazeGenerator)target;

        EditorGUILayout.Space();
        if (maze.IsGenerated)
            EditorGUILayout.HelpBox($"Shortest walk: about {maze.WalkLength:0} m ({maze.WalkLength / 5f:0} s at walk speed). " +
                                    "Select this object to see the route as a green line in the Scene view.", MessageType.Info);

        if (GUILayout.Button("Generate", GUILayout.Height(28))) MazeLevelBuilder.Build(maze);
        if (GUILayout.Button("Generate With New Seed"))
        {
            Undo.RecordObject(maze, "New maze seed");
            maze.Seed = Random.Range(1, 100000);
            MazeLevelBuilder.Build(maze);
        }
        if (GUILayout.Button("Clear"))
        {
            maze.Clear();
            EditorSceneManager.MarkSceneDirty(maze.gameObject.scene);
        }
    }
}


public static class MazeLevelBuilder
{
    public static void Build(MazeGenerator maze)
    {
        LevelAssetFactory.AssignDefaults(maze);

        maze.Generate();
        if (maze.TryGetComponent(out PropScatter props)) props.Scatter(maze);
        if (maze.TryGetComponent(out CeilingLightPlacer lights)) lights.Place(maze);
        if (maze.TryGetComponent(out PipeRunPlacer pipes)) pipes.Place(maze);
        if (maze.TryGetComponent(out CobwebPlacer cobwebs)) cobwebs.Place(maze);
        if (!maze.TryGetComponent(out CityBackdrop city)) city = Undo.AddComponent<CityBackdrop>(maze.gameObject);
        city.Build(maze);
        ApplyAtmosphere(maze);
        BakeNavMesh(maze);
        RepairBlockedCorridors(maze);
        
        if (maze.TryGetComponent(out BatteryPlacer batteries)) batteries.Place(maze);
        if (maze.TryGetComponent(out GrimeDecals grime)) grime.Place(maze);
        LogNavMeshWalk(maze);
        HookUpPlayer(maze);

        EditorUtility.SetDirty(maze);
        EditorSceneManager.MarkSceneDirty(maze.gameObject.scene);
    }

    
    
    static void ApplyAtmosphere(MazeGenerator maze)
    {
        var atmosphere = Object.FindAnyObjectByType<LevelAtmosphere>();
        if (atmosphere == null || maze.Theme == null) return;
        LevelTheme theme = maze.Theme;
        var so = new SerializedObject(atmosphere);
        so.FindProperty("ambientColor").colorValue = theme.ambientColor;
        so.FindProperty("fogColor").colorValue = theme.fogColor;
        so.FindProperty("fogDensity").floatValue = theme.fogDensity;
        so.FindProperty("skybox").objectReferenceValue = theme.outdoors ? theme.skybox : null;
        so.ApplyModifiedProperties();

        Transform sun = atmosphere.transform.Find("Sun");
        if (!theme.outdoors)
        {
            if (sun != null) Undo.DestroyObjectImmediate(sun.gameObject);
            return;
        }
        if (sun == null)
        {
            sun = new GameObject("Sun").transform;
            Undo.RegisterCreatedObjectUndo(sun.gameObject, "Add sun");
            sun.SetParent(atmosphere.transform, false);
        }
        sun.rotation = Quaternion.Euler(theme.sunEuler);
        if (!sun.TryGetComponent(out Light light)) light = sun.gameObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = theme.sunColor;
        light.intensity = theme.sunIntensity;
        light.shadows = LightShadows.Soft;
    }

    public static void BakeNavMesh(MazeGenerator maze)
    {
        if (!maze.TryGetComponent(out NavMeshSurface surface)) return;
        surface.BuildNavMesh();

        string scenePath = maze.gameObject.scene.path;
        if (string.IsNullOrEmpty(scenePath))
        {
            Debug.LogWarning("Save the scene first, then Generate again, so the NavMesh can be saved next to it.");
            return;
        }

        
        string assetPath = Path.ChangeExtension(scenePath, null) + "_NavMesh.asset";
        AssetDatabase.DeleteAsset(assetPath);
        AssetDatabase.CreateAsset(surface.navMeshData, assetPath);
        EditorUtility.SetDirty(surface);
    }

    
    
    
    internal static void RepairBlockedCorridors(MazeGenerator maze)
    {
        Transform props = maze.transform.Find("Generated/Props");
        if (props == null) return;

        for (int pass = 0; pass < 4; pass++)
        {
            
            Physics.SyncTransforms();
            var crowded = new HashSet<GameObject>();
            var path = new NavMeshPath();
            for (int x = 0; x < maze.GridSize.x; x++)
            for (int z = 0; z < maze.GridSize.y; z++)
            {
                var cell = new Vector2Int(x, z);
                if (maze.IsRoomCell(cell)) continue;
                for (int dir = 0; dir < 2; dir++)
                {
                    Vector2Int next = cell + MazeGenerator.Directions[dir];
                    if (!maze.InBounds(next) || maze.IsRoomCell(next) || maze.HasWall(cell, dir)) continue;
                    if (!FloorPoint(maze.CellCenter(cell), out Vector3 a) || !FloorPoint(maze.CellCenter(next), out Vector3 b)) continue;
                    if (NavMesh.CalculatePath(a, b, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete) continue;

                    
                    Vector3 doorway = (maze.CellCenter(cell) + maze.CellCenter(next)) / 2f;
                    foreach (Transform prop in props)
                        if (prop.TryGetComponent(out Collider c) && !c.isTrigger &&
                            c.bounds.SqrDistance(doorway) < maze.CellSize * maze.CellSize * 0.36f)
                            crowded.Add(prop.gameObject);
                }
            }
            if (crowded.Count == 0) return;

            foreach (GameObject prop in crowded) Object.DestroyImmediate(prop);
            Debug.Log($"Cleared {crowded.Count} props that were blocking corridors; re-baking the NavMesh.", maze);
            BakeNavMesh(maze);
        }
    }

    
    static bool FloorPoint(Vector3 p, out Vector3 floor)
    {
        floor = p;
        if (!NavMesh.SamplePosition(p, out NavMeshHit hit, 1.6f, NavMesh.AllAreas) || Mathf.Abs(hit.position.y - p.y) > 0.3f) return false;
        floor = hit.position;
        return true;
    }

    
    static void LogNavMeshWalk(MazeGenerator maze)
    {
        if (maze.SpawnPoint == null || maze.SolutionPath.Count < 2) return;
        if (!NavMesh.SamplePosition(maze.SpawnPoint.position, out NavMeshHit from, 2f, NavMesh.AllAreas) ||
            !NavMesh.SamplePosition(maze.SolutionPath[maze.SolutionPath.Count - 1], out NavMeshHit to, 2f, NavMesh.AllAreas))
            return;

        var path = new NavMeshPath();
        NavMesh.CalculatePath(from.position, to.position, NavMesh.AllAreas, path);
        if (path.status != NavMeshPathStatus.PathComplete)
        {
            Debug.LogError("No walkable path from spawn to exit. Something is blocking the route.", maze);
            return;
        }
        float length = 0f;
        for (int i = 1; i < path.corners.Length; i++) length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        Debug.Log($"NavMesh check: spawn to exit is {length:0} m of walking, about {length / 5f:0} s.", maze);
    }

    static void HookUpPlayer(MazeGenerator maze)
    {
        var player = Object.FindAnyObjectByType<CharacterController>();
        if (player == null || maze.SpawnPoint == null)
        {
            Debug.Log("No player in this scene yet. Copy Player and HUD from SampleScene, then press Generate again.");
            return;
        }

        
        Undo.RecordObject(player.transform, "Move player to spawn");
        float feetOffset = player.height / 2f - player.center.y + 0.05f;
        player.transform.SetPositionAndRotation(maze.SpawnPoint.position + Vector3.up * feetOffset, maze.SpawnPoint.rotation);

        
        Component health = player.GetComponent("PlayerHealth");
        if (health == null) return;
        var so = new SerializedObject(health);
        SerializedProperty spawn = so.FindProperty("spawnPoint");
        if (spawn == null) return;
        spawn.objectReferenceValue = maze.SpawnPoint;
        so.ApplyModifiedProperties();
    }
}
