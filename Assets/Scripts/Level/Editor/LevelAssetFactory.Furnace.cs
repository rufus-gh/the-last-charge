using System.Collections.Generic;
using UnityEngine;




public static partial class LevelAssetFactory
{
    const string FurnaceDir = PrefabRoot + "/Furnace";
    
    const float PipeSegmentLength = 4f;
    const float FurnaceWallHeight = 3.4f;

    static Material rust, pipeRed, pipeGrey, pipeLagging, pipeCopper, ductSteel, flameOrange, flameYellow, flameBlue, embers,
        firebrick, coal, hazardPaint, gaugeFace;

    static void BuildFurnace()
    {
        Material walls = Mat("Furnace/SootBrick", Color.white, 0.12f, baseMap: sootBrickTex, perMeter: Vector2.one / 1.6f);
        Material floor = Mat("Furnace/DiamondPlate", new Color(0.8f, 0.78f, 0.75f), 0.45f, metallic: 0.5f, baseMap: diamondPlateTex, perMeter: Vector2.one);
        Material ceilingMat = Mat("Furnace/Ceiling", new Color(0.12f, 0.11f, 0.1f), 0.05f, baseMap: concreteTex, perMeter: Vector2.one / 2f);
        rust = Mat("Furnace/Rust", new Color(0.3f, 0.16f, 0.09f), 0.25f, metallic: 0.4f);
        pipeRed = Mat("Furnace/PipeRed", new Color(0.42f, 0.08f, 0.05f), 0.35f, metallic: 0.3f);
        pipeGrey = Mat("Furnace/PipeGrey", new Color(0.3f, 0.31f, 0.32f), 0.45f, metallic: 0.7f);
        pipeLagging = Mat("Furnace/PipeLagging", new Color(0.62f, 0.59f, 0.5f), 0.1f);
        pipeCopper = Mat("Furnace/PipeCopper", new Color(0.5f, 0.28f, 0.14f), 0.5f, metallic: 0.85f);
        ductSteel = Mat("Furnace/DuctSteel", new Color(0.45f, 0.46f, 0.46f), 0.5f, metallic: 0.8f);
        flameOrange = Mat("Furnace/FlameOrange", new Color(1f, 0.45f, 0.08f), 0.2f, new Color(1f, 0.3f, 0.03f) * 2.6f);
        flameYellow = Mat("Furnace/FlameYellow", new Color(1f, 0.75f, 0.3f), 0.2f, new Color(1f, 0.6f, 0.15f) * 2.2f);
        flameBlue = Mat("Furnace/FlameBlue", new Color(0.3f, 0.5f, 1f), 0.2f, new Color(0.15f, 0.35f, 1f) * 2.4f);
        embers = Mat("Furnace/Embers", new Color(0.6f, 0.15f, 0.05f), 0.3f, new Color(1f, 0.25f, 0.04f) * 3f);
        firebrick = Mat("Furnace/Firebrick", new Color(0.12f, 0.05f, 0.03f), 0.1f);
        coal = Mat("Furnace/Coal", new Color(0.035f, 0.035f, 0.035f), 0.35f);
        hazardPaint = Mat("Furnace/HazardYellow", new Color(0.75f, 0.6f, 0.08f), 0.3f);
        gaugeFace = Mat("Furnace/GaugeFace", new Color(0.85f, 0.83f, 0.75f), 0.6f);

        SavePrefab(Furnace, $"{FurnaceDir}/Furnace.prefab", PrefabKind.Prop);
        SavePrefab(GasBurner, $"{FurnaceDir}/GasBurner.prefab", PrefabKind.Prop);
        SavePrefab(Boiler, $"{FurnaceDir}/Boiler.prefab", PrefabKind.Prop);
        SavePrefab(ValveManifold, $"{FurnaceDir}/ValveManifold.prefab", PrefabKind.Prop);
        SavePrefab(FuelDrums, $"{FurnaceDir}/FuelDrums.prefab", PrefabKind.Prop);
        SavePrefab(CoalPile, $"{FurnaceDir}/CoalPile.prefab", PrefabKind.Prop);
        SavePrefab(BoilerPlant, $"{FurnaceDir}/BoilerPlant.prefab", PrefabKind.Prop);
        SavePrefab(CoalLumps, $"{FurnaceDir}/CoalLumps.prefab", PrefabKind.Clutter);
        SavePrefab(Wrench, $"{FurnaceDir}/Wrench.prefab", PrefabKind.Clutter);
        SavePrefab(PipeOffcut, $"{FurnaceDir}/PipeOffcut.prefab", PrefabKind.Clutter);
        SavePrefab(OilyRag, $"{FurnaceDir}/OilyRag.prefab", PrefabKind.Clutter);
        SavePrefab(GasJet, $"{FurnaceDir}/GasJet.prefab", PrefabKind.Fixture);

        
        var styles = new[]
        {
            new[] { PipeSpec.Round(0.16f, -0.15f, 0.15f, pipeRed), PipeSpec.Round(0.11f, -0.13f, 0.36f, pipeGrey), PipeSpec.Round(0.07f, -0.29f, 0.26f, pipeCopper) },
            new[] { PipeSpec.Duct(0.42f, 0.24f, -0.15f, 0.28f, ductSteel), PipeSpec.Round(0.06f, -0.06f, 0.56f, pipeCopper) },
            new[] { PipeSpec.Round(0.28f, -0.2f, 0.2f, pipeLagging), PipeSpec.Round(0.06f, -0.06f, 0.45f, pipeRed), PipeSpec.Round(0.05f, -0.06f, 0.53f, pipeGrey) },
        };
        var runs = new List<PipeRun>();
        for (int i = 0; i < styles.Length; i++)
        {
            PipeSpec[] style = styles[i];
            string segmentPath = $"{FurnaceDir}/Pipes/PipeRun{i + 1}.prefab";
            string endPath = $"{FurnaceDir}/Pipes/PipeRun{i + 1}End.prefab";
            SavePrefab(() => PipeSegment($"PipeRun{i + 1}", style), segmentPath, PrefabKind.Decor);
            SavePrefab(() => PipeEnd($"PipeRun{i + 1}End", style), endPath, PrefabKind.Decor);
            runs.Add(new PipeRun { segment = Load<GameObject>(segmentPath), end = Load<GameObject>(endPath) });
        }

        SaveTheme("Furnace", t =>
        {
            t.displayName = "Furnace Level (B4)";
            t.wallMaterial = walls;
            t.floorMaterial = floor;
            t.ceilingMaterial = ceilingMat;
            t.wallHeight = FurnaceWallHeight;
            t.roomCount = 4;
            t.roomSizeRange = new Vector2Int(2, 3);
            t.startRoomSize = Vector2Int.zero;
            
            t.featureRooms = new List<RoomFurnishing>
            {
                Furnishing($"{FurnaceDir}/BoilerPlant.prefab", RoomLayout.Center),
                Furnishing($"{FurnaceDir}/BoilerPlant.prefab", RoomLayout.Center),
            };
            t.wallProps = new List<PropEntry>
            {
                Entry($"{FurnaceDir}/Furnace.prefab", 0.8f), Entry($"{FurnaceDir}/GasBurner.prefab", 0.9f),
                Entry($"{FurnaceDir}/Boiler.prefab", 2f), Entry($"{FurnaceDir}/ValveManifold.prefab", 2.5f),
                Entry($"{FurnaceDir}/FuelDrums.prefab", 2f, 0.25f), Entry($"{FurnaceDir}/CoalPile.prefab", 1.5f),
                Entry($"{OfficeProps}/BoxStack.prefab", 0.8f, 0.3f),
            };
            t.roomWallProps = new List<PropEntry>();
            t.clutter = new List<PropEntry>
            {
                Entry($"{FurnaceDir}/CoalLumps.prefab", 3f), Entry($"{FurnaceDir}/Wrench.prefab", 1f),
                Entry($"{FurnaceDir}/PipeOffcut.prefab", 1.5f), Entry($"{FurnaceDir}/OilyRag.prefab", 1.5f),
            };
            t.wallPropChance = 0.45f;
            t.maxWallPropsPerCell = 1;
            t.maxClutterPerCell = 2;
            
            t.lightFixture = Load<GameObject>($"{FurnaceDir}/GasJet.prefab");
            t.deadFixtureMaterial = rust;
            t.lightChance = 0.015f;
            t.flickerChance = 0f;
            t.deadChance = 0f;
            t.pipeChance = 0.6f;
            t.pipeRuns = runs;
            t.cobwebChance = 0.12f;
            t.cobwebs = new List<GameObject> { Load<GameObject>($"{PrefabRoot}/Cobwebs/CornerWebDense.prefab") };
            t.ambientColor = new Color(0.004f, 0.003f, 0.0025f);
            t.fogColor = new Color(0.006f, 0.003f, 0.002f);
            t.fogDensity = 0.07f;
        });
    }

    
    struct PipeSpec
    {
        public bool duct;
        public float width, height, y, z;
        public Material material;

