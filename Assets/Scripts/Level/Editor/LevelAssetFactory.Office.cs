using UnityEngine;




public static partial class LevelAssetFactory
{
    const string OfficeProps = PrefabRoot + "/Props";
    const string OfficeClutter = PrefabRoot + "/Clutter";

    static Material wall, floor, ceiling, wood, metal, darkPlastic, labWhite, screen, staticScreen, chem, fluorescent, fluorescentDead,
        paper, cardboard, bottleBlue, sodaRed, binderBlue, ledGreen, ledRed;

    static void BuildOffice()
    {
        wall = Mat("Wall", new Color(0.52f, 0.56f, 0.47f), 0.15f);
        floor = Mat("Floor", new Color(0.4f, 0.42f, 0.36f), 0.45f, baseMap: concreteTex, perMeter: Vector2.one / 2f);
        ceiling = Mat("Ceiling", new Color(0.55f, 0.57f, 0.52f), 0.05f, baseMap: ceilingTilesTex, perMeter: Vector2.one / 1.2f);
        wood = Mat("DeskWood", new Color(0.33f, 0.26f, 0.19f), 0.3f);
        metal = Mat("Metal", new Color(0.42f, 0.45f, 0.46f), 0.55f, metallic: 0.6f);
        darkPlastic = Mat("DarkPlastic", new Color(0.08f, 0.085f, 0.09f), 0.35f);
        labWhite = Mat("LabWhite", new Color(0.72f, 0.74f, 0.7f), 0.3f);
        screen = Mat("Screen", new Color(0.02f, 0.04f, 0.05f), 0.8f, new Color(0.2f, 0.55f, 0.7f) * 1.5f);
        
        staticScreen = Mat("StaticScreen", new Color(0.1f, 0.1f, 0.11f), 0.85f, new Color(0.75f, 0.82f, 0.95f) * 1.6f,
            baseMap: staticTex, perMeter: Vector2.one * 0.6f, emissionMap: staticTex);
        chem = Mat("ChemicalGreen", new Color(0.2f, 0.9f, 0.4f), 0.8f, new Color(0.1f, 1f, 0.3f) * 1.2f);
        fluorescent = Mat("Fluorescent", new Color(0.7f, 1f, 0.95f), 0.5f, new Color(0.45f, 1f, 0.85f) * 4f);
        fluorescentDead = Mat("FluorescentDead", new Color(0.35f, 0.37f, 0.36f), 0.3f);
        paper = Mat("Paper", new Color(0.72f, 0.7f, 0.62f), 0.05f);
        cardboard = Mat("Cardboard", new Color(0.45f, 0.33f, 0.21f), 0.1f);
        bottleBlue = Mat("BottleBlue", new Color(0.35f, 0.55f, 0.75f), 0.85f);
        sodaRed = Mat("SodaRed", new Color(0.6f, 0.06f, 0.05f), 0.7f, metallic: 0.5f);
        binderBlue = Mat("BinderBlue", new Color(0.1f, 0.18f, 0.35f), 0.4f);
        ledGreen = Mat("LedGreen", new Color(0.1f, 1f, 0.3f), 0.5f, new Color(0.1f, 1f, 0.3f) * 3f);
        ledRed = Mat("LedRed", new Color(1f, 0.1f, 0.05f), 0.5f, new Color(1f, 0.08f, 0.04f) * 3f);

        SavePrefab(Workstation, $"{OfficeProps}/Workstation.prefab", PrefabKind.Prop);
        SavePrefab(LaptopDesk, $"{OfficeProps}/LaptopDesk.prefab", PrefabKind.Prop);
        SavePrefab(LabBench, $"{OfficeProps}/LabBench.prefab", PrefabKind.Prop);
        SavePrefab(FilingCabinet, $"{OfficeProps}/FilingCabinet.prefab", PrefabKind.Prop);
        SavePrefab(ServerRack, $"{OfficeProps}/ServerRack.prefab", PrefabKind.Prop);
        SavePrefab(OfficeChair, $"{OfficeProps}/OfficeChair.prefab", PrefabKind.Prop);
        SavePrefab(BoxStack, $"{OfficeProps}/BoxStack.prefab", PrefabKind.Prop);
        SavePrefab(WaterCooler, $"{OfficeProps}/WaterCooler.prefab", PrefabKind.Prop);
        SavePrefab(Bookshelf, $"{OfficeProps}/Bookshelf.prefab", PrefabKind.Prop);
        SavePrefab(PaperSheet, $"{OfficeClutter}/PaperSheet.prefab", PrefabKind.Clutter);
        SavePrefab(CrumpledPaper, $"{OfficeClutter}/CrumpledPaper.prefab", PrefabKind.Clutter);
        SavePrefab(SodaCan, $"{OfficeClutter}/SodaCan.prefab", PrefabKind.Clutter);
        SavePrefab(Binder, $"{OfficeClutter}/Binder.prefab", PrefabKind.Clutter);
        SavePrefab(CeilingLight, $"{PrefabRoot}/CeilingLight.prefab", PrefabKind.Fixture);

        SaveTheme("Office", t =>
        {
            t.displayName = "Abandoned Offices (B3)";
            t.wallMaterial = wall;
            t.floorMaterial = floor;
            t.ceilingMaterial = ceiling;
            t.wallHeight = 3f;
            t.roomCount = 4;
            t.roomSizeRange = new Vector2Int(2, 3);
            t.startRoomSize = Vector2Int.zero;
            t.featureRooms = new System.Collections.Generic.List<RoomFurnishing>();
            t.wallProps = new System.Collections.Generic.List<PropEntry>
            {
                Entry($"{OfficeProps}/Workstation.prefab", 3f), Entry($"{OfficeProps}/LaptopDesk.prefab", 2f),
                Entry($"{OfficeProps}/LabBench.prefab", 2f), Entry($"{OfficeProps}/FilingCabinet.prefab", 2f, 0.15f),
                Entry($"{OfficeProps}/ServerRack.prefab", 1f), Entry($"{OfficeProps}/OfficeChair.prefab", 1.5f, 0.5f),
                Entry($"{OfficeProps}/BoxStack.prefab", 1.5f, 0.2f), Entry($"{OfficeProps}/WaterCooler.prefab", 0.7f, 0.4f),
                Entry($"{OfficeProps}/Bookshelf.prefab", 1f, 0.1f),
            };
            t.roomWallProps = new System.Collections.Generic.List<PropEntry>();
            t.clutter = new System.Collections.Generic.List<PropEntry>
            {
                Entry($"{OfficeClutter}/PaperSheet.prefab"), Entry($"{OfficeClutter}/CrumpledPaper.prefab"),
                Entry($"{OfficeClutter}/SodaCan.prefab"), Entry($"{OfficeClutter}/Binder.prefab"),
            };
            t.wallPropChance = 0.65f;
            t.maxWallPropsPerCell = 2;
            t.maxClutterPerCell = 3;
            t.lightFixture = Load<GameObject>($"{PrefabRoot}/CeilingLight.prefab");
            t.deadFixtureMaterial = fluorescentDead;
            
            t.lightChance = 0.06f;
            t.flickerChance = 0.6f;
            t.deadChance = 0.55f;
            t.cobwebChance = 0.3f;
            t.cobwebs = new System.Collections.Generic.List<GameObject>
            {
                Load<GameObject>($"{PrefabRoot}/Cobwebs/CornerWeb.prefab"), Load<GameObject>($"{PrefabRoot}/Cobwebs/CornerWebDense.prefab"),
            };
            t.ambientColor = new Color(0.008f, 0.012f, 0.014f);
            t.fogColor = new Color(0.004f, 0.008f, 0.01f);
            t.fogDensity = 0.065f;
        });
    }

    


