using System.Collections.Generic;
using UnityEngine;




public static partial class LevelAssetFactory
{
    const string HospitalDir = PrefabRoot + "/Hospital";

    static Material steel, sheetBlue, curtainGreen, monitorGreen, mriWhite, crashRed, rubber, pillOrange, coldPanel, surgicalLamp;

    static void BuildHospital()
    {
        Material walls = Mat("Hospital/TileWall", new Color(0.66f, 0.78f, 0.72f), 0.55f, baseMap: hospitalTileTex, perMeter: Vector2.one / 1.2f);
        Material floor = Mat("Hospital/TileFloor", new Color(0.8f, 0.82f, 0.8f), 0.6f, baseMap: hospitalTileTex, perMeter: Vector2.one / 1.2f);
        Material ceilingMat = Mat("Hospital/CeilingTiles", new Color(0.8f, 0.82f, 0.8f), 0.05f, baseMap: ceilingTilesTex, perMeter: Vector2.one / 1.2f);
        steel = Mat("Hospital/Steel", new Color(0.62f, 0.64f, 0.66f), 0.7f, metallic: 0.8f);
        sheetBlue = Mat("Hospital/SheetBlue", new Color(0.55f, 0.68f, 0.78f), 0.1f);
        curtainGreen = Mat("Hospital/CurtainGreen", new Color(0.4f, 0.55f, 0.5f), 0.05f);
        monitorGreen = Mat("Hospital/MonitorGreen", new Color(0.02f, 0.06f, 0.03f), 0.8f, new Color(0.1f, 1f, 0.35f) * 1.8f);
        mriWhite = Mat("Hospital/MriWhite", new Color(0.88f, 0.89f, 0.9f), 0.5f);
        crashRed = Mat("Hospital/CrashRed", new Color(0.6f, 0.06f, 0.05f), 0.4f);
        rubber = Mat("Hospital/Rubber", new Color(0.05f, 0.05f, 0.05f), 0.2f);
        pillOrange = Mat("Hospital/PillOrange", new Color(0.85f, 0.45f, 0.1f), 0.6f);
        coldPanel = Mat("Hospital/ColdPanel", new Color(0.9f, 0.97f, 1f), 0.4f, new Color(0.85f, 0.95f, 1f) * 3.5f);
        surgicalLamp = Mat("Hospital/SurgicalLamp", new Color(1f, 1f, 0.95f), 0.5f, new Color(1f, 0.98f, 0.9f) * 2.5f);

        SavePrefab(Gurney, $"{HospitalDir}/Gurney.prefab", PrefabKind.Prop);
        SavePrefab(Wheelchair, $"{HospitalDir}/Wheelchair.prefab", PrefabKind.Prop);
        SavePrefab(CrashCart, $"{HospitalDir}/CrashCart.prefab", PrefabKind.Prop);
        SavePrefab(IVStand, $"{HospitalDir}/IVStand.prefab", PrefabKind.Prop);
        SavePrefab(WaitingBench, $"{HospitalDir}/WaitingBench.prefab", PrefabKind.Prop);
        SavePrefab(HospitalBed, $"{HospitalDir}/HospitalBed.prefab", PrefabKind.Prop);
        SavePrefab(HeartMonitor, $"{HospitalDir}/HeartMonitor.prefab", PrefabKind.Prop);
        SavePrefab(PrivacyCurtain, $"{HospitalDir}/PrivacyCurtain.prefab", PrefabKind.Prop);
        SavePrefab(BedsideCabinet, $"{HospitalDir}/BedsideCabinet.prefab", PrefabKind.Prop);
        SavePrefab(MriMachine, $"{HospitalDir}/MriMachine.prefab", PrefabKind.Prop);
        SavePrefab(OperatingTable, $"{HospitalDir}/OperatingTable.prefab", PrefabKind.Prop);
        SavePrefab(PillBottle, $"{HospitalDir}/PillBottle.prefab", PrefabKind.Clutter);
        SavePrefab(Clipboard, $"{HospitalDir}/Clipboard.prefab", PrefabKind.Clutter);
        SavePrefab(Syringe, $"{HospitalDir}/Syringe.prefab", PrefabKind.Clutter);
        SavePrefab(GauzePad, $"{HospitalDir}/GauzePad.prefab", PrefabKind.Clutter);
        SavePrefab(() => Fixture("ColdPanelLight", r =>
        {
            Box(r, new Vector3(0f, -0.025f, 0f), new Vector3(1.25f, 0.05f, 0.34f), metal);
            Box(r, new Vector3(0f, -0.055f, 0f), new Vector3(1.18f, 0.015f, 0.28f), coldPanel);
        }, new Color(0.85f, 0.95f, 1f), 5f, 7f), $"{HospitalDir}/ColdPanelLight.prefab", PrefabKind.Fixture);

        SaveTheme("Hospital", t =>
        {
            t.displayName = "St. Mercy Hospital";
            t.wallMaterial = walls;
            t.floorMaterial = floor;
            t.ceilingMaterial = ceilingMat;
            t.wallHeight = 3f;
            
            t.roomCount = 6;
            t.roomSizeRange = new Vector2Int(2, 2);
            t.startRoomSize = Vector2Int.zero;
            t.featureRooms = new List<RoomFurnishing>
            {
                Furnishing($"{HospitalDir}/MriMachine.prefab", RoomLayout.Center),
                Furnishing($"{HospitalDir}/OperatingTable.prefab", RoomLayout.Center),
            };
            t.wallProps = new List<PropEntry>
            {
                Entry($"{HospitalDir}/Gurney.prefab", 3f, 0.15f), Entry($"{HospitalDir}/Wheelchair.prefab", 2f, 0.4f),
                Entry($"{HospitalDir}/CrashCart.prefab", 1.5f, 0.2f), Entry($"{HospitalDir}/IVStand.prefab", 1.5f, 0.4f),
                Entry($"{HospitalDir}/WaitingBench.prefab", 1.5f), Entry($"{OfficeProps}/FilingCabinet.prefab", 0.5f),
            };
            t.roomWallProps = new List<PropEntry>
            {
                Entry($"{HospitalDir}/HospitalBed.prefab", 4f), Entry($"{HospitalDir}/HeartMonitor.prefab", 2f, 0.2f),
                Entry($"{HospitalDir}/PrivacyCurtain.prefab", 2f), Entry($"{HospitalDir}/BedsideCabinet.prefab", 1.5f),
                Entry($"{HospitalDir}/IVStand.prefab", 1.5f, 0.3f),
            };
            t.clutter = new List<PropEntry>
            {
                Entry($"{HospitalDir}/PillBottle.prefab", 2f), Entry($"{HospitalDir}/Clipboard.prefab", 1.5f),
                Entry($"{HospitalDir}/Syringe.prefab", 1.5f), Entry($"{HospitalDir}/GauzePad.prefab", 2f),
                Entry($"{OfficeClutter}/PaperSheet.prefab", 1.5f),
            };
            t.wallPropChance = 0.55f;
            t.maxWallPropsPerCell = 2;
            t.maxClutterPerCell = 3;
            t.lightFixture = Load<GameObject>($"{HospitalDir}/ColdPanelLight.prefab");
            t.deadFixtureMaterial = fluorescentDead;
            t.lightChance = 0.18f;
            t.flickerChance = 0.45f;
            t.deadChance = 0.3f;
            t.ambientColor = new Color(0.03f, 0.034f, 0.036f);
            t.fogColor = new Color(0.012f, 0.017f, 0.018f);
            t.fogDensity = 0.03f;
            
            t.windowChance = 0.25f;
            
            t.windowWidth = HospitalWindowSize.x / 4f;
            t.windowSill = HospitalSill;
            t.windowHeader = t.wallHeight - HospitalSill - HospitalWindowSize.y;
            t.windows = new List<PropEntry>
            {
                Entry(WindowPath("Hospital", WindowStyle.Intact), 0.3f),
                Entry(WindowPath("Hospital", WindowStyle.Boarded), 0.35f),
                Entry(WindowPath("Hospital", WindowStyle.Broken), 0.35f),
            };
            t.outdoors = true;
            t.skybox = overcastSky;
            t.sunColor = new Color(0.8f, 0.85f, 0.9f);
            t.sunIntensity = 0.7f;
            t.sunEuler = new Vector3(48f, 140f, 0f);
            t.cityElevation = 9f;
            t.cityFacadeMaterial = cityFacade;
            t.cityGroundMaterial = cityGround;
        });
    }