        public static PipeSpec Round(float diameter, float y, float z, Material m) =>
            new PipeSpec { width = diameter, height = diameter, y = y, z = z, material = m };

        public static PipeSpec Duct(float width, float height, float y, float z, Material m) =>
            new PipeSpec { duct = true, width = width, height = height, y = y, z = z, material = m };
    }

    
    static GameObject PipeSegment(string name, PipeSpec[] pipes)
    {
        Transform r = Root(name);
        float half = PipeSegmentLength / 2f;
        float lowest = 0f, outmost = 0f;
        foreach (PipeSpec p in pipes)
        {
            if (p.duct)
            {
                Box(r, new Vector3(0f, p.y, p.z), new Vector3(PipeSegmentLength, p.height, p.width), p.material);
                
                for (int i = 0; i < 4; i++)
                    Box(r, new Vector3(-half + 0.5f + i, p.y, p.z), new Vector3(0.03f, p.height + 0.02f, p.width + 0.02f), p.material);
            }
            else
            {
                Cyl(r, new Vector3(0f, p.y, p.z), p.width, PipeSegmentLength, p.material, new Vector3(0f, 0f, 90f));
                
                Cyl(r, new Vector3(0f, p.y, p.z), p.width * 1.45f, 0.05f, p.material, new Vector3(0f, 0f, 90f));
            }
            lowest = Mathf.Min(lowest, p.y - p.height / 2f);
            outmost = Mathf.Max(outmost, p.z + p.width / 2f);
        }
        
        foreach (float x in new[] { -half + 0.6f, half - 0.6f })
        {
            Box(r, new Vector3(x, lowest / 2f, outmost + 0.02f), new Vector3(0.03f, -lowest + 0.02f, 0.03f), metal);
            Box(r, new Vector3(x, lowest - 0.015f, outmost / 2f), new Vector3(0.04f, 0.03f, outmost + 0.04f), metal);
        }
        return r.gameObject;
    }

    
    static GameObject PipeEnd(string name, PipeSpec[] pipes)
    {
        Transform r = Root(name);
        foreach (PipeSpec p in pipes)
        {
            if (p.duct)
            {
                Box(r, new Vector3(0f, p.y, p.z / 2f), new Vector3(p.width * 0.9f, p.height, p.z + p.width / 2f), p.material);
                Box(r, new Vector3(0f, p.y, 0.02f), new Vector3(p.width + 0.1f, p.height + 0.1f, 0.04f), p.material);
                continue;
            }
            Ball(r, new Vector3(0f, p.y, p.z), Vector3.one * p.width * 1.08f, p.material);
            Cyl(r, new Vector3(0f, p.y, p.z / 2f), p.width, p.z, p.material, new Vector3(90f, 0f, 0f));
            Cyl(r, new Vector3(0f, p.y, 0.02f), p.width * 1.6f, 0.04f, p.material, new Vector3(90f, 0f, 0f));
        }
        return r.gameObject;
    }

    
    static Transform Flame(Transform live, Vector3 pos, Vector3 size, Material m)
    {
        GameObject f = Ball(live, pos, size, m);
        f.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return f.transform;
    }

