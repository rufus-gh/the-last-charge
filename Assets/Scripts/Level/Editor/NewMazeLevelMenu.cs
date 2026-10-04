using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;




public static class NewMazeLevelMenu
{
    const string LevelFolder = "Assets/Scenes/Levels";

    const string PlayerRigPath = "Assets/Prefabs/Player Rig.prefab";

    
    
    
    static readonly (string theme, int grid, float walk, int seed, float difficulty)[] Levels =
    {
        ("Trading", 19, 175f, 2002, 0.68f),
        ("Hospital", 19, 180f, 4004, 0.76f),
        ("ServerRoom", 20, 190f, 5005, 0.84f),
        ("Archive", 20, 195f, 3003, 0.92f),
        ("Office", 20, 200f, 1025, 1.00f),
        ("Furnace", 21, 205f, 6006, 1.05f),
        ("Laboratory", 21, 215f, 7007, 1.10f),
    };

    [MenuItem("Losing Power/Build All Levels")]
    static void BuildAllLevels()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        BuildAll();
        EditorUtility.DisplayDialog("Levels built",
            $"Level_01 to Level_{Levels.Length:00} are in Assets/Scenes/Levels, each with the Player Rig. Open MainMenu and press Play.", "OK");
    }

    public static void BuildAll()
    {
        LevelAssetFactory.EnsureAssets();
        for (int i = 0; i < Levels.Length; i++)
        {
            var spec = Levels[i];
            string path = $"{LevelFolder}/Level_{i + 1:00}.unity";
            string next = i + 1 < Levels.Length ? $"Level_{i + 2:00}" : "";

            MazeGenerator maze = OpenOrCreateLevel(path);
            var so = new SerializedObject(maze);
            so.FindProperty("theme").objectReferenceValue = LevelAssetFactory.LoadTheme(spec.theme);
            so.FindProperty("width").intValue = spec.grid;
            so.FindProperty("depth").intValue = spec.grid;
            so.FindProperty("targetWalkLength").floatValue = spec.walk;
            so.FindProperty("seed").intValue = spec.seed;
            so.FindProperty("nextScene").stringValue = next;
            so.FindProperty("difficulty").floatValue = spec.difficulty;
            so.ApplyModifiedPropertiesWithoutUndo();

            SetUpBatteries(maze);

            MazeLevelBuilder.Build(maze);
            EditorSceneManager.SaveScene(maze.gameObject.scene);
            AddToBuildSettings(path);
            Debug.Log($"Built {path} ({spec.theme}): walk about {maze.WalkLength:0} m.");
        }
    }

    [MenuItem("Losing Power/Create New Maze Level")]
    static void CreateLevel()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        if (!AssetDatabase.IsValidFolder(LevelFolder)) AssetDatabase.CreateFolder("Assets/Scenes", "Levels");
        string path = EditorUtility.SaveFilePanelInProject("New maze level", NextLevelName(), "unity", "Name the level scene", LevelFolder);
        if (string.IsNullOrEmpty(path)) return;

        GameObject level = CreateLevelAt(path);
        Selection.activeGameObject = level;
        EditorUtility.DisplayDialog("Maze level created",
            "Pick a theme on Maze Level and press Generate. To play it, copy Player and HUD in from SampleScene, " +
            "then press Generate again to move the player to the spawn point.", "OK");
    }

    public static GameObject CreateLevelAt(string path)
    {
        MazeGenerator maze = OpenOrCreateLevel(path);
        maze.Seed = Random.Range(1, 100000);
        MazeLevelBuilder.Build(maze);
        EditorSceneManager.SaveScene(maze.gameObject.scene);
        AddToBuildSettings(path);
        return maze.gameObject;
    }

    
    static MazeGenerator OpenOrCreateLevel(string path)
    {
        if (!AssetDatabase.IsValidFolder(LevelFolder)) AssetDatabase.CreateFolder("Assets/Scenes", "Levels");
        LevelAssetFactory.EnsureAssets();

        if (System.IO.File.Exists(path))
        {
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var existing = Object.FindAnyObjectByType<MazeGenerator>();
            if (existing != null)
            {
                AddPlacers(existing.gameObject);
                return existing;
            }
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("Atmosphere").AddComponent<LevelAtmosphere>();

        var volume = new GameObject("Global Volume").AddComponent<Volume>();
        volume.isGlobal = true;
        volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(LevelAssetFactory.VolumeProfilePath);

        var level = new GameObject("Maze Level");
        var maze = level.AddComponent<MazeGenerator>();
        AddPlacers(level);
        SetUpBatteries(maze);

        
        var rig = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerRigPath);
        if (rig != null) PrefabUtility.InstantiatePrefab(rig, scene);
        var surface = level.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Children;
        
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

        
        EditorSceneManager.SaveScene(scene, path);
        return maze;
    }

    
    
    const string BatteryPrefabPath = "Assets/Prefabs/Battery.prefab";
    const float BatteryFreeRadius = 30f;

    static void SetUpBatteries(MazeGenerator maze)
    {
        if (!maze.TryGetComponent(out BatteryPlacer batteries)) batteries = maze.gameObject.AddComponent<BatteryPlacer>();
        var so = new SerializedObject(batteries);
        if (so.FindProperty("batteryPrefab").objectReferenceValue == null)
            so.FindProperty("batteryPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(BatteryPrefabPath);
        so.FindProperty("minDistanceFromStart").floatValue = BatteryFreeRadius;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    
    
    static void AddPlacers(GameObject level)
    {
        Ensure<PropScatter>(level);
        Ensure<CeilingLightPlacer>(level);
        Ensure<CobwebPlacer>(level);
        Ensure<PipeRunPlacer>(level);
        if (Ensure<GrimeDecals>(level, out var grime))
        {
            var so = new SerializedObject(grime);
            SetMaterials(so.FindProperty("puddleMaterials"), "Puddle1", "Puddle2", "Puddle3");
            SetMaterials(so.FindProperty("floorBloodMaterials"), "BloodFloor1", "BloodFloor2", "BloodFloor3");
            SetMaterials(so.FindProperty("wallBloodMaterials"), "BloodWall1", "BloodWall2");
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        
        if (level.TryGetComponent(out MonitorStatic oldStatic)) Object.DestroyImmediate(oldStatic);
    }

    static void Ensure<T>(GameObject go) where T : Component => Ensure<T>(go, out _);

    
    static bool Ensure<T>(GameObject go, out T component) where T : Component
    {
        component = go.GetComponent<T>();
        if (component != null) return false;
        component = go.AddComponent<T>();
        return true;
    }

    static void SetMaterials(SerializedProperty list, params string[] names)
    {
        list.arraySize = names.Length;
        for (int i = 0; i < names.Length; i++)
            list.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>($"Assets/Materials/Grime/{names[i]}.mat");
    }

    static string NextLevelName()
    {
        int n = 1;
        while (System.IO.File.Exists($"{LevelFolder}/Level_{n:00}.unity")) n++;
        return $"Level_{n:00}";
    }

    
    static void AddToBuildSettings(string path)
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.Any(s => s.path == path)) return;
        int lastLevel = scenes.FindLastIndex(s => s.path.StartsWith(LevelFolder + "/"));
        scenes.Insert(lastLevel >= 0 ? lastLevel + 1 : scenes.Count, new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
