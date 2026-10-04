using System.Linq;
using UnityEditor;
using UnityEngine;





public static partial class LevelAssetFactory
{
    public const string IntroDir = PrefabRoot + "/Intro";
    public const string HelicopterPath = IntroDir + "/Helicopter.prefab";
    public const string RoofHatchPath = IntroDir + "/RoofHatch.prefab";
    public const int CrowdZombieVariants = 3;
    public static string CrowdZombiePath(int variant) => $"{IntroDir}/CrowdZombie{variant}.prefab";

    const string ModelDir = IntroModels.Dir;
    
    const float HatchModelScale = 14f;

    static Material roofConcrete;

    static void BuildIntro()
    {
        IntroModels.Configure();

        roofConcrete = Mat("Intro/RoofConcrete", new Color(0.52f, 0.52f, 0.5f), 0.2f, baseMap: concreteTex, perMeter: Vector2.one / 2f);

        SavePrefab(Helicopter, HelicopterPath, PrefabKind.Fixture);
        SavePrefab(Hatch, RoofHatchPath, PrefabKind.Fixture);
        for (int i = 1; i <= CrowdZombieVariants; i++)
        {
            int variant = i;
            SavePrefab(() => CrowdZombie(variant), CrowdZombiePath(variant), PrefabKind.Fixture);
        }
    }

    
    
    public static Material CityMaterial(string source)
    {
        Color grey(float v) => new Color(v, v, v);
        switch (source)
        {
            case "Asphalt_A01_100cm": return Mat("Intro/City_Asphalt", new Color(0.1f, 0.1f, 0.11f), 0.25f);
            case "Concrete_Flooring_H01_2m_1":
            case "Concrete_Flooring_I01_2m": return Mat("Intro/City_Pavement", new Color(0.42f, 0.41f, 0.39f), 0.12f);
            case "Concrete_Flooring_H01_2m": return Mat("Intro/City_Concrete", new Color(0.5f, 0.47f, 0.43f), 0.1f);
            case "facade_5":
            case "facade_6":
            case "NEWFACADE":
            case "NEWFACADE_1": return Mat("Intro/City_Windows", new Color(0.5f, 0.56f, 0.62f), 0.35f);
            case "Glass_Coated_Black_2":
            case "Glass_Window_Neutral": return Mat("Intro/City_Glass", new Color(0.12f, 0.16f, 0.2f), 0.7f, metallic: 0.1f);
            case "Backing": return Mat("Intro/City_Panes", new Color(0.2f, 0.28f, 0.38f), 0.3f);
            case "Ground": return Mat("Intro/City_Ground", new Color(0.2f, 0.2f, 0.2f), 0.05f);
            case "Aluminum_Anodized_DarkGray_1": return Mat("Intro/City_Aluminium", grey(0.2f), 0.5f, metallic: 0.8f);
            case "Metal_Painted_Bumpy_Yellow_5cm":
            case "Metal_Painted_Bumpy_Yellow_5cm_1": return Mat("Intro/City_PaintYellow", new Color(0.62f, 0.5f, 0.08f), 0.3f);
            case "Metal_Painted_Bumpy_Red_5cm":
            case "Metal_Painted_Bumpy_Red_5cm_1": return Mat("Intro/City_PaintRed", new Color(0.42f, 0.07f, 0.05f), 0.3f);
            case "Grass_C_200cm": return Mat("Intro/City_Grass", new Color(0.22f, 0.27f, 0.12f), 0.05f);
            case "Soil_B01_200cm": return Mat("Intro/City_Soil", new Color(0.25f, 0.2f, 0.14f), 0.05f);
            case "AM113_072_Washingtonia_filifera":
            case "AM113_072_Washingtonia_filifera_1": return Mat("Intro/City_Palm", new Color(0.2f, 0.3f, 0.1f), 0.1f, doubleSided: true);
            case "_Charcoal_2": return Mat("Intro/City_Charcoal", grey(0.11f), 0.3f);
            default: return Mat("Intro/City_Other", grey(0.4f), 0.2f);
        }
    }

    
    