    static void Desk(Transform r, float width, float depth, float height = 0.75f)
    {
        Box(r, new Vector3(0f, height - 0.025f, 0f), new Vector3(width, 0.05f, depth), wood);
        float lx = width / 2f - 0.05f, lz = depth / 2f - 0.05f;
        foreach (int sx in new[] { -1, 1 })
        foreach (int sz in new[] { -1, 1 })
            Box(r, new Vector3(sx * lx, (height - 0.05f) / 2f, sz * lz), new Vector3(0.05f, height - 0.05f, 0.05f), metal);
        Box(r, new Vector3(0f, height * 0.55f, -depth / 2f + 0.03f), new Vector3(width - 0.1f, height * 0.5f, 0.02f), metal);
    }

    
    static void Chair(Transform r, Vector3 at, float yaw)
    {
        var c = new GameObject("Chair").transform;
        c.SetParent(r, false);
        c.localPosition = at;
        c.localEulerAngles = new Vector3(0f, yaw, 0f);
        Cyl(c, new Vector3(0f, 0.04f, 0f), 0.55f, 0.04f, darkPlastic);
        Cyl(c, new Vector3(0f, 0.26f, 0f), 0.05f, 0.4f, metal);
        Box(c, new Vector3(0f, 0.48f, 0f), new Vector3(0.48f, 0.07f, 0.46f), darkPlastic);
        Box(c, new Vector3(0f, 0.8f, 0.22f), new Vector3(0.46f, 0.55f, 0.06f), darkPlastic, new Vector3(-8f, 0f, 0f));
    }

    
    static void StaticMonitor(Transform r, Vector3 screenPos, Vector3 screenSize, Vector3 euler)
    {
        Transform live = Live(r);
        GameObject glass = Box(live, screenPos, screenSize, staticScreen, euler);
        glass.name = "Screen";
        Light glow = PointLight(live, "Screen Glow", screenPos + Quaternion.Euler(euler) * new Vector3(0f, 0f, 0.35f),
            new Color(0.72f, 0.8f, 1f), 1.6f, 3.4f);
        var monitor = r.gameObject.AddComponent<MonitorStatic>();
        SetFields(monitor, ("screen", glass.GetComponent<Renderer>()), ("glow", glow));
    }

