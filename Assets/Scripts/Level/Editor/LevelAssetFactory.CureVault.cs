using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;




public static partial class LevelAssetFactory
{
    const string BossPath = "Assets/Prefabs/BossZombie.prefab";
    
    const float LabWallHeight = 3.2f;

    static Material warningRed, cureGlow, bossSkin, pustule, eyeRed;

    static void VaultMaterials()
    {
        warningRed = Mat("Laboratory/WarningRed", new Color(1f, 0.15f, 0.1f), 0.5f, new Color(1f, 0.08f, 0.04f) * 3f);
        cureGlow = Mat("Laboratory/CureGlow", new Color(0.3f, 1f, 0.5f), 0.6f, new Color(0.2f, 1f, 0.4f) * 2.5f);
        bossSkin = Mat("Laboratory/PatientZeroSkin", new Color(0.4f, 0.47f, 0.37f), 0.55f);
        pustule = Mat("Laboratory/Pustule", new Color(0.45f, 1f, 0.3f), 0.8f, new Color(0.35f, 1f, 0.2f) * 1.6f);
        eyeRed = Mat("Laboratory/EyeRed", new Color(1f, 0.1f, 0.05f), 0.8f, new Color(1f, 0.05f, 0.02f) * 4f);
    }

    
    
    static GameObject CureVault()
    {
        if (warningRed == null) VaultMaterials();
        Transform r = Root("CureVault");
        
        for (int i = 0; i < 24; i++)
        {
            Vector3 p = Quaternion.Euler(0f, i * 15f, 0f) * Vector3.forward * 1.6f;
            Box(r, p + Vector3.up * 0.004f, new Vector3(0.38f, 0.008f, 0.16f), i % 2 == 0 ? hazardYellow : darkPlastic, new Vector3(0f, i * 15f, 0f));
        }
        Cyl(r, new Vector3(0f, 0.5f, 0f), 1f, 1f, steel);
        Cyl(r, new Vector3(0f, 0.9f, 0f), 1.04f, 0.04f, cureGlow);
        Cyl(r, new Vector3(0f, 1.03f, 0f), 1.12f, 0.06f, darkPlastic);
        Box(r, new Vector3(0f, 0.55f, 0.5f), new Vector3(0.4f, 0.2f, 0.03f), darkPlastic);
        Box(r, new Vector3(0f, 0.55f, 0.515f), new Vector3(0.34f, 0.14f, 0.005f), cureGlow);
        
        Cyl(r, new Vector3(0f, 2.45f + (LabWallHeight - 2.45f) / 2f, 0f), 1.15f, LabWallHeight - 2.45f, steel);
        Cyl(r, new Vector3(0f, 2.6f, 0f), 1.17f, 0.12f, hazardYellow);

        Transform live = Live(r);
        Transform briefcase = Group(live, "Briefcase", new Vector3(0f, 1.06f, 0f));
        if (!CureBriefcaseModel(briefcase))
        {
            Box(briefcase, new Vector3(0f, 0.19f, 0f), new Vector3(0.5f, 0.36f, 0.12f), briefcaseSilver);
            Box(briefcase, new Vector3(0f, 0.19f, 0f), new Vector3(0.505f, 0.03f, 0.125f), darkPlastic);
            Box(briefcase, new Vector3(0f, 0.4f, 0f), new Vector3(0.16f, 0.03f, 0.03f), darkPlastic);
            foreach (float x in new[] { -0.07f, 0.07f }) Box(briefcase, new Vector3(x, 0.385f, 0f), new Vector3(0.025f, 0.04f, 0.025f), darkPlastic);
            foreach (float x in new[] { -0.17f, 0.17f }) Box(briefcase, new Vector3(x, 0.3f, 0.065f), new Vector3(0.05f, 0.03f, 0.01f), metal);
            Box(briefcase, new Vector3(0f, 0.12f, 0.062f), new Vector3(0.12f, 0.12f, 0.005f), cureGlow, new Vector3(0f, 0f, 45f));
        }

        Transform glassCase = Group(live, "Glass Case", new Vector3(0f, 1.06f, 0f));
        Cyl(glassCase, new Vector3(0f, 0.38f, 0f), 0.9f, 0.76f, tankGlass);
        Cyl(glassCase, new Vector3(0f, 0.78f, 0f), 0.96f, 0.05f, steel);
        Cyl(glassCase, new Vector3(0f, 0.02f, 0f), 0.96f, 0.04f, steel);

        
        BuildVault(r);
        
        Transform ladder = BuildEscapeLadder(live);

        var alarms = new List<Object>();
        foreach (int side in new[] { -1, 1 })
        {
            Vector3 post = new Vector3(side * 1.25f, 0f, -0.7f);
            Cyl(r, post + Vector3.up * 0.8f, 0.1f, 1.6f, steel);
            Transform beacon = Group(live, "Beacon", post + Vector3.up * 1.7f);
            Cyl(beacon, Vector3.zero, 0.22f, 0.2f, warningRed);
            Box(beacon, new Vector3(0f, 0f, 0.08f), new Vector3(0.12f, 0.12f, 0.06f), metal);
            alarms.Add(SpotLight(beacon, "Alarm", new Vector3(0f, 0f, 0.12f), new Vector3(8f, 0f, 0f), new Color(1f, 0.1f, 0.05f), 7f, 10f, 45f, 20f));
            var spin = beacon.gameObject.AddComponent<SpinningBeacon>();
            SetFields(spin, ("degreesPerSecond", side * 200f));
        }
        alarms.Add(PointLight(live, "Vault Glow", new Vector3(0f, 2.1f, 0f), new Color(1f, 0.15f, 0.1f), 1.8f, 6f));

        var pedestal = r.gameObject.AddComponent<BoxCollider>();
        pedestal.center = new Vector3(0f, 0.75f, 0f);
        pedestal.size = new Vector3(1.1f, 1.5f, 1.1f);
        
        var modifier = r.gameObject.AddComponent<Unity.AI.Navigation.NavMeshModifier>();
        modifier.overrideArea = true;
        modifier.area = 1;

        var cure = r.gameObject.AddComponent<CureBriefcase>();
        SetFields(cure, ("bossPrefab", Load<GameObject>(BossPath)), ("glassCase", glassCase), ("briefcase", briefcase),
            ("escapeLadder", ladder), ("explosionVideo", Load<UnityEngine.Video.VideoClip>(ExplosionVideoPath)),
            ("alarmLights", alarms.ToArray()), ("caseLift", 1.3f), ("bossOffset", new Vector3(0f, 0f, 5f)));
        return r.gameObject;
    }

    
    static void BuildVault(Transform r)
    {
        const float z = 1.35f;
        Box(r, new Vector3(-1.25f, 1.35f, z), new Vector3(0.3f, 2.7f, 0.55f), steel);
        Box(r, new Vector3(1.25f, 1.35f, z), new Vector3(0.3f, 2.7f, 0.55f), steel);
        Box(r, new Vector3(0f, 2.75f, z), new Vector3(2.9f, 0.4f, 0.55f), steel);
        Box(r, new Vector3(0f, 0.06f, z), new Vector3(2.6f, 0.12f, 0.55f), hazardYellow);
    }

    
    
    
    static Transform BuildEscapeLadder(Transform live)
    {
        Transform ladder = Group(live, "Escape Ladder", new Vector3(0f, 0f, -1.75f));
        
        Box(ladder, new Vector3(0f, 1.6f, -0.22f), new Vector3(1.0f, 3.2f, 0.12f), metal);

        var ladderPrefab = Load<GameObject>(LadderPrefabPath);
        if (ladderPrefab != null)
        {
            var model = (GameObject)PrefabUtility.InstantiatePrefab(ladderPrefab, ladder);
            model.name = "Ladder";
            model.transform.localPosition = new Vector3(0f, 0f, -0.05f);
            
            model.transform.localScale = Vector3.one * (LabWallHeight - 0.02f) / 3.26f;
        }

        
        foreach (var o in new[] { new Vector3(0f, 0f, 0.55f), new Vector3(0f, 0f, -0.55f), new Vector3(0.55f, 0f, 0f), new Vector3(-0.55f, 0f, 0f) })
            Box(ladder, new Vector3(o.x, LabWallHeight - 0.08f, o.z), new Vector3(o.x == 0f ? 1.2f : 0.2f, 0.16f, o.z == 0f ? 1.2f : 0.2f), steel);
        Material daylight = Mat("Laboratory/HatchDaylight", new Color(0.8f, 0.85f, 0.9f), 0.2f, new Color(0.8f, 0.9f, 1f) * 1.6f);
        Box(ladder, new Vector3(0f, LabWallHeight - 0.03f, 0f), new Vector3(0.98f, 0.02f, 0.98f), daylight);
        PointLight(ladder, "Surface Light", new Vector3(0f, LabWallHeight - 0.3f, 0.15f), new Color(0.7f, 0.8f, 0.9f), 2.6f, 5f);
        PointLight(ladder, "Guide", new Vector3(0f, 1.6f, 0.5f), new Color(0.3f, 1f, 0.5f), 1.3f, 4f);
        return ladder;
    }

    
    