    static void Wheel(Transform r, Vector3 at, float diameter, float yaw = 0f) =>
        Cyl(r, at, diameter, 0.03f, rubber, new Vector3(0f, yaw, 90f));

    
    static void BedParts(Transform r, bool rails)
    {
        Box(r, new Vector3(0f, 0.45f, 0f), new Vector3(2.05f, 0.08f, 0.92f), steel);
        Box(r, new Vector3(0.05f, 0.56f, 0f), new Vector3(1.95f, 0.14f, 0.88f), sheetBlue);
        Box(r, new Vector3(-0.8f, 0.68f, 0f), new Vector3(0.35f, 0.1f, 0.6f), labWhite);
        Box(r, new Vector3(-1.04f, 0.75f, 0f), new Vector3(0.05f, 0.65f, 0.92f), steel);
        Box(r, new Vector3(1.04f, 0.6f, 0f), new Vector3(0.05f, 0.35f, 0.92f), steel);
        foreach (float x in new[] { -0.95f, 0.95f })
        foreach (float z in new[] { -0.4f, 0.4f })
        {
            Box(r, new Vector3(x, 0.24f, z), new Vector3(0.04f, 0.38f, 0.04f), steel);
            Wheel(r, new Vector3(x, 0.05f, z), 0.1f);
        }
        if (!rails) return;
        foreach (float z in new[] { -0.47f, 0.47f }) Box(r, new Vector3(-0.3f, 0.75f, z), new Vector3(0.9f, 0.2f, 0.02f), steel);
    }