    static GameObject Workstation()
    {
        Transform r = Root("Workstation");
        Desk(r, 1.4f, 0.7f);
        Box(r, new Vector3(0f, 0.82f, -0.2f), new Vector3(0.08f, 0.12f, 0.08f), darkPlastic);
        Box(r, new Vector3(0f, 1.02f, -0.2f), new Vector3(0.56f, 0.34f, 0.04f), darkPlastic);
        StaticMonitor(r, new Vector3(0f, 1.02f, -0.178f), new Vector3(0.52f, 0.3f, 0.005f), Vector3.zero);
        Box(r, new Vector3(0f, 0.76f, 0.08f), new Vector3(0.44f, 0.02f, 0.14f), darkPlastic);
        Cyl(r, new Vector3(0.45f, 0.8f, -0.05f), 0.08f, 0.1f, labWhite);
        Box(r, new Vector3(-0.45f, 0.752f, 0.05f), new Vector3(0.21f, 0.004f, 0.3f), paper, new Vector3(0f, 12f, 0f));
        Chair(r, new Vector3(0.1f, 0f, 0.55f), 14f);
        return r.gameObject;
    }

    static GameObject LaptopDesk()
    {
        Transform r = Root("LaptopDesk");
        Desk(r, 1.2f, 0.6f);
        Box(r, new Vector3(0.1f, 0.76f, 0f), new Vector3(0.34f, 0.02f, 0.24f), darkPlastic);
        Box(r, new Vector3(0.1f, 0.87f, -0.14f), new Vector3(0.34f, 0.22f, 0.012f), darkPlastic, new Vector3(-12f, 0f, 0f));
        StaticMonitor(r, new Vector3(0.1f, 0.87f, -0.133f), new Vector3(0.31f, 0.19f, 0.003f), new Vector3(-12f, 0f, 0f));
        Box(r, new Vector3(-0.35f, 0.752f, 0.05f), new Vector3(0.21f, 0.004f, 0.3f), paper, new Vector3(0f, 15f, 0f));
        Box(r, new Vector3(-0.38f, 0.756f, 0.02f), new Vector3(0.21f, 0.004f, 0.3f), paper, new Vector3(0f, -8f, 0f));
        Cyl(r, new Vector3(-0.2f, 0.8f, -0.15f), 0.08f, 0.1f, labWhite);
        return r.gameObject;
    }

