using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;









public static class PropModelInstaller
{
    const string RigPath = "Assets/Prefabs/Player Rig.prefab";
    const string BatteryPath = "Assets/Prefabs/Battery.prefab";
    const string LevelFolder = "Assets/Scenes/Levels";

    
    static readonly (string weapon, string model, float yaw)[] Guns =
    {
        ("Pistol", "Pistol", 0f),
        ("Shotgun", "Shotgun", 90f),
        ("Rifle", "Rifle", 180f),
    };

    [MenuItem("Losing Power/Install Prop Models")]
    public static void Install()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("Stop play mode first."); return; }
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        string previous = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
        InstallAll();
        if (!string.IsNullOrEmpty(previous)) EditorSceneManager.OpenScene(previous);
    }

    public static void RunBatch()
    {
        InstallAll();
        string dir = PropModelPreview.ArgAfter("-previewDir");
        if (dir != null) RenderChecks(dir);
        Debug.Log("PROP MODELS INSTALLED");
    }

    static void InstallAll()
    {
        PropModels.ApplyImportSettings();
        LevelAssetFactory.RebuildModelProps();
        InstallViewmodels();
        InstallBattery();
        AssetDatabase.SaveAssets();
        RebakeLevels();
    }

    static void InstallViewmodels()
    {
        GameObject rig = PrefabUtility.LoadPrefabContents(RigPath);
        try
        {
            foreach (var (weapon, model, yaw) in Guns)
            {
                Transform root = rig.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == weapon && t.parent != null && t.parent.name == "Weapons");
                Transform modelRoot = root != null ? root.Find("Model") : null;
                if (modelRoot == null) { Debug.LogWarning($"Install Prop Models: no {weapon}/Model in the Player Rig"); continue; }
                if (PropModels.Has(model)) Swap(modelRoot, model, yaw, t => !IsFlashlightPart(t) && t.name != "Muzzle Flash" && t.name != "Flashlight Mount");
                if (PropModels.Has("Flashlight")) Swap(modelRoot, "Flashlight", -90f, IsFlashlightPart);
            }
            PrefabUtility.SaveAsPrefabAsset(rig, RigPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(rig); }
    }

    static bool IsFlashlightPart(Transform t) => t.name.StartsWith("Light ");

    
    
    static void Swap(Transform modelRoot, string model, float yaw, System.Func<Transform, bool> select)
    {
        if (modelRoot.Find(model) != null) return;
        
        var parts = modelRoot.Cast<Transform>().Where(t => select(t) && t.GetComponentInChildren<MeshRenderer>(true) != null).ToList();
        if (parts.Count == 0) return;

        Bounds? box = null;
        Matrix4x4 toRoot = modelRoot.worldToLocalMatrix;
        foreach (MeshFilter mf in parts.SelectMany(t => t.GetComponentsInChildren<MeshFilter>(true)))
        {
            Mesh mesh = mf.sharedMesh;
            if (mesh == null) continue;
            Matrix4x4 m = toRoot * mf.transform.localToWorldMatrix;
            for (int i = 0; i < 8; i++)
            {
                Vector3 c = mesh.bounds.center + Vector3.Scale(mesh.bounds.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                Vector3 p = m.MultiplyPoint3x4(c);
                if (box == null) box = new Bounds(p, Vector3.zero);
                else { Bounds b = box.Value; b.Encapsulate(p); box = b; }
            }
        }
        if (box == null) return;
        Bounds space = box.Value;
        int layer = parts[0].gameObject.layer;
        foreach (Transform part in parts) Object.DestroyImmediate(part.gameObject);

        Transform placed = PropModels.Place(modelRoot, model, new Vector3(space.center.x, space.min.y, space.center.z), space.size, yaw);
        foreach (Transform t in placed.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = layer;
        foreach (Renderer r in placed.GetComponentsInChildren<Renderer>(true)) r.shadowCastingMode = ShadowCastingMode.Off;
    }

    static void InstallBattery()
    {
        if (!PropModels.Has("Battery")) return;
        GameObject battery = PrefabUtility.LoadPrefabContents(BatteryPath);
        try
        {
            Transform visual = battery.transform.Find("Visual");
            if (visual != null && visual.Find("Battery") == null)
            {
                Swap(visual, "Battery", 0f, t => true);
                PrefabUtility.SaveAsPrefabAsset(battery, BatteryPath);
            }
        }
        finally { PrefabUtility.UnloadPrefabContents(battery); }
    }

    
    
    static void RebakeLevels()
    {
        foreach (string path in Directory.GetFiles(LevelFolder, "Level_*.unity").OrderBy(p => p))
        {
            EditorSceneManager.OpenScene(path.Replace('\\', '/'));
            var maze = Object.FindAnyObjectByType<MazeGenerator>();
            if (maze == null) continue;
            MazeLevelBuilder.BakeNavMesh(maze);
            MazeLevelBuilder.RepairBlockedCorridors(maze);
            EditorSceneManager.SaveScene(maze.gameObject.scene);
            Debug.Log($"Re-baked {path}");
        }
    }

    static void RenderChecks(string dir)
    {
        Directory.CreateDirectory(dir);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        foreach (string path in LevelAssetFactory.ModelPrefabPaths())
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            PropModelPreview.RenderSheet(go, Path.Combine(dir, "Prefab_" + Path.GetFileNameWithoutExtension(path) + ".png"));
            Debug.Log($"CHECK {path}: size {PropModels.Bounds(go).size}");
            Object.DestroyImmediate(go);
        }
        GameObject rig = PrefabUtility.LoadPrefabContents(RigPath);
        foreach (var (weapon, _, _) in Guns)
        {
            Transform model = rig.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == weapon && t.parent != null && t.parent.name == "Weapons")?.Find("Model");
            if (model == null) continue;
            var copy = Object.Instantiate(model.gameObject);
            copy.SetActive(true);
            foreach (Transform t in copy.GetComponentsInChildren<Transform>(true)) { t.gameObject.layer = 0; t.gameObject.SetActive(true); }
            PropModelPreview.RenderSheet(copy, Path.Combine(dir, "Viewmodel_" + weapon + ".png"));
            Object.DestroyImmediate(copy);
        }
        PrefabUtility.UnloadPrefabContents(rig);
        var bat = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BatteryPath));
        PropModelPreview.RenderSheet(bat, Path.Combine(dir, "Prefab_Battery.png"));
        Object.DestroyImmediate(bat);
    }
}