    static Material TexturedMat(string name, string albedo, string normal, string metallic, float smoothness)
    {
        Material mat = Mat(name, Color.white, smoothness, baseMap: Load<Texture2D>(albedo));
        mat.SetFloat("_Smoothness", smoothness);
        if (!string.IsNullOrEmpty(normal))
        {
            mat.SetTexture("_BumpMap", Load<Texture2D>(normal));
            mat.EnableKeyword("_NORMALMAP");
        }
        if (!string.IsNullOrEmpty(metallic))
        {
            mat.SetTexture("_MetallicGlossMap", Load<Texture2D>(metallic));
            mat.EnableKeyword("_METALLICSPECGLOSSMAP");
            mat.SetFloat("_Metallic", 1f);
        }
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static GameObject Model(string path, string name)
    {
        var asset = Load<GameObject>(path);
        if (asset == null)
        {
            Debug.LogError("Missing intro model " + path);
            return new GameObject(name);
        }
        GameObject go = Object.Instantiate(asset);
        go.name = name;
        return go;
    }

    

    
    
    
    static GameObject Helicopter()
    {
        Transform root = Root("Helicopter");
        Transform mount = Group(Live(root), "Camera Mount", new Vector3(0f, 1.2f, 3f));
        var rig = root.gameObject.AddComponent<HelicopterRig>();
        SetFields(rig, ("doorMount", mount));
        return root.gameObject;
    }

    

    
    
    
    static GameObject Hatch()
    {
        Transform r = Root("RoofHatch");
        const float half = 1.0f;     
        const float curb = 0.3f;
        const float curbTop = 0.16f;

        
        foreach (int side in new[] { -1, 1 })
        {
            Box(r, new Vector3(0f, curbTop / 2f, side * (half + curb / 2f)), new Vector3(half * 2f + curb * 2f, curbTop, curb), roofConcrete);
            Box(r, new Vector3(side * (half + curb / 2f), curbTop / 2f, 0f), new Vector3(curb, curbTop, half * 2f), roofConcrete);
        }
        
        Box(r, new Vector3(0f, 0.03f, 0f), new Vector3(half * 2f, 0.06f, half * 2f), Mat("Intro/Hatch_Pit", new Color(0.01f, 0.01f, 0.01f), 0f));

        
        Transform hinge = Group(r, "Hinge", new Vector3(0f, curbTop + 0.05f, -(half + curb)));
        Transform lid = Group(hinge, "Lid", new Vector3(0f, 0f, half + curb));

        
        Material steel = Mat("Intro/Hatch_Steel", new Color(0.24f, 0.27f, 0.25f), 0.4f, metallic: 0.7f);
        Material outer = Mat("Intro/Hatch_Outer", new Color(0.3f, 0.33f, 0.3f), 0.3f, metallic: 0.5f);
        Material parts = Mat("Intro/Hatch_Parts", new Color(0.14f, 0.14f, 0.15f), 0.45f, metallic: 0.85f);
        Material brass = Mat("Intro/Hatch_Brass", new Color(0.5f, 0.38f, 0.16f), 0.5f, metallic: 0.85f);
        GameObject model = Model(ModelDir + "/Hatch/Hatch.fbx", "Hatch Model");
        model.transform.SetParent(lid, false);
        model.transform.localScale = Vector3.one * HatchModelScale;
        
        model.transform.localPosition = new Vector3(0f, 0.0185f * HatchModelScale, -0.0104f * HatchModelScale);
        foreach (Renderer rend in model.GetComponentsInChildren<Renderer>())
        {
            string material = rend.sharedMaterial != null ? rend.sharedMaterial.name : "";
            rend.sharedMaterial = material switch
            {
                "outer_hatch" => outer,
                "hatch_parts_1" => parts,
                "hatch_secondary" => brass,
                _ => steel,
            };
        }

        
        Transform live = Live(r);
        Light glow = PointLight(live, "Shaft Light", new Vector3(0f, 0.35f, 0f), new Color(1f, 0.68f, 0.3f), 4f, 7f);
        Transform bottom = Group(live, "Descend To", new Vector3(0f, -6.4f, 0f));

        
        var triggerGo = new GameObject("Enter Trigger");
        triggerGo.transform.SetParent(r, false);
        var trigger = triggerGo.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 0.5f, 0f);
        
        trigger.size = new Vector3(half * 2f + curb * 2f, 1.8f, half * 2f + curb * 2f);
        
        var body = r.gameObject.AddComponent<Rigidbody>();
        body.isKinematic = true;

        var hatch = r.gameObject.AddComponent<RoofHatch>();
        SetFields(hatch, ("hinge", hinge), ("descendTo", bottom), ("shaftLight", glow));
        return r.gameObject;
    }

    

    
    
    static GameObject CrowdZombie(int variant)
    {
        GameObject zombie = ZombieVisual(variant, ZombieModels.CrowdControllerPath, $"CrowdZombie{variant}", false);
        zombie.GetComponent<Animator>().cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        return zombie;
    }
}