    static GameObject HospitalBed()
    {
        Transform r = Root("HospitalBed");
        BedParts(r, true);
        
        Box(r, new Vector3(0.45f, 0.65f, 0.05f), new Vector3(1.1f, 0.06f, 0.95f), sheetBlue, new Vector3(0f, 6f, 2f));
        return r.gameObject;
    }

    static GameObject Gurney()
    {
        Transform r = Root("Gurney");
        Box(r, new Vector3(0f, 0.85f, 0f), new Vector3(1.95f, 0.06f, 0.62f), steel);
        Box(r, new Vector3(0f, 0.92f, 0f), new Vector3(1.85f, 0.08f, 0.58f), labWhite);
        Box(r, new Vector3(-0.3f, 0.97f, 0.05f), new Vector3(1.1f, 0.03f, 0.6f), sheetBlue, new Vector3(0f, 0f, 3f));
        Box(r, new Vector3(0f, 0.45f, 0f), new Vector3(1.6f, 0.03f, 0.04f), steel, new Vector3(0f, 0f, 22f));
        Box(r, new Vector3(0f, 0.45f, 0f), new Vector3(1.6f, 0.03f, 0.04f), steel, new Vector3(0f, 0f, -22f));
        foreach (float x in new[] { -0.75f, 0.75f })
        foreach (float z in new[] { -0.25f, 0.25f })
            Wheel(r, new Vector3(x, 0.06f, z), 0.12f);
        return r.gameObject;
    }