    static GameObject LabBench()
    {
        Transform r = Root("LabBench");
        Box(r, new Vector3(0f, 0.43f, 0f), new Vector3(1.8f, 0.86f, 0.65f), labWhite);
        Box(r, new Vector3(0f, 0.88f, 0f), new Vector3(1.86f, 0.04f, 0.7f), darkPlastic);
        
        Box(r, new Vector3(0f, 1.35f, -0.25f), new Vector3(1.8f, 0.03f, 0.22f), metal);
        Box(r, new Vector3(-0.85f, 1.12f, -0.3f), new Vector3(0.03f, 0.45f, 0.03f), metal);
        Box(r, new Vector3(0.85f, 1.12f, -0.3f), new Vector3(0.03f, 0.45f, 0.03f), metal);
        Cyl(r, new Vector3(-0.5f, 1.43f, -0.25f), 0.1f, 0.14f, bottleBlue);
        Cyl(r, new Vector3(0.2f, 1.42f, -0.25f), 0.08f, 0.12f, chem);
        
        Cyl(r, new Vector3(-0.6f, 0.97f, 0.05f), 0.09f, 0.14f, chem);
        Cyl(r, new Vector3(-0.45f, 0.96f, -0.1f), 0.07f, 0.12f, chem);
        Ball(r, new Vector3(-0.2f, 0.97f, 0.1f), Vector3.one * 0.14f, chem);
        Cyl(r, new Vector3(-0.2f, 1.08f, 0.1f), 0.035f, 0.1f, labWhite);
        
        Box(r, new Vector3(0.12f, 0.93f, -0.12f), new Vector3(0.26f, 0.06f, 0.07f), wood);
        for (int i = 0; i < 4; i++) Cyl(r, new Vector3(0.03f + i * 0.06f, 0.99f, -0.12f), 0.02f, 0.12f, chem);
        
        Box(r, new Vector3(0.55f, 0.915f, 0f), new Vector3(0.18f, 0.03f, 0.22f), metal);
        Box(r, new Vector3(0.55f, 1.05f, -0.07f), new Vector3(0.05f, 0.28f, 0.05f), metal, new Vector3(-10f, 0f, 0f));
        Box(r, new Vector3(0.55f, 1.0f, 0.02f), new Vector3(0.12f, 0.015f, 0.12f), darkPlastic);
        Cyl(r, new Vector3(0.55f, 1.13f, 0.01f), 0.05f, 0.18f, metal, new Vector3(20f, 0f, 0f));
        return r.gameObject;
    }

    static GameObject FilingCabinet()
    {
        Transform r = Root("FilingCabinet");
        Box(r, new Vector3(0f, 0.66f, 0f), new Vector3(0.5f, 1.32f, 0.6f), darkPlastic);
        for (int i = 0; i < 4; i++)
        {
            Box(r, new Vector3(0f, 0.17f + i * 0.33f, 0.3f), new Vector3(0.46f, 0.3f, 0.02f), metal);
            Box(r, new Vector3(0f, 0.25f + i * 0.33f, 0.315f), new Vector3(0.12f, 0.02f, 0.02f), darkPlastic);
        }
        return r.gameObject;
    }

    static GameObject ServerRack()
    {
        Transform r = Root("ServerRack");
        Box(r, new Vector3(0f, 1f, 0f), new Vector3(0.6f, 2f, 0.8f), darkPlastic);
        for (int i = 0; i < 12; i++)
        {
            float y = 0.25f + i * 0.13f;
            Box(r, new Vector3(0f, y, 0.402f), new Vector3(0.52f, 0.1f, 0.01f), metal);
            Box(r, new Vector3(-0.21f, y, 0.41f), Vector3.one * 0.015f, i % 3 == 0 ? ledRed : ledGreen);
            Box(r, new Vector3(-0.18f, y, 0.41f), Vector3.one * 0.015f, ledGreen);
        }
        return r.gameObject;
    }

    static GameObject OfficeChair()
    {
        Transform r = Root("OfficeChair");
        
        Chair(r, Vector3.zero, 180f);
        return r.gameObject;
    }

