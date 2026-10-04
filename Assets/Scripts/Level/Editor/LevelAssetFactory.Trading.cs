using System.Collections.Generic;
using UnityEngine;




public static partial class LevelAssetFactory
{
    const string TradingDir = PrefabRoot + "/Trading";

    static Material mahogany, leather, brass, tickerGreen, tickerRed, tickerBlue, plantGreen, terracotta, money, warmPanel;

    static void BuildTrading()
    {
        Material walls = Mat("Trading/WoodPanelWall", Color.white, 0.35f, baseMap: woodPanelTex, perMeter: Vector2.one / 1.2f);
        Material carpet = Mat("Trading/Carpet", Color.white, 0.05f, baseMap: carpetTex, perMeter: Vector2.one);
        Material tiles = Mat("Trading/CeilingTiles", new Color(0.75f, 0.72f, 0.66f), 0.05f, baseMap: ceilingTilesTex, perMeter: Vector2.one / 1.2f);
        mahogany = Mat("Trading/Mahogany", new Color(0.26f, 0.1f, 0.06f), 0.55f);
        leather = Mat("Trading/Leather", new Color(0.09f, 0.05f, 0.035f), 0.45f);
        brass = Mat("Trading/Brass", new Color(0.75f, 0.58f, 0.25f), 0.7f, metallic: 0.9f);
        tickerGreen = Mat("Trading/TickerGreen", new Color(0.02f, 0.08f, 0.03f), 0.8f, new Color(0.1f, 1f, 0.25f) * 1.6f);
        tickerRed = Mat("Trading/TickerRed", new Color(0.08f, 0.02f, 0.02f), 0.8f, new Color(1f, 0.12f, 0.08f) * 1.6f);
        tickerBlue = Mat("Trading/TickerBlue", new Color(0.02f, 0.03f, 0.08f), 0.8f, new Color(0.2f, 0.45f, 1f) * 1.4f);
        plantGreen = Mat("Trading/Plant", new Color(0.12f, 0.3f, 0.1f), 0.3f);
        terracotta = Mat("Trading/Pot", new Color(0.45f, 0.2f, 0.12f), 0.2f);
        money = Mat("Trading/Money", new Color(0.4f, 0.52f, 0.36f), 0.1f);
        warmPanel = Mat("Trading/WarmPanel", new Color(1f, 0.92f, 0.78f), 0.4f, new Color(1f, 0.82f, 0.55f) * 3.5f);

        SavePrefab(TradingPod, $"{TradingDir}/TradingPod.prefab", PrefabKind.Prop);
        SavePrefab(ExecutiveDesk, $"{TradingDir}/ExecutiveDesk.prefab", PrefabKind.Prop);
        SavePrefab(LeatherChair, $"{TradingDir}/LeatherChair.prefab", PrefabKind.Prop);
        SavePrefab(PottedPlant, $"{TradingDir}/PottedPlant.prefab", PrefabKind.Prop);
        SavePrefab(TickerCabinet, $"{TradingDir}/TickerCabinet.prefab", PrefabKind.Prop);
        SavePrefab(Boardroom, $"{TradingDir}/Boardroom.prefab", PrefabKind.Prop);
        SavePrefab(CoffeeCup, $"{TradingDir}/CoffeeCup.prefab", PrefabKind.Clutter);
        SavePrefab(MoneyStack, $"{TradingDir}/MoneyStack.prefab", PrefabKind.Clutter);
        SavePrefab(() => Fixture("WarmPanelLight", r =>
        {
            Box(r, new Vector3(0f, -0.02f, 0f), new Vector3(0.66f, 0.04f, 0.66f), metal);
            Box(r, new Vector3(0f, -0.045f, 0f), new Vector3(0.6f, 0.015f, 0.6f), warmPanel);
        }, new Color(1f, 0.8f, 0.55f), 5.5f, 7f), $"{TradingDir}/WarmPanelLight.prefab", PrefabKind.Fixture);

        SaveTheme("Trading", t =>
        {
            t.displayName = "Trading Floor (42nd floor)";
            t.wallMaterial = walls;
            t.floorMaterial = carpet;
            t.ceilingMaterial = tiles;
            t.wallHeight = 3.2f;
            t.roomCount = 3;
            t.roomSizeRange = new Vector2Int(2, 3);
            
            t.startRoomSize = new Vector2Int(6, 4);
            t.startRoomFurnishing = Furnishing($"{TradingDir}/TradingPod.prefab", RoomLayout.Grid, new Vector2(4.8f, 4.6f));
            t.featureRooms = new List<RoomFurnishing>
            {
                Furnishing($"{TradingDir}/Boardroom.prefab", RoomLayout.Center),
                Furnishing($"{TradingDir}/Boardroom.prefab", RoomLayout.Center),
            };
            t.wallProps = new List<PropEntry>
            {
                Entry($"{TradingDir}/ExecutiveDesk.prefab", 3f), Entry($"{TradingDir}/LeatherChair.prefab", 1.5f, 0.4f),
                Entry($"{TradingDir}/PottedPlant.prefab", 2f, 0.3f), Entry($"{TradingDir}/TickerCabinet.prefab", 1.5f),
                Entry($"{OfficeProps}/WaterCooler.prefab", 0.7f, 0.4f), Entry($"{OfficeProps}/FilingCabinet.prefab", 1f, 0.1f),
                Entry($"{OfficeProps}/Bookshelf.prefab", 1f, 0.1f),
            };
            t.roomWallProps = new List<PropEntry>();
            t.clutter = new List<PropEntry>
            {
                Entry($"{OfficeClutter}/PaperSheet.prefab", 3f), Entry($"{OfficeClutter}/CrumpledPaper.prefab", 2f),
                Entry($"{TradingDir}/CoffeeCup.prefab", 1.5f), Entry($"{TradingDir}/MoneyStack.prefab", 1f),
            };
            t.wallPropChance = 0.6f;
            t.maxWallPropsPerCell = 2;
            t.maxClutterPerCell = 4;
            t.lightFixture = Load<GameObject>($"{TradingDir}/WarmPanelLight.prefab");
            t.deadFixtureMaterial = fluorescentDead;
            t.lightChance = 0.24f;
            t.flickerChance = 0.25f;
            t.deadChance = 0.25f;
            
            t.ambientColor = new Color(0.08f, 0.074f, 0.066f);
            t.fogColor = new Color(0.03f, 0.022f, 0.014f);
            
            t.fogDensity = 0.0025f;
            
            t.windowChance = 0.6f;
            t.windows = new List<PropEntry> { Entry(WindowPath("Trading", WindowStyle.Intact)) };
            
            t.windowWidth = TradingWindowSize.x / 4f;
            t.windowSill = TradingSill;
            t.windowHeader = t.wallHeight - TradingSill - TradingWindowSize.y;
            t.outdoors = true;
            t.skybox = daySky;
            t.sunColor = new Color(1f, 0.95f, 0.85f);
            t.sunIntensity = 1.8f;
            t.sunEuler = new Vector3(32f, 35f, 0f);
            t.cityElevation = 110f;
            t.cityFacadeMaterial = cityFacade;
            t.cityGroundMaterial = cityGround;
        });
    }

    
    
