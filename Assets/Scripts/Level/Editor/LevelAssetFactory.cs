using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;






public static partial class LevelAssetFactory
{
    const string MaterialRoot = "Assets/Materials/Level";
    const string PrefabRoot = "Assets/Prefabs/Level";
    const string MeshDir = "Assets/Prefabs/Level/Meshes";
    const string TextureDir = "Assets/Textures/Level";
    const string ThemeDir = "Assets/Settings/LevelThemes";
    public const string VolumeProfilePath = "Assets/Settings/LevelVolumeProfile.asset";

    
    public static readonly string[] ThemeNames = { "Trading", "Hospital", "ServerRoom", "Archive", "Office", "Furnace", "Laboratory" };

    
    
    const string LiveGroup = "Live";

    static bool overwrite;

    [MenuItem("Losing Power/Rebuild Placeholder Level Assets")]
    static void RebuildAll()
    {
        if (!EditorUtility.DisplayDialog("Rebuild level assets",
                "Overwrite the placeholder textures, materials, prefabs and themes in the Level folders?", "Rebuild", "Cancel"))
            return;
        Build(true);
    }

    
    public static void EnsureAssets() => Build(false);

    public static void Build(bool overwriteExisting)
    {
        overwrite = overwriteExisting;
        foreach (string dir in new[] { MaterialRoot, PrefabRoot, MeshDir, TextureDir, ThemeDir }) EnsureFolder(dir);

        CreateTextures();
        BuildWindowAssets();
        BuildCobwebs();
        BuildExitDoor();
        BuildZombieModels();
        BuildFinale();
        BuildOffice();
        BuildWindowPrefabs();
        BuildTrading();
        BuildArchive();
        BuildHospital();
        BuildServerRoom();
        BuildFurnace();
        BuildLaboratory();
        
        BuildIntro();
        CreateVolumeProfile();

        AssetDatabase.SaveAssets();
        FixEmissionKeywords();
    }

    public static LevelTheme LoadTheme(string name) => Load<LevelTheme>($"{ThemeDir}/{name}.asset");

    
    public static void AssignDefaults(MazeGenerator maze)
    {
        EnsureAssets();
        if (maze.Theme != null) return;
        Undo.RecordObject(maze, "Assign level theme");
        maze.Theme = LoadTheme(ThemeNames[0]);
        EditorUtility.SetDirty(maze);
    }

