using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;






public static class PropModels
{
    public const string Root = "Assets/Models/Props";
    static readonly string[] ModelExtensions = { ".fbx", ".obj", ".dae" };

    
    
    static readonly Dictionary<string, (string key, string set)[]> SetHints = new Dictionary<string, (string, string)[]>
    {
        ["VintageBoiler"] = new[] { ("exhaust", "BoilerExhaustPipe"), ("pipe", "BoilerExhaustPipe"), ("detail", "BoilerDetail"), ("body", "BoilerBody") },
        ["Lockers"] = new[] { ("2", "Locker2"), ("1", "Locker1") },
        ["FileCabinets"] = new[] { ("3", "Cabinet3Drawer"), ("2", "Cabinet2Drawer") },
        ["PlantPot"] = new[] { ("soil", "PlantSoil"), ("dirt", "PlantSoil"), ("ground", "PlantSoil"), ("leaf", "PlantLeaves"), ("leav", "PlantLeaves"), ("plant", "PlantLeaves") },
    };

    
    
    static readonly Dictionary<string, (Color color, float smooth, float metal)> SolidColours = new Dictionary<string, (Color, float, float)>
    {
        ["LeatherChair"] = (new Color(0.11f, 0.065f, 0.045f), 0.5f, 0f),
        ["MeetingTable"] = (new Color(0.27f, 0.12f, 0.07f), 0.55f, 0f),
        ["Gurney"] = (new Color(0.62f, 0.64f, 0.66f), 0.6f, 0.8f),
        ["Wheelchair"] = (new Color(0.58f, 0.6f, 0.62f), 0.55f, 0.7f),
        ["FilingCabinet"] = (new Color(0.24f, 0.26f, 0.28f), 0.4f, 0.5f),
    };

    
    static readonly string[] JunkNames = { "camera", "light", "smoke", "domain", "plane", "floor", "ground" };

    public static IEnumerable<string> AllModelPaths() =>
        Directory.GetDirectories(Root).OrderBy(d => d)
            .SelectMany(d => Directory.GetFiles(d).Where(f => ModelExtensions.Contains(Path.GetExtension(f).ToLowerInvariant())))
            .Select(p => p.Replace('\\', '/'));

    public static string ModelPath(string name) =>
        AllModelPaths().FirstOrDefault(p => Path.GetFileName(Path.GetDirectoryName(p)) == name);

    public static bool Has(string name) => ModelPath(name) != null;

    
    public static void ApplyImportSettings()
    {
        AssetDatabase.Refresh();
        foreach (string path in AllModelPaths())
        {
            if (!(AssetImporter.GetAtPath(path) is ModelImporter mi)) continue;
            bool dirty = false;
            void Set<T>(T current, T wanted, System.Action<T> apply) { if (!EqualityComparer<T>.Default.Equals(current, wanted)) { apply(wanted); dirty = true; } }
            Set(mi.importAnimation, false, v => mi.importAnimation = v);
            Set(mi.animationType, ModelImporterAnimationType.None, v => mi.animationType = v);
            Set(mi.isReadable, true, v => mi.isReadable = v);
            Set(mi.importCameras, false, v => mi.importCameras = v);
            Set(mi.importLights, false, v => mi.importLights = v);
            Set(mi.addCollider, false, v => mi.addCollider = v);
            Set(mi.materialImportMode, ModelImporterMaterialImportMode.ImportViaMaterialDescription, v => mi.materialImportMode = v);
            if (dirty) mi.SaveAndReimport();
        }
        foreach (string dir in Directory.GetDirectories(Root))
        foreach (string tex in Directory.GetFiles(dir, "*.png"))
        {
            if (!(AssetImporter.GetAtPath(tex.Replace('\\', '/')) is TextureImporter ti)) continue;
            bool normal = tex.EndsWith("_Normal.png");
            bool dirty = false;
            if (normal && ti.textureType != TextureImporterType.NormalMap) { ti.textureType = TextureImporterType.NormalMap; dirty = true; }
            if (ti.maxTextureSize != 1024) { ti.maxTextureSize = 1024; dirty = true; }
            if (dirty) ti.SaveAndReimport();
        }
        AssetDatabase.Refresh();
    }

    
    public static GameObject Instantiate(string modelPathOrName)
    {
        string path = modelPathOrName.Contains("/") ? modelPathOrName : ModelPath(modelPathOrName);
        var asset = path != null ? AssetDatabase.LoadAssetAtPath<GameObject>(path) : null;
        if (asset == null) return null;
        var go = (GameObject)Object.Instantiate(asset);
        go.name = Path.GetFileNameWithoutExtension(path);
        string name = Path.GetFileName(Path.GetDirectoryName(path));
        BakeSkinned(go, name);
        RemoveJunk(go);
        ApplyMaterials(go, name);
        return go;
    }

    
    static void BakeSkinned(GameObject go, string name)
    {
        foreach (SkinnedMeshRenderer smr in go.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            string meshPath = $"{Root}/{name}/{name}_{smr.name}_Baked.asset";
            var baked = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (baked == null)
            {
                baked = new Mesh { name = $"{name}_{smr.name}_Baked" };
                smr.BakeMesh(baked, true);
                baked.RecalculateBounds();
                AssetDatabase.CreateAsset(baked, meshPath);
            }
            var holder = new GameObject(smr.name + " Mesh");
            holder.transform.SetParent(smr.transform.parent, false);
            holder.transform.SetPositionAndRotation(smr.transform.position, smr.transform.rotation);
            holder.AddComponent<MeshFilter>().sharedMesh = baked;
            holder.AddComponent<MeshRenderer>().sharedMaterials = smr.sharedMaterials;
            Object.DestroyImmediate(smr.gameObject.GetComponent<SkinnedMeshRenderer>());
        }
    }

    
    