    static GameObject TradingPod()
    {
        Transform r = Root("TradingPod");
        Box(r, new Vector3(0f, 0.74f, 0f), new Vector3(2.8f, 0.05f, 1.5f), mahogany);
        Box(r, new Vector3(0f, 0.36f, 0f), new Vector3(2.7f, 0.7f, 0.06f), mahogany);
        foreach (float x in new[] { -1.35f, 1.35f }) Box(r, new Vector3(x, 0.36f, 0f), new Vector3(0.06f, 0.72f, 1.45f), mahogany);

        Material[] screens = { tickerGreen, tickerRed, tickerBlue, tickerGreen, tickerRed, tickerGreen, tickerBlue, tickerRed };
        int s = 0;
        foreach (int side in new[] { -1, 1 })
        {
            
            for (int i = 0; i < 4; i++)
            {
                float x = -1.05f + i * 0.7f;
                for (int row = 0; row < 2; row++)
                {
                    float y = 1.0f + row * 0.36f;
                    Box(r, new Vector3(x, y, side * 0.12f), new Vector3(0.6f, 0.34f, 0.04f), darkPlastic);
                    Box(r, new Vector3(x, y, side * 0.142f), new Vector3(0.56f, 0.3f, 0.004f), screens[s++ % screens.Length]);
                }
            }
            Box(r, new Vector3(0f, 1.18f, 0f), new Vector3(2.8f, 0.85f, 0.18f), darkPlastic);
            foreach (float x in new[] { -0.7f, 0.7f })
            {
                Box(r, new Vector3(x, 0.775f, side * 0.5f), new Vector3(0.45f, 0.02f, 0.15f), darkPlastic);
                Box(r, new Vector3(x + 0.35f, 0.78f, side * 0.48f), new Vector3(0.18f, 0.06f, 0.2f), darkPlastic);
                Chair(r, new Vector3(x + (side * 0.1f), 0f, side * 1.05f), side > 0 ? 8f : 188f);
            }
            Box(r, new Vector3(-1.1f, 0.755f, side * 0.55f), new Vector3(0.21f, 0.004f, 0.3f), paper, new Vector3(0f, 20f, 0f));
        }
        return r.gameObject;
    }