    static void AddFire(Transform r, Transform[] flames)
    {
        var fire = r.gameObject.AddComponent<FireFlicker>();
        SetFields(fire, ("flames", (Object[])flames));
    }

    static void Gauge(Transform r, Vector3 pos, Vector3 euler)
    {
        Transform g = Group(r, "Gauge", pos);
        g.localEulerAngles = euler;
        Cyl(g, Vector3.zero, 0.16f, 0.04f, pipeGrey, new Vector3(90f, 0f, 0f));
        Cyl(g, new Vector3(0f, 0f, 0.021f), 0.13f, 0.005f, gaugeFace, new Vector3(90f, 0f, 0f));
        Box(g, new Vector3(0.02f, 0.02f, 0.026f), new Vector3(0.06f, 0.008f, 0.004f), crashRed, new Vector3(0f, 0f, 40f));
    }

    static void ValveWheel(Transform r, Vector3 pos, float diameter, Vector3 euler)
    {
        Transform v = Group(r, "Valve", pos);
        v.localEulerAngles = euler;
        Cyl(v, Vector3.zero, diameter, 0.025f, pipeRed, new Vector3(90f, 0f, 0f));
        Cyl(v, new Vector3(0f, 0f, 0.003f), diameter - 0.05f, 0.03f, darkPlastic, new Vector3(90f, 0f, 0f));
        for (int i = 0; i < 3; i++) Box(v, new Vector3(0f, 0f, 0.01f), new Vector3(diameter - 0.03f, 0.02f, 0.02f), pipeRed, new Vector3(0f, 0f, i * 60f));
        Cyl(v, new Vector3(0f, 0f, -0.06f), 0.04f, 0.12f, pipeGrey, new Vector3(90f, 0f, 0f));
    }

    
    