    static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);

    

    static void SaveTheme(string name, Action<LevelTheme> fill)
    {
        string path = $"{ThemeDir}/{name}.asset";
        var theme = Load<LevelTheme>(path);
        if (theme != null && !overwrite) return;

        bool isNew = theme == null;
        if (isNew) theme = ScriptableObject.CreateInstance<LevelTheme>();
        fill(theme);
        theme.exitPrefab = Load<GameObject>($"{PrefabRoot}/ExitDoor.prefab");
        if (isNew) AssetDatabase.CreateAsset(theme, path);
        EditorUtility.SetDirty(theme);
    }

    static PropEntry Entry(string prefabPath, float weight = 1f, float tip = 0f) =>
        new PropEntry { prefab = Load<GameObject>(prefabPath), weight = weight, tipOverChance = tip };

    static RoomFurnishing Furnishing(string prefabPath, RoomLayout layout, Vector2 spacing = default, float edgeMargin = 1.4f) =>
        new RoomFurnishing { prefab = Load<GameObject>(prefabPath), layout = layout, spacing = spacing, edgeMargin = edgeMargin };

    

    
    enum Surface { Opaque, Cutout, Transparent }

    static Material Mat(string name, Color color, float smoothness, Color? emission = null, float metallic = 0f,
        Texture2D baseMap = null, Vector2? perMeter = null, Texture2D emissionMap = null, Surface surface = Surface.Opaque, bool doubleSided = false)
    {
        string path = $"{MaterialRoot}/{name}.mat";
        EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
        var mat = Load<Material>(path);
        if (mat != null && !overwrite) return mat;

        bool isNew = mat == null;
        if (isNew) mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Smoothness", smoothness);
        mat.SetFloat("_Metallic", metallic);
        mat.SetTexture("_BaseMap", baseMap);
        mat.SetTextureScale("_BaseMap", perMeter ?? Vector2.one);
        SetSurface(mat, surface, doubleSided);
        if (emission.HasValue)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emission.Value);
            mat.SetTexture("_EmissionMap", emissionMap);
            
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        else
        {
            mat.DisableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.black);
        }

        if (isNew) AssetDatabase.CreateAsset(mat, path);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    
    
    static void SetSurface(Material mat, Surface surface, bool doubleSided)
    {
        bool transparent = surface == Surface.Transparent, cutout = surface == Surface.Cutout;
        mat.SetFloat("_Surface", transparent ? 1f : 0f);
        mat.SetFloat("_Blend", 0f);
        mat.SetFloat("_AlphaClip", cutout ? 1f : 0f);
        mat.SetFloat("_Cutoff", 0.5f);
        mat.SetFloat("_SrcBlend", transparent ? (float)BlendMode.SrcAlpha : (float)BlendMode.One);
        mat.SetFloat("_DstBlend", transparent ? (float)BlendMode.OneMinusSrcAlpha : (float)BlendMode.Zero);
        mat.SetFloat("_ZWrite", transparent ? 0f : 1f);
        mat.SetFloat("_Cull", doubleSided ? 0f : 2f);
        if (transparent) mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); else mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        if (cutout) mat.EnableKeyword("_ALPHATEST_ON"); else mat.DisableKeyword("_ALPHATEST_ON");
        mat.SetOverrideTag("RenderType", transparent ? "Transparent" : cutout ? "TransparentCutout" : "Opaque");
        mat.renderQueue = transparent ? (int)RenderQueue.Transparent : cutout ? (int)RenderQueue.AlphaTest : -1;
    }

    
    static Material TransparentMat(string name, Color color, float smoothness, Color? emission = null, Texture2D baseMap = null,
        bool doubleSided = false, Vector2? perMeter = null)
    {
        Material mat = Mat(name, color, smoothness, emission, 0f, baseMap, perMeter, surface: Surface.Transparent, doubleSided: doubleSided);
        mat.SetShaderPassEnabled("ShadowCaster", false);
        mat.SetShaderPassEnabled("DepthOnly", false);
        mat.SetShaderPassEnabled("MotionVectors", false);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static Material SkyMat(string name, Color tint, Color ground, float thickness, float exposure, float sunSize)
    {
        string path = $"{MaterialRoot}/{name}.mat";
        EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
        var mat = Load<Material>(path);
        if (mat != null && !overwrite) return mat;
        bool isNew = mat == null;
        if (isNew) mat = new Material(Shader.Find("Skybox/Procedural"));
        mat.SetColor("_SkyTint", tint);
        mat.SetColor("_GroundColor", ground);
        mat.SetFloat("_AtmosphereThickness", thickness);
        mat.SetFloat("_Exposure", exposure);
        mat.SetFloat("_SunSize", sunSize);
        if (isNew) AssetDatabase.CreateAsset(mat, path);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    
    static void FixEmissionKeywords()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { MaterialRoot }))
        {
            var mat = Load<Material>(AssetDatabase.GUIDToAssetPath(guid));
            if (mat == null || !mat.HasProperty("_EmissionColor") || mat.GetColor("_EmissionColor").maxColorComponent <= 0f) continue;
            if (mat.IsKeywordEnabled("_EMISSION") && (mat.globalIlluminationFlags & MaterialGlobalIlluminationFlags.AnyEmissive) != 0) continue;
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            mat.EnableKeyword("_EMISSION");
            EditorUtility.SetDirty(mat);
        }
        AssetDatabase.SaveAssets();
    }

    

    enum PrefabKind
    {
        
        Prop,
        
        Clutter,
        
        Fixture,
        
        Decor,
    }

    
    static void SavePrefab(Func<GameObject> build, string path, PrefabKind kind, Action<GameObject> afterCombine = null)
    {
        if (!overwrite && Load<GameObject>(path) != null) return;
        EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
        
        build = ModelBuilderFor(path) ?? build;

        GameObject root = build();
        if (kind == PrefabKind.Prop)
        {
            Bounds b = RendererBounds(root);
            var box = root.AddComponent<BoxCollider>();
            box.center = b.center - root.transform.position;
            box.size = b.size;
        }
        if (kind != PrefabKind.Fixture)
        {
            string name = path.StartsWith(PrefabRoot + "/") ? path.Substring(PrefabRoot.Length + 1) : Path.GetFileName(path);
            string meshPath = $"{MeshDir}/{name.Replace('/', '_').Replace(".prefab", "")}.asset";
            CombineParts(root, meshPath, kind != PrefabKind.Clutter);
            afterCombine?.Invoke(root);
        }
        
        foreach (Renderer r in root.GetComponentsInChildren<Renderer>())
            if (r.transform != root.transform && r.bounds.size.magnitude < 0.3f) r.shadowCastingMode = ShadowCastingMode.Off;

        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    
    
    static void CombineParts(GameObject root, string meshPath, bool castShadows)
    {
        var groups = new Dictionary<Material, List<CombineInstance>>();
        var order = new List<Material>();
        Matrix4x4 toRoot = root.transform.worldToLocalMatrix;
        Transform live = root.transform.Find(LiveGroup);
        foreach (MeshFilter mf in root.GetComponentsInChildren<MeshFilter>())
        {
            if (live != null && mf.transform.IsChildOf(live)) continue;
            Material mat = mf.GetComponent<MeshRenderer>().sharedMaterial;
            if (!groups.TryGetValue(mat, out var list))
            {
                groups[mat] = list = new List<CombineInstance>();
                order.Add(mat);
            }
            list.Add(new CombineInstance { mesh = mf.sharedMesh, transform = toRoot * mf.transform.localToWorldMatrix });
        }

        var perMaterial = new List<CombineInstance>();
        var temp = new List<Mesh>();
        foreach (Material mat in order)
        {
            var m = new Mesh { indexFormat = IndexFormat.UInt32 };
            m.CombineMeshes(groups[mat].ToArray(), true, true);
            temp.Add(m);
            perMaterial.Add(new CombineInstance { mesh = m, transform = Matrix4x4.identity });
        }
        var combined = new Mesh { name = Path.GetFileNameWithoutExtension(meshPath), indexFormat = IndexFormat.UInt32 };
        combined.CombineMeshes(perMaterial.ToArray(), false, false);
        combined.RecalculateBounds();
        foreach (Mesh m in temp) Object.DestroyImmediate(m);

        for (int i = root.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = root.transform.GetChild(i);
            if (child != live) Object.DestroyImmediate(child.gameObject);
        }

        
        
        var existing = Load<Mesh>(meshPath);
        if (existing != null)
        {
            EditorUtility.CopySerialized(combined, existing);
            Object.DestroyImmediate(combined);
            combined = existing;
            EditorUtility.SetDirty(combined);
        }
        else AssetDatabase.CreateAsset(combined, meshPath);
        root.AddComponent<MeshFilter>().sharedMesh = combined;
        var renderer = root.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = order.ToArray();
        renderer.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
    }

    static Bounds RendererBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        Bounds b = renderers[0].bounds;
        foreach (Renderer r in renderers) b.Encapsulate(r.bounds);
        return b;
    }

    static GameObject Part(Transform parent, PrimitiveType type, Vector3 pos, Vector3 scale, Material mat, Vector3 euler = default)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localEulerAngles = euler;
        go.transform.localScale = scale;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    static GameObject Box(Transform p, Vector3 pos, Vector3 size, Material m, Vector3 euler = default) =>
        Part(p, PrimitiveType.Cube, pos, size, m, euler);

    
    static GameObject Cyl(Transform p, Vector3 pos, float diameter, float height, Material m, Vector3 euler = default) =>
        Part(p, PrimitiveType.Cylinder, pos, new Vector3(diameter, height / 2f, diameter), m, euler);

    static GameObject Ball(Transform p, Vector3 pos, Vector3 size, Material m) => Part(p, PrimitiveType.Sphere, pos, size, m);

    static Transform Root(string name) => new GameObject(name).transform;

    
    static Transform Live(Transform root)
    {
        Transform live = root.Find(LiveGroup);
        return live != null ? live : Group(root, LiveGroup, Vector3.zero);
    }

    static Light PointLight(Transform parent, string name, Vector3 pos, Color color, float intensity, float range) =>
        AddLight(parent, name, LightType.Point, pos, Vector3.zero, color, intensity, range);

    static Light SpotLight(Transform parent, string name, Vector3 pos, Vector3 euler, Color color, float intensity, float range, float angle, float innerAngle)
    {
        Light light = AddLight(parent, name, LightType.Spot, pos, euler, color, intensity, range, angle);
        light.innerSpotAngle = innerAngle;
        return light;
    }

    
    public static void SetFields(Object target, params (string name, object value)[] fields)
    {
        var so = new SerializedObject(target);
        foreach (var (name, value) in fields)
        {
            SerializedProperty p = so.FindProperty(name);
            if (p == null)
            {
                Debug.LogError($"{target.GetType().Name} has no field {name}");
                continue;
            }
            switch (value)
            {
                case Object o: p.objectReferenceValue = o; break;
                case float f: p.floatValue = f; break;
                case int i: p.intValue = i; break;
                case bool b: p.boolValue = b; break;
                case string str: p.stringValue = str; break;
                case Vector2 v2: p.vector2Value = v2; break;
                case Vector3 v3: p.vector3Value = v3; break;
                case Color c: p.colorValue = c; break;
                case Object[] arr:
                    p.arraySize = arr.Length;
                    for (int k = 0; k < arr.Length; k++) p.GetArrayElementAtIndex(k).objectReferenceValue = arr[k];
                    break;
                default: Debug.LogError($"SetFields: unsupported type for {name}"); break;
            }
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static Light AddLight(Transform parent, string name, LightType type, Vector3 pos, Vector3 euler, Color color, float intensity, float range, float angle = 120f)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localEulerAngles = euler;
        var light = go.AddComponent<Light>();
        light.type = type;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        if (type == LightType.Spot)
        {
            light.spotAngle = angle;
            light.innerSpotAngle = angle * 0.45f;
        }
        
        light.shadows = type == LightType.Spot ? LightShadows.Soft : LightShadows.None;
        return light;
    }

    static Transform Group(Transform parent, string name, Vector3 pos, float yaw = 0f)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent, false);
        t.localPosition = pos;
        t.localEulerAngles = new Vector3(0f, yaw, 0f);
        return t;
    }

    
    
    static GameObject Fixture(string name, Action<Transform> visual, Color lightColor, float intensity, float range, float lightY = -0.15f)
    {
        Transform r = Root(name);
        visual(r);

        var lightGo = new GameObject("Light");
        lightGo.transform.SetParent(r, false);
        lightGo.transform.localPosition = new Vector3(0f, lightY, 0f);
        lightGo.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Spot;
        light.color = lightColor;
        light.intensity = intensity;
        light.range = range;
        light.spotAngle = 150f;
        light.innerSpotAngle = 70f;
        light.shadows = LightShadows.Soft;

        
        r.gameObject.AddComponent<FixtureFlicker>().enabled = false;
        return r.gameObject;
    }

    
    static void CreateVolumeProfile()
    {
        var profile = Load<VolumeProfile>(VolumeProfilePath);
        if (profile != null && !overwrite) return;
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, VolumeProfilePath);
        }

        var bloom = GetOrAdd<Bloom>(profile);
        bloom.intensity.Override(1.2f);
        bloom.threshold.Override(0.95f);
        bloom.scatter.Override(0.75f);

        var vignette = GetOrAdd<Vignette>(profile);
        vignette.intensity.Override(0.4f);
        vignette.smoothness.Override(0.5f);

        var grain = GetOrAdd<FilmGrain>(profile);
        grain.type.Override(FilmGrainLookup.Medium3);
        grain.intensity.Override(0.35f);

        var color = GetOrAdd<ColorAdjustments>(profile);
        color.contrast.Override(12f);
        color.saturation.Override(-15f);
        color.colorFilter.Override(new Color(0.92f, 1f, 0.97f));

        var tonemap = GetOrAdd<Tonemapping>(profile);
        tonemap.mode.Override(TonemappingMode.Neutral);

        var aberration = GetOrAdd<ChromaticAberration>(profile);
        aberration.intensity.Override(0.15f);

        EditorUtility.SetDirty(profile);
    }

    static T GetOrAdd<T>(VolumeProfile profile) where T : VolumeComponent
    {
        if (profile.TryGet(out T existing)) return existing;
        T component = profile.Add<T>(true);
        component.name = typeof(T).Name;
        AssetDatabase.AddObjectToAsset(component, profile);
        return component;
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