    static GameObject BoxStack()
    {
        Transform r = Root("BoxStack");
        Box(r, new Vector3(0f, 0.2f, 0f), new Vector3(0.6f, 0.4f, 0.45f), cardboard);
        Box(r, new Vector3(0.05f, 0.6f, 0.02f), new Vector3(0.5f, 0.4f, 0.4f), cardboard, new Vector3(0f, 8f, 0f));
        Box(r, new Vector3(-0.45f, 0.15f, 0.1f), new Vector3(0.35f, 0.3f, 0.35f), cardboard, new Vector3(0f, -15f, 0f));
        return r.gameObject;
    }

    static GameObject WaterCooler()
    {
        Transform r = Root("WaterCooler");
        Box(r, new Vector3(0f, 0.5f, 0f), new Vector3(0.32f, 1f, 0.32f), labWhite);
        Cyl(r, new Vector3(0f, 1.22f, 0f), 0.27f, 0.42f, bottleBlue);
        Box(r, new Vector3(0f, 0.78f, 0.17f), new Vector3(0.06f, 0.04f, 0.04f), darkPlastic);
        return r.gameObject;
    }

    static GameObject Bookshelf()
    {
        Transform r = Root("Bookshelf");
        Box(r, new Vector3(-0.45f, 0.9f, 0f), new Vector3(0.03f, 1.8f, 0.32f), wood);
        Box(r, new Vector3(0.45f, 0.9f, 0f), new Vector3(0.03f, 1.8f, 0.32f), wood);
        Box(r, new Vector3(0f, 0.9f, -0.15f), new Vector3(0.9f, 1.8f, 0.02f), wood);
        float[] shelves = { 0.02f, 0.45f, 0.9f, 1.35f, 1.78f };
        foreach (float y in shelves) Box(r, new Vector3(0f, y, 0f), new Vector3(0.9f, 0.03f, 0.32f), wood);

        Material[] binderMats = { binderBlue, cardboard, paper, darkPlastic, binderBlue };
        for (int s = 0; s < 4; s++)
        {
            float x = -0.4f;
            for (int i = 0; x < 0.38f; i++)
            {
                float w = 0.05f + (i * 7 + s * 3) % 4 * 0.015f;
                
                if ((i + s) % 5 != 3)
                {
                    float lean = (i + s) % 7 == 0 ? 12f : 0f;
                    Box(r, new Vector3(x + w / 2f, shelves[s] + 0.17f, 0f), new Vector3(w, 0.3f, 0.24f), binderMats[(i + s) % binderMats.Length], new Vector3(0f, 0f, lean));
                }
                x += w + 0.005f;
            }
        }
        return r.gameObject;
    }

    

    static GameObject PaperSheet()
    {
        Transform r = Root("PaperSheet");
        Box(r, new Vector3(0f, 0.002f, 0f), new Vector3(0.21f, 0.003f, 0.3f), paper);
        Box(r, new Vector3(0.06f, 0.005f, 0.04f), new Vector3(0.21f, 0.003f, 0.3f), paper, new Vector3(0f, 25f, 0f));
        return r.gameObject;
    }

    static GameObject CrumpledPaper()
    {
        Transform r = Root("CrumpledPaper");
        Ball(r, new Vector3(0f, 0.035f, 0f), new Vector3(0.08f, 0.065f, 0.07f), paper);
        return r.gameObject;
    }

    static GameObject SodaCan()
    {
        Transform r = Root("SodaCan");
        Cyl(r, new Vector3(0f, 0.033f, 0f), 0.066f, 0.12f, sodaRed, new Vector3(90f, 0f, 0f));
        return r.gameObject;
    }

    static GameObject Binder()
    {
        Transform r = Root("Binder");
        Box(r, new Vector3(0f, 0.025f, 0f), new Vector3(0.3f, 0.05f, 0.25f), binderBlue);
        return r.gameObject;
    }

    
    static GameObject CeilingLight() => Fixture("CeilingLight", r =>
    {
        Box(r, new Vector3(0f, -0.03f, 0f), new Vector3(1.3f, 0.06f, 0.26f), metal);
        Box(r, new Vector3(0f, -0.075f, 0f), new Vector3(1.2f, 0.04f, 0.12f), fluorescent);
    }, new Color(0.55f, 1f, 0.88f), 5f, 7f);
}