    static GameObject Furnace()
    {
        Transform r = Root("Furnace");
        Box(r, new Vector3(0f, 0.9f, -0.05f), new Vector3(1.5f, 1.8f, 1f), rust);
        Box(r, new Vector3(0f, 1.85f, -0.05f), new Vector3(1.4f, 0.1f, 0.9f), pipeGrey);
        Box(r, new Vector3(0f, 0.1f, 0.48f), new Vector3(1.55f, 0.2f, 0.08f), hazardPaint);
        
        Box(r, new Vector3(0f, 0.75f, 0.44f), new Vector3(0.8f, 0.6f, 0.04f), firebrick);
        for (int i = 0; i < 5; i++) Box(r, new Vector3(-0.3f + i * 0.15f, 0.75f, 0.47f), new Vector3(0.025f, 0.6f, 0.025f), darkPlastic);
        for (int i = 0; i < 6; i++) Ball(r, new Vector3(-0.3f + i * 0.12f, 0.5f + Hash(i, 3) * 0.05f, 0.44f), Vector3.one * (0.12f + Hash(i, 4) * 0.06f), embers);
        
        Transform door = Group(r, "Door", new Vector3(0.42f, 0.75f, 0.47f), 110f);
        Box(door, new Vector3(-0.4f, 0f, 0f), new Vector3(0.8f, 0.65f, 0.05f), rust);
        Box(door, new Vector3(-0.7f, 0f, 0.05f), new Vector3(0.08f, 0.12f, 0.06f), pipeGrey);
        
        Cyl(r, new Vector3(-0.35f, 1.9f + (FurnaceWallHeight - 1.9f) / 2f, -0.25f), 0.32f, FurnaceWallHeight - 1.9f, rust);
        Gauge(r, new Vector3(0.45f, 1.45f, 0.46f), Vector3.zero);
        Gauge(r, new Vector3(0.2f, 1.45f, 0.46f), Vector3.zero);

        Transform live = Live(r);
        var flames = new[]
        {
            Flame(live, new Vector3(-0.15f, 0.72f, 0.42f), new Vector3(0.22f, 0.38f, 0.12f), flameOrange),
            Flame(live, new Vector3(0.12f, 0.7f, 0.42f), new Vector3(0.2f, 0.32f, 0.12f), flameYellow),
            Flame(live, new Vector3(0f, 0.8f, 0.4f), new Vector3(0.16f, 0.4f, 0.1f), flameOrange),
        };
        PointLight(live, "Fire Light", new Vector3(0f, 0.8f, 0.9f), new Color(1f, 0.5f, 0.18f), 3.8f, 7.5f);
        AddFire(r, flames);
        return r.gameObject;
    }

    
    static GameObject GasBurner()
    {
        Transform r = Root("GasBurner");
        foreach (float a in new[] { 0f, 120f, 240f })
        {
            Vector3 foot = Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 0f, 0.24f);
            Box(r, new Vector3(foot.x, 0.33f, 0.1f + foot.z), new Vector3(0.04f, 0.66f, 0.04f), pipeGrey);
        }
        Cyl(r, new Vector3(0f, 0.66f, 0.1f), 0.55f, 0.06f, pipeGrey);
        Cyl(r, new Vector3(0f, 0.7f, 0.1f), 0.45f, 0.03f, darkPlastic);
        
