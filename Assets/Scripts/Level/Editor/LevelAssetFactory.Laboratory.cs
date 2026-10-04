using System.Collections.Generic;
using UnityEditor;
using UnityEngine;






public static partial class LevelAssetFactory
{
    const string LabDir = PrefabRoot + "/Laboratory";

    static Material tankGlass, tankFluid, labSteel, blood, hazardYellow, briefcaseSilver, alarmRed;

    static void BuildLaboratory()
    {
        Material walls = Mat("Laboratory/LabWall", new Color(0.7f, 0.76f, 0.78f), 0.6f, baseMap: hospitalTileTex, perMeter: Vector2.one / 1.2f);
        Material floor = Mat("Laboratory/EpoxyFloor", new Color(0.55f, 0.58f, 0.6f), 0.75f, baseMap: concreteTex, perMeter: Vector2.one / 2f);
        Material ceilingMat = Mat("Laboratory/CeilingTiles", new Color(0.7f, 0.74f, 0.76f), 0.05f, baseMap: ceilingTilesTex, perMeter: Vector2.one / 1.2f);
        tankGlass = Mat("Laboratory/TankGlass", new Color(0.6f, 0.85f, 0.8f, 0.18f), 0.95f, surface: Surface.Transparent, doubleSided: true);
        tankFluid = Mat("Laboratory/TankFluid", new Color(0.25f, 0.9f, 0.45f, 0.42f), 0.9f, new Color(0.03f, 0.36f, 0.13f), surface: Surface.Transparent);
        labSteel = Mat("Laboratory/Steel", new Color(0.6f, 0.62f, 0.64f), 0.75f, metallic: 0.8f);
        blood = Mat("Laboratory/Blood", new Color(0.22f, 0.015f, 0.015f), 0.85f);
        hazardYellow = Mat("Laboratory/Hazard", new Color(0.85f, 0.65f, 0.05f), 0.4f);
        briefcaseSilver = Mat("Laboratory/Briefcase", new Color(0.75f, 0.77f, 0.8f), 0.85f, metallic: 0.9f);
        alarmRed = Mat("Laboratory/AlarmScreen", new Color(0.1f, 0.01f, 0.01f), 0.8f, new Color(1f, 0.05f, 0.03f) * 1.8f);

        SavePrefab(SpecimenTank, SpecimenTankPath, PrefabKind.Prop, AddTankGlow);
        SavePrefab(() => BrokenTank(false), $"{LabDir}/BrokenTank.prefab", PrefabKind.Prop);
        SavePrefab(() => BrokenTank(true), $"{LabDir}/ShatteredTank.prefab", PrefabKind.Prop);
        SavePrefab(LabConsole, $"{LabDir}/LabConsole.prefab", PrefabKind.Prop);
        SavePrefab(GlassShards, $"{LabDir}/GlassShards.prefab", PrefabKind.Clutter);
        SavePrefab(BloodPool, $"{LabDir}/BloodPool.prefab", PrefabKind.Clutter);
        SavePrefab(FluidSpill, $"{LabDir}/FluidSpill.prefab", PrefabKind.Clutter);
        SavePrefab(DragMarks, $"{LabDir}/DragMarks.prefab", PrefabKind.Clutter);
        
        SavePrefab(BossZombie, BossPath, PrefabKind.Fixture);
        SavePrefab(CureVault, $"{LabDir}/CureVault.prefab", PrefabKind.Decor);

        SaveTheme("Laboratory", t =>
        {
            t.displayName = "Research Lab (B5)";
            t.wallMaterial = walls;
            t.floorMaterial = floor;
            t.ceilingMaterial = ceilingMat;
            t.wallHeight = LabWallHeight;
            
            t.roomCount = 4;
            t.roomSizeRange = new Vector2Int(3, 3);
            t.startRoomSize = Vector2Int.zero;
            t.featureRooms = new List<RoomFurnishing>();
            t.wallProps = new List<PropEntry>
            {
                Entry($"{LabDir}/SpecimenTank.prefab", 3f), Entry($"{LabDir}/BrokenTank.prefab", 2f),
                Entry($"{LabDir}/ShatteredTank.prefab", 1f), Entry($"{OfficeProps}/LabBench.prefab", 1.5f, 0.4f),
                Entry($"{LabDir}/LabConsole.prefab", 1f, 0.15f), Entry($"{OfficeProps}/FilingCabinet.prefab", 0.5f, 0.4f),
                Entry($"{OfficeProps}/OfficeChair.prefab", 0.5f, 0.7f),
            };
            
            t.roomWallProps = new List<PropEntry>
            {
                Entry($"{LabDir}/SpecimenTank.prefab", 3f), Entry($"{LabDir}/BrokenTank.prefab", 3f), Entry($"{LabDir}/ShatteredTank.prefab", 2f),
            };
            t.clutter = new List<PropEntry>
            {
                Entry($"{LabDir}/GlassShards.prefab", 3f), Entry($"{LabDir}/BloodPool.prefab", 1.5f), Entry($"{LabDir}/DragMarks.prefab", 1f),
                Entry($"{LabDir}/FluidSpill.prefab", 2f),
                Entry($"{OfficeClutter}/PaperSheet.prefab", 2f), Entry($"{HospitalDir}/Syringe.prefab", 1f), Entry($"{HospitalDir}/Clipboard.prefab", 1f),
            };
            t.wallPropChance = 0.7f;
            t.maxWallPropsPerCell = 2;
            t.maxClutterPerCell = 4;
            t.lightFixture = Load<GameObject>($"{HospitalDir}/ColdPanelLight.prefab");
            t.deadFixtureMaterial = fluorescentDead;
            t.lightChance = 0.2f;
            t.flickerChance = 0.5f;
            t.deadChance = 0.3f;
            t.ambientColor = new Color(0.014f, 0.026f, 0.026f);
            t.fogColor = new Color(0.008f, 0.02f, 0.02f);
            t.fogDensity = 0.06f;
            
            t.goalPrefab = Load<GameObject>($"{LabDir}/CureVault.prefab");
            t.bossPrefab = null;
            
            t.goalRoomSize = new Vector2Int(8, 8);
        });
    }