    static void RemoveJunk(GameObject go)
    {
        foreach (Camera c in go.GetComponentsInChildren<Camera>(true)) Object.DestroyImmediate(c.gameObject);
        foreach (Light l in go.GetComponentsInChildren<Light>(true)) Object.DestroyImmediate(l.gameObject);
        Renderer[] rs = go.GetComponentsInChildren<Renderer>(true);
        if (rs.Length < 2) return;
        float[] extents = rs.Select(r => r.bounds.size.magnitude).OrderBy(x => x).ToArray();
        float median = extents[extents.Length / 2];
        foreach (Renderer r in rs)
        {
            if (r == null) continue;
            string n = r.name.ToLowerInvariant();
            Vector3 size = r.bounds.size;
            bool flatAndHuge = size.magnitude > median * 8f && Mathf.Min(size.x, size.y, size.z) < size.magnitude * 0.02f;
            if (flatAndHuge || JunkNames.Any(j => n.StartsWith(j))) Object.DestroyImmediate(r.gameObject);
        }
    }

    static void ApplyMaterials(GameObject go, string name)
    {
        Dictionary<string, Material> sets = Materials(name);
        if (sets.Count == 0 && SolidColours.TryGetValue(name, out var solid)) sets[name] = SolidMaterial(name, solid.color, solid.smooth, solid.metal);
        if (sets.Count == 0) return;
        SetHints.TryGetValue(name, out var hints);
        Material fallback = sets.Values.First();
        foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
        {
            Material[] mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                string slot = ((mats[i] != null ? mats[i].name : "") + " " + r.name).ToLowerInvariant();
                Material chosen = fallback;
                if (hints != null)
                    foreach (var (key, set) in hints)
                        if (slot.Contains(key) && sets.TryGetValue(set, out Material m)) { chosen = m; break; }
                mats[i] = chosen;
            }
            r.sharedMaterials = mats;
        }
    }

    static Material SolidMaterial(string name, Color color, float smoothness, float metallic)
    {
        string matPath = $"{Root}/{name}/{name}_Solid.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
            AssetDatabase.CreateAsset(mat, matPath);
        }
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Smoothness", smoothness);
        mat.SetFloat("_Metallic", metallic);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    
    static Dictionary<string, Material> Materials(string name)
    {
        var result = new Dictionary<string, Material>();
        string dir = $"{Root}/{name}";
        foreach (string albedoPath in Directory.GetFiles(dir, "*_Albedo.png").OrderBy(p => p))
        {
            string set = Path.GetFileNameWithoutExtension(albedoPath).Replace("_Albedo", "");
            string matPath = $"{dir}/{set}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = set };
                AssetDatabase.CreateAsset(mat, matPath);
            }
            mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath.Replace('\\', '/')));
            mat.SetColor("_BaseColor", Color.white);
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>($"{dir}/{set}_Normal.png");
            if (normal != null)
            {
                mat.SetTexture("_BumpMap", normal);
                mat.EnableKeyword("_NORMALMAP");
            }
            mat.SetFloat("_Metallic", 0f);
            mat.SetFloat("_Smoothness", 0.35f);
            EditorUtility.SetDirty(mat);
            result[set] = mat;
        }
        return result;
    }

    public static Bounds Bounds(GameObject go)
    {
        Renderer[] rs = go.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        Bounds b = rs[0].bounds;
        foreach (Renderer r in rs) b.Encapsulate(r.bounds);
        return b;
    }

    
    
    
    
    public static Transform Place(Transform parent, string name, Vector3 bottomCentre, Vector3 maxSize, float yaw = 0f, Vector3 baseEuler = default, string keep = null)
    {
        GameObject go = Instantiate(name);
        if (go == null) return null;
        
        if (keep != null)
            foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
                if (!r.name.Contains(keep) && !r.transform.parent.name.Contains(keep)) Object.DestroyImmediate(r.gameObject);
        var pivot = new GameObject(name).transform;
        pivot.SetParent(parent, false);
        go.transform.SetParent(pivot, false);
        
        go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f) * Quaternion.Euler(baseEuler) * go.transform.localRotation;

        Bounds b = LocalBounds(pivot);
        float scale = float.MaxValue;
        if (maxSize.x > 0f && b.size.x > 1e-4f) scale = Mathf.Min(scale, maxSize.x / b.size.x);
        if (maxSize.y > 0f && b.size.y > 1e-4f) scale = Mathf.Min(scale, maxSize.y / b.size.y);
        if (maxSize.z > 0f && b.size.z > 1e-4f) scale = Mathf.Min(scale, maxSize.z / b.size.z);
        if (scale == float.MaxValue) scale = 1f;
        go.transform.localScale *= scale;
        go.transform.localPosition *= scale;

        b = LocalBounds(pivot);
        go.transform.localPosition += bottomCentre - new Vector3(b.center.x, b.min.y, b.center.z);
        return pivot;
    }

    
    public static Bounds LocalBounds(Transform t)
    {
        bool any = false;
        var b = new Bounds();
        Matrix4x4 toLocal = t.worldToLocalMatrix;
        foreach (MeshFilter mf in t.GetComponentsInChildren<MeshFilter>())
        {
            if (mf.sharedMesh == null) continue;
            Matrix4x4 m = toLocal * mf.transform.localToWorldMatrix;
            Bounds mb = mf.sharedMesh.bounds;
            for (int i = 0; i < 8; i++)
            {
                Vector3 c = mb.center + Vector3.Scale(mb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                Vector3 p = m.MultiplyPoint3x4(c);
                if (!any) { b = new Bounds(p, Vector3.zero); any = true; }
                else b.Encapsulate(p);
            }
        }
        return b;
    }
}