        Cyl(r, new Vector3(0f, 0.55f, -0.2f), 0.06f, 0.6f, pipeCopper, new Vector3(90f, 0f, 0f));
        Cyl(r, new Vector3(0f, 1.6f, -0.45f), 0.06f, 2.1f, pipeCopper);
        ValveWheel(r, new Vector3(0f, 1.1f, -0.38f), 0.22f, Vector3.zero);
        Box(r, new Vector3(0f, 0.004f, 0.1f), new Vector3(0.9f, 0.008f, 0.9f), hazardPaint);

        Transform live = Live(r);
        var flames = new List<Transform>();
        for (int i = 0; i < 8; i++)
        {
            Vector3 p = new Vector3(0f, 0.78f, 0.1f) + Quaternion.Euler(0f, i * 45f, 0f) * new Vector3(0f, 0f, 0.18f);
            flames.Add(Flame(live, p, new Vector3(0.07f, 0.16f, 0.07f), flameBlue));
            flames.Add(Flame(live, p + Vector3.up * 0.1f, new Vector3(0.05f, 0.12f, 0.05f), flameOrange));
        }
        flames.Add(Flame(live, new Vector3(0f, 0.86f, 0.1f), new Vector3(0.18f, 0.3f, 0.18f), flameOrange));
        PointLight(live, "Fire Light", new Vector3(0f, 1.1f, 0.4f), new Color(1f, 0.58f, 0.3f), 2.8f, 5.5f);
        AddFire(r, flames.ToArray());
        return r.gameObject;
    }

    
    static GameObject Boiler()
    {
        Transform r = Root("Boiler");
        Cyl(r, new Vector3(0f, 0.85f, 0f), 1.05f, 2.2f, rust, new Vector3(0f, 0f, 90f));
        foreach (float x in new[] { -1.1f, 1.1f }) Ball(r, new Vector3(x, 0.85f, 0f), new Vector3(0.3f, 1.05f, 1.05f), rust);
        foreach (float x in new[] { -0.7f, 0.7f }) Box(r, new Vector3(x, 0.25f, 0f), new Vector3(0.2f, 0.5f, 0.85f), pipeGrey);
        
        foreach (float x in new[] { -0.4f, 0.4f }) Cyl(r, new Vector3(x, 0.85f, 0f), 1.09f, 0.05f, pipeGrey, new Vector3(0f, 0f, 90f));
        Gauge(r, new Vector3(-0.3f, 1.2f, 0.49f), new Vector3(-20f, 0f, 0f));
        Gauge(r, new Vector3(0.1f, 1.25f, 0.47f), new Vector3(-25f, 0f, 0f));
        Cyl(r, new Vector3(0.6f, 1.38f, 0f), 0.12f, 0.2f, pipeGrey);
        ValveWheel(r, new Vector3(0.6f, 1.52f, 0f), 0.3f, new Vector3(90f, 0f, 0f));
        Cyl(r, new Vector3(-0.7f, 1.35f + (FurnaceWallHeight - 1.35f) / 2f, -0.1f), 0.16f, FurnaceWallHeight - 1.35f, pipeLagging);
        return r.gameObject;
    }

    
    static GameObject ValveManifold()
    {
        Transform r = Root("ValveManifold");
        float h = FurnaceWallHeight;
        float[] xs = { -0.5f, 0f, 0.5f };
        Material[] mats = { pipeRed, pipeGrey, pipeCopper };
        for (int i = 0; i < xs.Length; i++)
        {
            Cyl(r, new Vector3(xs[i], h / 2f, -0.08f), 0.12f, h, mats[i]);
            ValveWheel(r, new Vector3(xs[i], 1.1f + i * 0.12f, 0.02f), 0.26f, Vector3.zero);
            Cyl(r, new Vector3(xs[i], 0.6f, -0.08f), 0.17f, 0.05f, mats[i]);
        }
        Cyl(r, new Vector3(0f, 1.7f, -0.08f), 0.16f, 1.3f, pipeGrey, new Vector3(0f, 0f, 90f));
        Gauge(r, new Vector3(0.25f, 1.7f, 0.02f), Vector3.zero);
        Box(r, new Vector3(0f, 0.9f, -0.17f), new Vector3(1.3f, 1.8f, 0.02f), hazardPaint);
        return r.gameObject;
    }

    static void Drum(Transform r, Vector3 pos, Material m)
    {
        Cyl(r, pos + Vector3.up * 0.44f, 0.58f, 0.88f, m);
        foreach (float y in new[] { 0.2f, 0.68f }) Cyl(r, pos + Vector3.up * y, 0.6f, 0.03f, m);
        Cyl(r, pos + new Vector3(0.15f, 0.885f, 0.1f), 0.06f, 0.01f, pipeGrey);
    }

    static GameObject FuelDrums()
    {
        Transform r = Root("FuelDrums");
        Drum(r, new Vector3(-0.32f, 0f, 0f), pipeRed);
        Drum(r, new Vector3(0.32f, 0f, 0.05f), rust);
        Drum(r, new Vector3(0f, 0f, 0.5f), pipeGrey);
        return r.gameObject;
    }

    static GameObject CoalPile()
    {
        Transform r = Root("CoalPile");
        Ball(r, new Vector3(0f, 0.1f, 0.05f), new Vector3(1.5f, 0.55f, 0.95f), coal);
        Ball(r, new Vector3(-0.25f, 0.2f, -0.05f), new Vector3(0.8f, 0.55f, 0.7f), coal);
        for (int i = 0; i < 10; i++)
            Ball(r, new Vector3((Hash(i, 71) - 0.5f) * 1.6f, 0.05f, 0.35f + Hash(i, 72) * 0.25f), Vector3.one * (0.08f + Hash(i, 73) * 0.06f), coal);
        
        Box(r, new Vector3(0.35f, 0.55f, 0.05f), new Vector3(0.035f, 0.9f, 0.035f), wood, new Vector3(0f, 0f, -25f));
        Box(r, new Vector3(0.18f, 0.2f, 0.05f), new Vector3(0.22f, 0.26f, 0.02f), pipeGrey, new Vector3(0f, 0f, -25f));
        return r.gameObject;
    }

    
    static GameObject BoilerPlant()
    {
        Transform r = Root("BoilerPlant");
        Box(r, new Vector3(0f, 0.5f, 0f), new Vector3(4.2f, 1f, 1.8f), firebrick);
        Cyl(r, new Vector3(0f, 1.75f, 0f), 1.6f, 4f, rust, new Vector3(0f, 0f, 90f));
        foreach (float x in new[] { -2f, 2f }) Ball(r, new Vector3(x, 1.75f, 0f), new Vector3(0.4f, 1.6f, 1.6f), rust);
        foreach (float x in new[] { -1.2f, 0f, 1.2f }) Cyl(r, new Vector3(x, 1.75f, 0f), 1.66f, 0.06f, pipeGrey, new Vector3(0f, 0f, 90f));
        
        foreach (float x in new[] { -1.3f, 1.3f })
            Cyl(r, new Vector3(x, 2.5f + (FurnaceWallHeight - 2.5f) / 2f, 0f), 0.4f, FurnaceWallHeight - 2.5f, rust);

        Transform live = Live(r);
        var flames = new List<Transform>();
        foreach (int side in new[] { -1, 1 })
        for (int i = 0; i < 3; i++)
        {
            float x = -1.3f + i * 1.3f;
            Box(r, new Vector3(x, 0.45f, side * 0.91f), new Vector3(0.7f, 0.45f, 0.03f), darkPlastic);
            for (int k = 0; k < 4; k++) Ball(r, new Vector3(x - 0.24f + k * 0.16f, 0.3f, side * 0.88f), Vector3.one * 0.14f, embers);
            flames.Add(Flame(live, new Vector3(x - 0.1f, 0.48f, side * 0.88f), new Vector3(0.2f, 0.3f, 0.08f), flameOrange));
            flames.Add(Flame(live, new Vector3(x + 0.12f, 0.45f, side * 0.88f), new Vector3(0.16f, 0.24f, 0.08f), flameYellow));
        }
        foreach (int side in new[] { -1, 1 })
            PointLight(live, "Fire Light", new Vector3(0f, 0.6f, side * 1.6f), new Color(1f, 0.48f, 0.16f), 4.5f, 8f);
        AddFire(r, flames.ToArray());
        return r.gameObject;
    }

    
    
    static GameObject GasJet()
    {
        Transform r = Root("GasJet");
        Cyl(r, new Vector3(0f, -0.02f, 0f), 0.12f, 0.04f, pipeCopper);
        Cyl(r, new Vector3(0f, -0.4f, 0f), 0.05f, 0.8f, pipeCopper);
        Ball(r, new Vector3(0f, -0.8f, 0f), Vector3.one * 0.06f, pipeCopper);
        Cyl(r, new Vector3(0.1f, -0.8f, 0f), 0.05f, 0.2f, pipeCopper, new Vector3(0f, 0f, 90f));
        Ball(r, new Vector3(0.2f, -0.8f, 0f), Vector3.one * 0.06f, pipeCopper);
        Cyl(r, new Vector3(0.2f, -0.74f, 0f), 0.07f, 0.1f, pipeGrey);
        Transform live = Live(r);
        var flames = new[]
        {
            Flame(live, new Vector3(0.2f, -0.65f, 0f), new Vector3(0.06f, 0.1f, 0.06f), flameBlue),
            Flame(live, new Vector3(0.2f, -0.56f, 0f), new Vector3(0.08f, 0.18f, 0.08f), flameOrange),
        };
        PointLight(live, "Fire Light", new Vector3(0.2f, -0.75f, 0f), new Color(1f, 0.6f, 0.3f), 2.4f, 6f);
        AddFire(r, flames);
        return r.gameObject;
    }

    static GameObject CoalLumps()
    {
        Transform r = Root("CoalLumps");
        for (int i = 0; i < 5; i++)
            Ball(r, new Vector3((Hash(i, 81) - 0.5f) * 0.4f, 0.03f, (Hash(i, 82) - 0.5f) * 0.4f), Vector3.one * (0.05f + Hash(i, 83) * 0.05f), coal);
        return r.gameObject;
    }

    static GameObject Wrench()
    {
        Transform r = Root("Wrench");
        Box(r, new Vector3(0f, 0.01f, 0f), new Vector3(0.32f, 0.015f, 0.04f), pipeGrey);
        Cyl(r, new Vector3(0.17f, 0.01f, 0f), 0.08f, 0.015f, pipeGrey);
        return r.gameObject;
    }

    static GameObject PipeOffcut()
    {
        Transform r = Root("PipeOffcut");
        Cyl(r, new Vector3(0f, 0.05f, 0f), 0.1f, 0.6f, pipeGrey, new Vector3(0f, 0f, 90f));
        return r.gameObject;
    }

    static GameObject OilyRag()
    {
        Transform r = Root("OilyRag");
        Box(r, new Vector3(0f, 0.01f, 0f), new Vector3(0.3f, 0.02f, 0.25f), coal, new Vector3(0f, 20f, 3f));
        Box(r, new Vector3(0.08f, 0.02f, 0.04f), new Vector3(0.18f, 0.02f, 0.14f), pipeLagging, new Vector3(4f, -15f, 0f));
        return r.gameObject;
    }
}