    static GameObject BossZombie()
    {
        Transform r = Root("BossZombie");
        SetLayerRecursive(r.gameObject, 9);

        var collider = r.gameObject.AddComponent<CapsuleCollider>();
        collider.center = new Vector3(0f, 1.4f, 0f);
        collider.radius = 0.75f;
        collider.height = 2.8f;

        var agent = r.gameObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
        agent.radius = 0.6f;
        agent.height = 2.8f;
        agent.baseOffset = 0f;
        agent.speed = 2f;
        agent.angularSpeed = 300f;
        agent.acceleration = 12f;

        var health = r.gameObject.AddComponent<EnemyHealth>();
        SetFields(health, ("maxHealth", 1000f));
        SetFields(r.gameObject.AddComponent<TokenReward>(), ("tokens", 15));
        r.gameObject.AddComponent<global::BossZombie>();
        ApplyBossVisual(r.gameObject);
        return r.gameObject;
    }

    
    
    public static void ApplyBossVisual(GameObject root)
    {
        for (int i = root.transform.childCount - 1; i >= 0; i--) Object.DestroyImmediate(root.transform.GetChild(i).gameObject);

        var model = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(ZombieModelPath(3)), root.transform);
        model.name = "Model";
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        
        model.transform.localScale = Vector3.one * (2.8f / 1.82f);
        SetLayerRecursive(model, root.layer);

        Light eyes = PointLight(root.transform, "Eye Glow", new Vector3(0f, 2.6f, 0.45f), new Color(1f, 0.15f, 0.08f), 1.6f, 4f);

        var boss = root.GetComponent<global::BossZombie>();
        var so = new SerializedObject(boss);
        so.FindProperty("body").objectReferenceValue = null;
        so.FindProperty("animator").objectReferenceValue = model.GetComponent<Animator>();
        so.FindProperty("eyeLight").objectReferenceValue = eyes;
        SerializedProperty skin = so.FindProperty("skin");
        Renderer[] renderers = model.GetComponentsInChildren<SkinnedMeshRenderer>();
        skin.arraySize = renderers.Length;
        for (int i = 0; i < renderers.Length; i++) skin.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetLayerRecursive(GameObject go, int layer)
    {
        foreach (Transform t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = layer;
    }
}
