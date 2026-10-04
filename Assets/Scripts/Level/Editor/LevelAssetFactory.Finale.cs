using UnityEditor;
using UnityEngine;
using UnityEngine.Video;





public static partial class LevelAssetFactory
{
    public const string FinaleModelDir = "Assets/Models/Finale";
    public const string FinalePrefabDir = "Assets/Resources/Finale";
    public const string C4PrefabPath = FinalePrefabDir + "/C4Viewmodel.prefab";
    public const string LadderPrefabPath = FinalePrefabDir + "/Ladder.prefab";
    public const string HatchLidPrefabPath = FinalePrefabDir + "/HatchLid.prefab";
    public const string ExplosionVideoPath = "Assets/Video/CityExplosion.mp4";

    
    public const float C4Scale = 0.14f;

    static void BuildFinale()
    {
        ConfigureFinaleImports();
        SavePrefab(C4Viewmodel, C4PrefabPath, PrefabKind.Fixture);
        SavePrefab(LadderModel, LadderPrefabPath, PrefabKind.Fixture);
        SavePrefab(HatchLid, HatchLidPrefabPath, PrefabKind.Fixture);
    }

    
    
    [MenuItem("Losing Power/Rebuild Finale Assets")]
    public static void RebuildFinale()
    {
        EnsureAssets();
        overwrite = true;
        try
        {
            ConfigureFinaleImports();
            SavePrefab(C4Viewmodel, C4PrefabPath, PrefabKind.Fixture);
            SavePrefab(LadderModel, LadderPrefabPath, PrefabKind.Fixture);
            SavePrefab(HatchLid, HatchLidPrefabPath, PrefabKind.Fixture);
            SavePrefab(CureVault, $"{LabDir}/CureVault.prefab", PrefabKind.Decor);
        }
        finally
        {
            overwrite = false;
        }
        AssetDatabase.SaveAssets();
    }

    static void ConfigureFinaleImports()
    {
        foreach (string path in new[] { FinaleModelDir + "/C4.fbx", FinaleModelDir + "/Ladder.fbx" })
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null || (!importer.importCameras && !importer.importLights)) continue;
            
            importer.importCameras = false;
            importer.importLights = false;
            importer.SaveAndReimport();
        }
    }

    
    
    static GameObject C4Viewmodel()
    {
        Material screen = Mat("Finale/C4Screen", new Color(0.02f, 0.035f, 0.02f), 0.6f);
        Material ledRed = Mat("Finale/C4LedRed", new Color(0.3f, 0.02f, 0.02f), 0.4f, new Color(0.9f, 0.05f, 0.03f) * 1.5f);
        Material ledGreen = Mat("Finale/C4LedGreen", new Color(0.05f, 0.3f, 0.05f), 0.4f, new Color(0.15f, 0.9f, 0.2f) * 0.6f);

        Transform root = Root("C4Viewmodel");
        GameObject model = Model(FinaleModelDir + "/C4.fbx", "Model");
        model.transform.SetParent(root, false);
        model.transform.localScale = Vector3.one * C4Scale;

        Renderer red = null;
        var greens = new System.Collections.Generic.List<Object>();
        foreach (Renderer r in model.GetComponentsInChildren<Renderer>())
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Material[] mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                string name = mats[i] != null ? mats[i].name : "";
                if (name.StartsWith("Procedural")) mats[i] = screen;
                else if (name.EndsWith(".014")) { mats[i] = ledRed; red = r; }
                else if (name.EndsWith(".013")) { mats[i] = ledGreen; greens.Add(r); }
            }
            r.sharedMaterials = mats;
        }

        
        var display = new GameObject("Display");
        display.transform.SetParent(model.transform, false);
        display.transform.localPosition = new Vector3(-0.074f, 0.306f, -0.324f);
        display.transform.localEulerAngles = new Vector3(90f, 90f, 180f);
        var text = display.AddComponent<TextMesh>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 64;
        text.characterSize = 0.017f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = new Color(1f, 0.12f, 0.08f);
        text.text = "";
        display.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;

        var device = root.gameObject.AddComponent<C4Device>();
        SetFields(device, ("display", text), ("redLed", red), ("greenLeds", greens.ToArray()), ("model", model.transform));
        return root.gameObject;
    }

    
    static GameObject LadderModel()
    {
        Material metal = Mat("Finale/Ladder", new Color(0.46f, 0.48f, 0.5f), 0.45f, metallic: 0.7f);
        GameObject ladder = Model(FinaleModelDir + "/Ladder.fbx", "Ladder");
        foreach (Renderer r in ladder.GetComponentsInChildren<Renderer>()) r.sharedMaterial = metal;
        return ladder;
    }

    
    
    static GameObject HatchLid()
    {
        Material steel = Mat("Intro/Hatch_Steel", new Color(0.24f, 0.27f, 0.25f), 0.4f, metallic: 0.7f);
        Material outer = Mat("Intro/Hatch_Outer", new Color(0.3f, 0.33f, 0.3f), 0.3f, metallic: 0.5f);
        Material parts = Mat("Intro/Hatch_Parts", new Color(0.14f, 0.14f, 0.15f), 0.45f, metallic: 0.85f);
        Material brass = Mat("Intro/Hatch_Brass", new Color(0.5f, 0.38f, 0.16f), 0.5f, metallic: 0.85f);

        Transform root = Root("HatchLid");
        Transform lid = Group(root, "Lid", new Vector3(0f, 0f, 1.3f));
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
        return root.gameObject;
    }
}