    static GameObject ExecutiveDesk()
    {
        Transform r = Root("ExecutiveDesk");
        Box(r, new Vector3(0f, 0.76f, 0f), new Vector3(1.8f, 0.06f, 0.85f), mahogany);
        Box(r, new Vector3(-0.65f, 0.37f, 0f), new Vector3(0.45f, 0.74f, 0.8f), mahogany);
        Box(r, new Vector3(0.65f, 0.37f, 0f), new Vector3(0.45f, 0.74f, 0.8f), mahogany);
        Box(r, new Vector3(0f, 0.45f, -0.38f), new Vector3(0.85f, 0.55f, 0.04f), mahogany);
        for (int i = 0; i < 3; i++) Box(r, new Vector3(0.65f, 0.15f + i * 0.22f, 0.405f), new Vector3(0.12f, 0.02f, 0.02f), brass);
        
        Cyl(r, new Vector3(-0.7f, 0.8f, -0.2f), 0.14f, 0.02f, brass);
        Cyl(r, new Vector3(-0.7f, 0.95f, -0.2f), 0.02f, 0.3f, brass);
        Box(r, new Vector3(-0.7f, 1.1f, -0.12f), new Vector3(0.3f, 0.06f, 0.12f), plantGreen, new Vector3(-10f, 0f, 0f));
        Box(r, new Vector3(0.6f, 0.82f, 0.1f), new Vector3(0.2f, 0.06f, 0.18f), darkPlastic);
        foreach (float x in new[] { -0.2f, 0.25f })
        {
            Box(r, new Vector3(x, 1.03f, -0.22f), new Vector3(0.48f, 0.3f, 0.04f), darkPlastic, new Vector3(0f, x < 0 ? 10f : -10f, 0f));
            Box(r, new Vector3(x, 1.03f, -0.198f), new Vector3(0.44f, 0.26f, 0.004f), x < 0 ? tickerGreen : tickerRed, new Vector3(0f, x < 0 ? 10f : -10f, 0f));
        }
        Box(r, new Vector3(0.1f, 0.795f, 0.15f), new Vector3(0.21f, 0.004f, 0.3f), paper, new Vector3(0f, -12f, 0f));
        LeatherChairParts(r, new Vector3(0f, 0f, 0.8f), 0f);
        return r.gameObject;
    }

    
    static void LeatherChairParts(Transform parent, Vector3 at, float yaw)
    {
        Transform c = Group(parent, "LeatherChair", at, yaw);
        Cyl(c, new Vector3(0f, 0.04f, 0f), 0.62f, 0.04f, darkPlastic);
        Cyl(c, new Vector3(0f, 0.25f, 0f), 0.06f, 0.4f, brass);
        Box(c, new Vector3(0f, 0.48f, 0f), new Vector3(0.58f, 0.12f, 0.55f), leather);
        Box(c, new Vector3(0f, 0.95f, 0.26f), new Vector3(0.56f, 0.85f, 0.12f), leather, new Vector3(-8f, 0f, 0f));
        foreach (float x in new[] { -0.3f, 0.3f }) Box(c, new Vector3(x, 0.65f, 0.02f), new Vector3(0.08f, 0.06f, 0.48f), leather);
    }

    static GameObject LeatherChair()
    {
        Transform r = Root("LeatherChair");
        LeatherChairParts(r, Vector3.zero, 180f);
        return r.gameObject;
    }