    static GameObject Wheelchair()
    {
        Transform r = Root("Wheelchair");
        Box(r, new Vector3(0f, 0.5f, 0f), new Vector3(0.48f, 0.05f, 0.45f), rubber);
        Box(r, new Vector3(0f, 0.8f, -0.22f), new Vector3(0.46f, 0.5f, 0.03f), rubber, new Vector3(-8f, 0f, 0f));
        foreach (float x in new[] { -0.29f, 0.29f })
        {
            Cyl(r, new Vector3(x, 0.3f, -0.05f), 0.6f, 0.03f, steel, new Vector3(0f, 0f, 90f));
            Cyl(r, new Vector3(x, 0.3f, -0.05f), 0.54f, 0.035f, rubber, new Vector3(0f, 0f, 90f));
            Box(r, new Vector3(x - Mathf.Sign(x) * 0.04f, 0.68f, -0.02f), new Vector3(0.04f, 0.04f, 0.4f), steel);
            Wheel(r, new Vector3(x * 0.7f, 0.06f, 0.3f), 0.12f);
            Box(r, new Vector3(x * 0.7f, 0.12f, 0.33f), new Vector3(0.1f, 0.02f, 0.14f), steel);
        }
        return r.gameObject;
    }

    static GameObject CrashCart()
    {
        Transform r = Root("CrashCart");
        Box(r, new Vector3(0f, 0.55f, 0f), new Vector3(0.75f, 0.95f, 0.5f), crashRed);
        for (int i = 0; i < 4; i++)
        {
            Box(r, new Vector3(0f, 0.25f + i * 0.21f, 0.255f), new Vector3(0.7f, 0.18f, 0.01f), crashRed);
            Box(r, new Vector3(0f, 0.3f + i * 0.21f, 0.265f), new Vector3(0.3f, 0.02f, 0.02f), steel);
        }
        Box(r, new Vector3(0f, 1.04f, 0f), new Vector3(0.78f, 0.03f, 0.53f), steel);
        Box(r, new Vector3(0.15f, 1.12f, -0.05f), new Vector3(0.3f, 0.14f, 0.25f), darkPlastic);
        Box(r, new Vector3(0.15f, 1.12f, 0.076f), new Vector3(0.18f, 0.08f, 0.004f), monitorGreen);
        foreach (float x in new[] { -0.32f, 0.32f })
        foreach (float z in new[] { -0.2f, 0.2f })
            Wheel(r, new Vector3(x, 0.05f, z), 0.1f);
        return r.gameObject;
    }

    static GameObject IVStand()
    {
        Transform r = Root("IVStand");
        for (int i = 0; i < 5; i++) Box(r, new Vector3(0f, 0.04f, 0f), new Vector3(0.5f, 0.02f, 0.03f), steel, new Vector3(0f, i * 72f, 0f));
        Cyl(r, new Vector3(0f, 0.95f, 0f), 0.025f, 1.85f, steel);
        Box(r, new Vector3(0f, 1.86f, 0f), new Vector3(0.35f, 0.015f, 0.015f), steel);
        Box(r, new Vector3(0.12f, 1.72f, 0f), new Vector3(0.1f, 0.18f, 0.04f), sheetBlue);
        Cyl(r, new Vector3(0.12f, 1.25f, 0f), 0.006f, 0.8f, labWhite);
        return r.gameObject;
    }

    static GameObject WaitingBench()
    {
        Transform r = Root("WaitingBench");
        Box(r, new Vector3(0f, 0.2f, 0f), new Vector3(1.6f, 0.04f, 0.04f), steel);
        for (int i = 0; i < 3; i++)
        {
            float x = -0.52f + i * 0.52f;
            Box(r, new Vector3(x, 0.45f, 0.02f), new Vector3(0.48f, 0.05f, 0.45f), sheetBlue);
            Box(r, new Vector3(x, 0.75f, -0.2f), new Vector3(0.48f, 0.45f, 0.04f), sheetBlue, new Vector3(-8f, 0f, 0f));
        }
        foreach (float x in new[] { -0.75f, 0.75f }) Box(r, new Vector3(x, 0.22f, 0f), new Vector3(0.04f, 0.44f, 0.4f), steel);
        return r.gameObject;
    }