    const string SpecimenTankPath = LabDir + "/SpecimenTank.prefab";

    static void AddTankGlow(GameObject r) =>
        AddLight(r.transform, "Tank Glow", LightType.Point, new Vector3(0f, 1.4f, 0.4f), Vector3.zero, new Color(0.3f, 1f, 0.5f), 0.8f, 2.6f);

    
    
    static void Specimen(Transform r, float y, float tilt)
    {
        var prefab = Load<GameObject>(ZombieModelPath(1));
        if (prefab != null)
        {
            var zombie = (GameObject)PrefabUtility.InstantiatePrefab(prefab, Live(r));
            zombie.name = "Specimen";
            zombie.transform.localScale = Vector3.one * 0.78f;
            zombie.transform.localPosition = new Vector3(0f, y - 0.7f, 0f);
            zombie.transform.localEulerAngles = new Vector3(tilt, 20f, 4f);
            var animator = zombie.GetComponent<Animator>();
            animator.runtimeAnimatorController = Load<RuntimeAnimatorController>(ZombieModels.GameControllerPath);
            animator.speed = 0.3f;
            animator.cullingMode = AnimatorCullingMode.CullCompletely;
        }
        Cyl(r, new Vector3(0f, y + 0.75f, 0f), 0.02f, 0.5f, darkPlastic);
    }

    static void TankBase(Transform r)
    {
        Cyl(r, new Vector3(0f, 0.2f, 0f), 1.3f, 0.4f, labSteel);
        Cyl(r, new Vector3(0f, 0.42f, 0f), 1.18f, 0.04f, darkPlastic);
        for (int i = 0; i < 6; i++)
        {
            float a = i * 60f * Mathf.Deg2Rad;
            Box(r, new Vector3(Mathf.Cos(a) * 0.62f, 0.2f, Mathf.Sin(a) * 0.62f), new Vector3(0.05f, 0.08f, 0.05f), hazardYellow);
        }
    }

    static void TankTop(Transform r, float tilt)
    {
        Transform top = Group(r, "Top", new Vector3(0f, 2.55f, 0f));
        top.localEulerAngles = new Vector3(tilt, 0f, 0f);
        Cyl(top, Vector3.zero, 1.25f, 0.25f, labSteel);
        foreach (float x in new[] { -0.3f, 0f, 0.3f }) Cyl(top, new Vector3(x, 0.35f, 0f), 0.06f, 0.5f, darkPlastic);
    }

    
    static GameObject SpecimenTank()
    {
        Transform r = Root("SpecimenTank");
        TankBase(r);
        Cyl(r, new Vector3(0f, 1.44f, 0f), 1.06f, 2.0f, tankFluid);
        Specimen(r, 1.45f, 6f);
        for (int i = 0; i < 6; i++)
            Ball(r, new Vector3((Hash(i, 4) - 0.5f) * 0.6f, 0.7f + i * 0.28f, (Hash(i, 6) - 0.5f) * 0.6f), Vector3.one * (0.04f + Hash(i, 2) * 0.04f), tankGlass);
        Cyl(r, new Vector3(0f, 1.44f, 0f), 1.12f, 2.04f, tankGlass);
        TankTop(r, 0f);
        return r.gameObject;
    }

    
    