    static GameObject PottedPlant()
    {
        Transform r = Root("PottedPlant");
        Cyl(r, new Vector3(0f, 0.25f, 0f), 0.45f, 0.5f, terracotta);
        Cyl(r, new Vector3(0f, 0.6f, 0f), 0.05f, 0.4f, wood);
        Ball(r, new Vector3(0f, 1.1f, 0f), new Vector3(0.7f, 0.8f, 0.7f), plantGreen);
        Ball(r, new Vector3(0.15f, 1.4f, 0.05f), new Vector3(0.45f, 0.5f, 0.45f), plantGreen);
        Ball(r, new Vector3(-0.12f, 0.9f, 0.1f), new Vector3(0.5f, 0.45f, 0.5f), plantGreen);
        return r.gameObject;
    }

    
    static GameObject TickerCabinet()
    {
        Transform r = Root("TickerCabinet");
        Box(r, new Vector3(0f, 0.4f, 0f), new Vector3(1.6f, 0.8f, 0.5f), mahogany);
        for (int i = 0; i < 3; i++) Box(r, new Vector3(-0.5f + i * 0.5f, 0.6f, 0.255f), new Vector3(0.1f, 0.02f, 0.02f), brass);
        Material[] screens = { tickerRed, tickerGreen, tickerBlue, tickerGreen, tickerRed, tickerGreen };
        for (int i = 0; i < 6; i++)
        {
            float x = -0.53f + (i % 3) * 0.53f, y = 1.1f + (i / 3) * 0.32f;
            Box(r, new Vector3(x, y, -0.1f), new Vector3(0.52f, 0.31f, 0.05f), darkPlastic);
            Box(r, new Vector3(x, y, -0.073f), new Vector3(0.48f, 0.27f, 0.004f), screens[i]);
        }
        return r.gameObject;
    }

    
    static GameObject Boardroom()
    {
        Transform r = Root("Boardroom");
        Box(r, new Vector3(0f, 0.76f, 0f), new Vector3(4.6f, 0.07f, 1.4f), mahogany);
        Box(r, new Vector3(-1.4f, 0.37f, 0f), new Vector3(0.5f, 0.74f, 0.8f), mahogany);
        Box(r, new Vector3(1.4f, 0.37f, 0f), new Vector3(0.5f, 0.74f, 0.8f), mahogany);
        for (int i = 0; i < 4; i++)
        {
            float x = -1.5f + i * 1f;
            LeatherChairParts(r, new Vector3(x, 0f, 1.05f), 0f);
            LeatherChairParts(r, new Vector3(x + 0.15f, 0f, -1.05f), 180f);
            Box(r, new Vector3(x, 0.8f, 0.4f), new Vector3(0.21f, 0.004f, 0.3f), paper, new Vector3(0f, i * 9f, 0f));
        }
        LeatherChairParts(r, new Vector3(-2.65f, 0f, 0f), 90f);
        LeatherChairParts(r, new Vector3(2.65f, 0f, 0f), -90f);
        
        Cyl(r, new Vector3(0.3f, 0.9f, 0f), 0.12f, 0.22f, bottleBlue);
        for (int i = 0; i < 3; i++) Cyl(r, new Vector3(0.55f + i * 0.12f, 0.84f, 0.1f), 0.06f, 0.09f, bottleBlue);
        return r.gameObject;
    }

    static GameObject CoffeeCup()
    {
        Transform r = Root("CoffeeCup");
        Cyl(r, new Vector3(0f, 0.045f, 0f), 0.08f, 0.09f, labWhite);
        Cyl(r, new Vector3(0f, 0.092f, 0f), 0.085f, 0.008f, leather);
        return r.gameObject;
    }

    static GameObject MoneyStack()
    {
        Transform r = Root("MoneyStack");
        Box(r, new Vector3(0f, 0.02f, 0f), new Vector3(0.16f, 0.04f, 0.07f), money);
        Box(r, new Vector3(0f, 0.02f, 0f), new Vector3(0.03f, 0.042f, 0.072f), paper);
        Box(r, new Vector3(0.05f, 0.05f, 0.03f), new Vector3(0.16f, 0.025f, 0.07f), money, new Vector3(0f, 25f, 0f));
        return r.gameObject;
    }
}