    static GameObject HeartMonitor()
    {
        Transform r = Root("HeartMonitor");
        for (int i = 0; i < 5; i++) Box(r, new Vector3(0f, 0.04f, 0f), new Vector3(0.45f, 0.02f, 0.03f), steel, new Vector3(0f, i * 72f, 0f));
        Cyl(r, new Vector3(0f, 0.6f, 0f), 0.035f, 1.1f, steel);
        Box(r, new Vector3(0f, 1.3f, 0f), new Vector3(0.38f, 0.3f, 0.14f), mriWhite);
        Box(r, new Vector3(-0.03f, 1.31f, 0.072f), new Vector3(0.27f, 0.2f, 0.004f), monitorGreen);
        Box(r, new Vector3(0.15f, 1.31f, 0.072f), new Vector3(0.04f, 0.2f, 0.004f), darkPlastic);
        return r.gameObject;
    }

    static GameObject PrivacyCurtain()
    {
        Transform r = Root("PrivacyCurtain");
        Box(r, new Vector3(0f, 2.3f, 0f), new Vector3(2f, 0.03f, 0.03f), steel);
        
        for (int i = 0; i < 10; i++)
            Box(r, new Vector3(-0.9f + i * 0.2f, 1.35f, (i % 2) * 0.06f), new Vector3(0.22f, 1.85f, 0.02f), curtainGreen, new Vector3(0f, (i % 2 == 0 ? 15f : -15f), 0f));
        foreach (float x in new[] { -0.98f, 0.98f }) Box(r, new Vector3(x, 1.15f, 0f), new Vector3(0.03f, 2.3f, 0.03f), steel);
        return r.gameObject;
    }

