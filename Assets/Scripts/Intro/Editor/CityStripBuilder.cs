using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;









public static class CityStripBuilder
{
    public const string ModelPath = IntroModels.Dir + "/City/City.fbx";
    public const string StripPath = LevelAssetFactory.IntroDir + "/CityStrip.prefab";
    public const string TowerPath = LevelAssetFactory.IntroDir + "/CityTower.prefab";
    public const string ParkTowerPath = LevelAssetFactory.IntroDir + "/CityTowerB.prefab";
    const string MeshDir = "Assets/Prefabs/Level/Meshes";

    
    
    
    static readonly string[] RoofPieces = { "Mesh1315", "Mesh1317", "Mesh1318" };

    
    static readonly Vector4 ParkRect = new Vector4(-106f, -74f, -220f, -149f);
    static readonly Vector4 TowerRect = new Vector4(-106f, -80f, -291f, -250f);

    
    
    
    static readonly (bool tower, Vector3 at, float yaw, Vector3 scale)[] Plots =
    {
        
        (true, new Vector3(-84.5f, 0f, -316.4f), 90f, Vector3.one),                       
        (true, new Vector3(-97f, 0f, -233.7f), 0f, new Vector3(0.6f, 0.5f, 0.45f)),       
        (false, new Vector3(-90.1f, 0f, -103.6f), 0f, Vector3.one),                        
        
        (true, new Vector3(-174.5f, 0f, -316.4f), -90f, Vector3.one),                      
        (true, new Vector3(-166.6f, 0f, -268.2f), 180f, Vector3.one),                      
        (false, new Vector3(-169.5f, 0f, -204.7f), 180f, Vector3.one),                     
        (false, new Vector3(-169.5f, 0f, -103.6f), 180f, Vector3.one),                     
    };

    public static GameObject Build()
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        if (model == null)
        {
            Debug.LogError("Missing city model " + ModelPath);
            return null;
        }

        var strip = (GameObject)PrefabUtility.InstantiatePrefab(model);
        PrefabUtility.UnpackPrefabInstance(strip, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        strip.name = "CityStrip";
        foreach (Transform t in strip.GetComponentsInChildren<Transform>(true))
            if (t.name == "Mesh2089") { Object.DestroyImmediate(t.gameObject); break; }

        var renderers = strip.GetComponentsInChildren<MeshRenderer>(true).ToList();
        foreach (MeshRenderer r in renderers)
        {
            r.sharedMaterials = r.sharedMaterials.Select(m => LevelAssetFactory.CityMaterial(m != null ? m.name : "")).ToArray();
            r.shadowCastingMode = ShadowCastingMode.Off;
        }

        
        GameObject tower = ExtractBuilding(strip, renderers, TowerRect, "CityTower", TowerPath, out Vector3 towerPivot, true);
        GameObject park = ExtractBuilding(strip, renderers, ParkRect, "CityTowerB", ParkTowerPath, out Vector3 parkPivot, false);

        Place(strip.transform, tower, towerPivot, 0f, Vector3.one);
        Place(strip.transform, park, parkPivot, 0f, Vector3.one);
        foreach (var plot in Plots) Place(strip.transform, plot.tower ? tower : park, plot.at, plot.yaw, plot.scale);

        MakeStatic(strip);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(strip, StripPath);
        Object.DestroyImmediate(strip);
        return prefab;
    }

    static void Place(Transform parent, GameObject prefab, Vector3 at, float yaw, Vector3 scale)
    {
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        instance.transform.localPosition = at;
        instance.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        instance.transform.localScale = scale;
    }

    
    
    static GameObject ExtractBuilding(GameObject strip, List<MeshRenderer> renderers, Vector4 rect, string name, string path, out Vector3 pivot, bool roofColliders)
    {
        var pieces = renderers.Where(r => r != null && InRect(r.bounds.center, rect) && !IsGround(r)).ToList();
        Bounds footprint = pieces[0].bounds;
        foreach (MeshRenderer r in pieces) footprint.Encapsulate(r.bounds);
        pivot = new Vector3(footprint.center.x, 0f, footprint.center.z);

        var root = new GameObject(name);
        root.transform.position = pivot;

        var windows = pieces.Where(IsWindow).GroupBy(r => r.sharedMaterial).ToList();
        foreach (var group in windows)
        {
            var combine = new List<CombineInstance>();
            foreach (MeshRenderer r in group)
                combine.Add(new CombineInstance { mesh = r.GetComponent<MeshFilter>().sharedMesh, transform = root.transform.worldToLocalMatrix * r.transform.localToWorldMatrix });
            var mesh = new Mesh { name = $"Intro_{name}_{group.Key.name}", indexFormat = IndexFormat.UInt32 };
            mesh.CombineMeshes(combine.ToArray(), true, true);
            mesh.RecalculateBounds();

            var part = new GameObject(group.Key.name);
            part.transform.SetParent(root.transform, false);
            part.AddComponent<MeshFilter>().sharedMesh = SaveMesh(mesh);
            var renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = group.Key;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        
        
        
        var backing = new List<CombineInstance>();
        var cube = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        foreach (MeshRenderer r in pieces.Where(IsWindow))
        {
            Bounds b = r.bounds;
            Vector3 size = b.size;
            int thin = size.x < size.y ? (size.x < size.z ? 0 : 2) : (size.y < size.z ? 1 : 2);
            size[thin] = 0.05f;
            backing.Add(new CombineInstance { mesh = cube, transform = root.transform.worldToLocalMatrix * Matrix4x4.TRS(b.center, Quaternion.identity, size) });
        }
        if (backing.Count > 0)
        {
            var mesh = new Mesh { name = $"Intro_{name}_Backing", indexFormat = IndexFormat.UInt32 };
            mesh.CombineMeshes(backing.ToArray(), true, true);
            mesh.RecalculateBounds();
            var part = new GameObject("Window Panes");
            part.transform.SetParent(root.transform, false);
            part.AddComponent<MeshFilter>().sharedMesh = SaveMesh(mesh);
            var renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = LevelAssetFactory.CityMaterial("Backing");
            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        foreach (MeshRenderer r in pieces)
        {
            if (IsWindow(r))
            {
                Object.DestroyImmediate(r.gameObject);
                continue;
            }
            r.transform.SetParent(root.transform, true);
            if (roofColliders && RoofPieces.Contains(r.name) && r.GetComponent<MeshCollider>() == null)
                r.gameObject.AddComponent<MeshCollider>();
        }

        MakeStatic(root);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    static bool InRect(Vector3 p, Vector4 r) => p.x > r.x && p.x < r.y && p.z > r.z && p.z < r.w;

    
    static bool IsGround(Renderer r) => r.name == "Mesh1" || r.name == "Mesh2" || r.name == "Mesh2019";

    static bool IsWindow(Renderer r)
    {
        string material = r.sharedMaterial != null ? r.sharedMaterial.name : "";
        return material == "City_Windows" && r.bounds.size.x * r.bounds.size.y * r.bounds.size.z < 20f;
    }

    
    static Mesh SaveMesh(Mesh mesh)
    {
        if (!Directory.Exists(MeshDir)) Directory.CreateDirectory(MeshDir);
        string path = $"{MeshDir}/{mesh.name}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing != null)
        {
            EditorUtility.CopySerialized(mesh, existing);
            Object.DestroyImmediate(mesh);
            return existing;
        }
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    static void MakeStatic(GameObject root)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (!PrefabUtility.IsPartOfPrefabInstance(t.gameObject))
                GameObjectUtility.SetStaticEditorFlags(t.gameObject,
                    StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
    }
}