    static GameObject BrokenTank(bool shattered)
    {
        Transform r = Root(shattered ? "ShatteredTank" : "BrokenTank");
        TankBase(r);
        const int staves = 16;
        for (int i = 0; i < staves; i++)
        {
            float angle = i * 360f / staves;
            
            float facing = Mathf.Abs(Mathf.DeltaAngle(angle, 90f));
            float height = 2.0f;
            if (shattered) height = 0.2f + Hash(i, 31) * 0.6f;
            else if (facing < 50f) height = facing < 30f ? 0f : 0.3f + Hash(i, 3) * 0.5f;
            if (height <= 0.01f) continue;
            float a = angle * Mathf.Deg2Rad;
            Box(r, new Vector3(Mathf.Cos(a) * 0.55f, 0.44f + height / 2f, Mathf.Sin(a) * 0.55f), new Vector3(0.22f, height, 0.012f), tankGlass,
                new Vector3(0f, 90f - angle, 0f));
        }
        if (!shattered) TankTop(r, 0f);
        else
        {
            
            Transform top = Group(r, "Top", new Vector3(0.75f, 0.35f, 0.4f));
            top.localEulerAngles = new Vector3(70f, 30f, 0f);
            Cyl(top, Vector3.zero, 1.25f, 0.25f, labSteel);
        }
        
        Cyl(r, new Vector3(0f, 0.445f, 0f), 1.05f, 0.006f, tankFluid);
        
        Cyl(r, new Vector3(0.25f, 2.0f, 0.2f), 0.05f, 1.2f, darkPlastic, new Vector3(25f, 0f, 10f));
        return r.gameObject;
    }

    
    static GameObject LabConsole()
    {
        Transform r = Root("LabConsole");
        Box(r, new Vector3(0f, 0.45f, 0f), new Vector3(1.8f, 0.9f, 0.6f), labWhite);
        Box(r, new Vector3(0f, 0.95f, 0.05f), new Vector3(1.8f, 0.08f, 0.7f), darkPlastic, new Vector3(-12f, 0f, 0f));
        for (int i = 0; i < 3; i++)
        {
            float x = -0.6f + i * 0.6f;
            Box(r, new Vector3(x, 1.35f, -0.15f), new Vector3(0.52f, 0.34f, 0.05f), darkPlastic);
            Box(r, new Vector3(x, 1.35f, -0.122f), new Vector3(0.48f, 0.3f, 0.004f), i == 1 ? alarmRed : monitorGreen);
        }
        for (int i = 0; i < 10; i++) Box(r, new Vector3(-0.8f + i * 0.17f, 1.0f, 0.15f), new Vector3(0.05f, 0.02f, 0.05f), i % 3 == 0 ? ledRed : ledGreen, new Vector3(-12f, 0f, 0f));
        return r.gameObject;
    }

    static GameObject GlassShards()
    {
        Transform r = Root("GlassShards");
        for (int i = 0; i < 7; i++)
            Box(r, new Vector3((Hash(i, 12) - 0.5f) * 0.7f, 0.005f, (Hash(i, 13) - 0.5f) * 0.7f), new Vector3(0.05f + Hash(i, 14) * 0.12f, 0.008f, 0.04f + Hash(i, 15) * 0.08f),
                tankGlass, new Vector3(0f, Hash(i, 16) * 180f, 0f));
        return r.gameObject;
    }

    static GameObject BloodPool()
    {
        Transform r = Root("BloodPool");
        Cyl(r, new Vector3(0f, 0.003f, 0f), 0.7f, 0.004f, blood);
        Cyl(r, new Vector3(0.32f, 0.003f, 0.12f), 0.3f, 0.004f, blood);
        Cyl(r, new Vector3(-0.2f, 0.003f, 0.35f), 0.18f, 0.004f, blood);
        return r.gameObject;
    }

    
    static GameObject FluidSpill()
    {
        Transform r = Root("FluidSpill");
        Cyl(r, new Vector3(0f, 0.004f, 0f), 1.3f, 0.006f, tankFluid);
        Cyl(r, new Vector3(0.55f, 0.004f, 0.3f), 0.6f, 0.006f, tankFluid);
        for (int i = 0; i < 5; i++)
            Box(r, new Vector3((Hash(i, 8) - 0.5f) * 1f, 0.008f, (Hash(i, 9) - 0.5f) * 1f), new Vector3(0.08f + Hash(i, 1) * 0.1f, 0.008f, 0.06f), tankGlass,
                new Vector3(0f, Hash(i, 7) * 180f, 0f));
        return r.gameObject;
    }

    static GameObject DragMarks()
    {
        Transform r = Root("DragMarks");
        for (int i = 0; i < 3; i++) Box(r, new Vector3(i * 0.12f - 0.12f, 0.003f, 0f), new Vector3(0.04f, 0.004f, 1.6f), blood, new Vector3(0f, 6f - i * 4f, 0f));
        return r.gameObject;
    }
}