    static GameObject BedsideCabinet()
    {
        Transform r = Root("BedsideCabinet");
        Box(r, new Vector3(0f, 0.4f, 0f), new Vector3(0.45f, 0.8f, 0.45f), mriWhite);
        Box(r, new Vector3(0f, 0.6f, 0.228f), new Vector3(0.4f, 0.2f, 0.01f), labWhite);
        Box(r, new Vector3(0f, 0.65f, 0.24f), new Vector3(0.12f, 0.02f, 0.02f), steel);
        Cyl(r, new Vector3(0.1f, 0.86f, 0.05f), 0.07f, 0.12f, bottleBlue);
        Cyl(r, new Vector3(-0.1f, 0.84f, -0.05f), 0.045f, 0.08f, pillOrange);
        return r.gameObject;
    }

    
    static GameObject MriMachine()
    {
        Transform r = Root("MriMachine");
        Cyl(r, new Vector3(-0.6f, 1.15f, 0f), 2.3f, 1.7f, mriWhite, new Vector3(0f, 0f, 90f));
        Box(r, new Vector3(-0.6f, 0.4f, 0f), new Vector3(1.7f, 0.8f, 2.1f), mriWhite);
        Cyl(r, new Vector3(0.26f, 1.05f, 0f), 0.75f, 0.04f, darkPlastic, new Vector3(0f, 0f, 90f));
        Cyl(r, new Vector3(0.255f, 1.05f, 0f), 0.95f, 0.03f, steel, new Vector3(0f, 0f, 90f));
        Box(r, new Vector3(0.27f, 1.95f, 0f), new Vector3(0.02f, 0.18f, 0.6f), darkPlastic);
        Box(r, new Vector3(0.28f, 1.95f, 0f), new Vector3(0.004f, 0.12f, 0.5f), tickerBlue);
        
        Box(r, new Vector3(1.3f, 0.5f, 0f), new Vector3(2f, 0.5f, 0.6f), mriWhite);
        Box(r, new Vector3(1.1f, 0.82f, 0f), new Vector3(2.5f, 0.12f, 0.55f), sheetBlue);
        Box(r, new Vector3(2.15f, 0.92f, 0f), new Vector3(0.3f, 0.08f, 0.4f), labWhite);
        return r.gameObject;
    }

    
    static GameObject OperatingTable()
    {
        Transform r = Root("OperatingTable");
        Box(r, new Vector3(0f, 0.15f, 0f), new Vector3(0.7f, 0.3f, 0.5f), steel);
        Cyl(r, new Vector3(0f, 0.5f, 0f), 0.25f, 0.5f, steel);
        Box(r, new Vector3(0f, 0.85f, 0f), new Vector3(2f, 0.1f, 0.6f), rubber);
        Box(r, new Vector3(0.2f, 0.93f, 0f), new Vector3(1.4f, 0.05f, 0.62f), sheetBlue);
        Cyl(r, new Vector3(0f, 2.45f, 0f), 0.06f, 0.5f, steel);
        Box(r, new Vector3(0.35f, 2.2f, 0f), new Vector3(0.7f, 0.05f, 0.05f), steel, new Vector3(0f, 0f, -20f));
        Cyl(r, new Vector3(0.7f, 2.0f, 0f), 0.8f, 0.15f, mriWhite);
        Cyl(r, new Vector3(0.7f, 1.92f, 0f), 0.65f, 0.02f, surgicalLamp);
        
        Cyl(r, new Vector3(-1.4f, 0.5f, 0.6f), 0.04f, 1f, steel);
        Box(r, new Vector3(-1.4f, 1.0f, 0.6f), new Vector3(0.5f, 0.02f, 0.35f), steel);
        for (int i = 0; i < 5; i++) Box(r, new Vector3(-1.55f + i * 0.07f, 1.02f, 0.6f), new Vector3(0.015f, 0.01f, 0.18f), steel);
        return r.gameObject;
    }

    static GameObject PillBottle()
    {
        Transform r = Root("PillBottle");
        Cyl(r, new Vector3(0f, 0.025f, 0f), 0.045f, 0.08f, pillOrange, new Vector3(90f, 0f, 0f));
        Cyl(r, new Vector3(0f, 0.025f, 0.045f), 0.05f, 0.015f, labWhite, new Vector3(90f, 0f, 0f));
        return r.gameObject;
    }

    static GameObject Clipboard()
    {
        Transform r = Root("Clipboard");
        Box(r, new Vector3(0f, 0.005f, 0f), new Vector3(0.23f, 0.008f, 0.32f), cardboard);
        Box(r, new Vector3(0f, 0.011f, -0.01f), new Vector3(0.21f, 0.003f, 0.28f), paper);
        Box(r, new Vector3(0f, 0.015f, 0.14f), new Vector3(0.1f, 0.01f, 0.03f), steel);
        return r.gameObject;
    }

    static GameObject Syringe()
    {
        Transform r = Root("Syringe");
        Cyl(r, new Vector3(0f, 0.008f, 0f), 0.016f, 0.1f, labWhite, new Vector3(90f, 0f, 0f));
        Cyl(r, new Vector3(0f, 0.008f, 0.07f), 0.003f, 0.05f, steel, new Vector3(90f, 0f, 0f));
        return r.gameObject;
    }

    static GameObject GauzePad()
    {
        Transform r = Root("GauzePad");
        Box(r, new Vector3(0f, 0.004f, 0f), new Vector3(0.1f, 0.006f, 0.1f), labWhite, new Vector3(0f, 0f, 2f));
        Box(r, new Vector3(0.12f, 0.004f, 0.05f), new Vector3(0.12f, 0.006f, 0.08f), labWhite, new Vector3(0f, 30f, 0f));
        return r.gameObject;
    }
}
