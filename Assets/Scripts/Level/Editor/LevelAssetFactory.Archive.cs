using System.Collections.Generic;
using UnityEngine;




public static partial class LevelAssetFactory
{
    const string ArchiveDir = PrefabRoot + "/Archive";

    static Material shelfMetal, archiveBox, boxLabel, manila, oldWood, bulbWarm;

    static void BuildArchive()
    {
        Material walls = Mat("Archive/CinderBlockWall", new Color(0.78f, 0.75f, 0.66f), 0.1f, baseMap: cinderBlockTex, perMeter: Vector2.one / 1.6f);
        Material floor = Mat("Archive/ConcreteFloor", new Color(0.75f, 0.73f, 0.68f), 0.3f, baseMap: concreteTex, perMeter: Vector2.one / 2f);
        Material ceilingMat = Mat("Archive/ConcreteCeiling", new Color(0.4f, 0.4f, 0.38f), 0.05f, baseMap: concreteTex, perMeter: Vector2.one / 2f);
        shelfMetal = Mat("Archive/ShelfMetal", new Color(0.33f, 0.37f, 0.33f), 0.45f, metallic: 0.5f);
        archiveBox = Mat("Archive/ArchiveBox", new Color(0.55f, 0.45f, 0.3f), 0.1f);
        boxLabel = Mat("Archive/BoxLabel", new Color(0.85f, 0.83f, 0.75f), 0.1f);
        manila = Mat("Archive/Manila", new Color(0.82f, 0.68f, 0.42f), 0.1f);
        oldWood = Mat("Archive/OldWood", new Color(0.3f, 0.2f, 0.12f), 0.35f);
        bulbWarm = Mat("Archive/BulbWarm", new Color(1f, 0.85f, 0.6f), 0.5f, new Color(1f, 0.6f, 0.25f) * 5f);

        SavePrefab(TallShelf, $"{ArchiveDir}/TallShelf.prefab", PrefabKind.Prop);
        SavePrefab(FilingBank, $"{ArchiveDir}/FilingBank.prefab", PrefabKind.Prop);
        SavePrefab(CardCatalog, $"{ArchiveDir}/CardCatalog.prefab", PrefabKind.Prop);
        SavePrefab(FileCart, $"{ArchiveDir}/FileCart.prefab", PrefabKind.Prop);
        SavePrefab(Stack, $"{ArchiveDir}/Stack.prefab", PrefabKind.Prop);
        SavePrefab(ManilaFolder, $"{ArchiveDir}/ManilaFolder.prefab", PrefabKind.Clutter);
        SavePrefab(PaperPile, $"{ArchiveDir}/PaperPile.prefab", PrefabKind.Clutter);
        SavePrefab(BoxLid, $"{ArchiveDir}/BoxLid.prefab", PrefabKind.Clutter);
        SavePrefab(() => Fixture("HangingBulb", r =>
        {
            Cyl(r, new Vector3(0f, -0.02f, 0f), 0.12f, 0.03f, darkPlastic);
            Cyl(r, new Vector3(0f, -0.35f, 0f), 0.01f, 0.65f, darkPlastic);
            Cyl(r, new Vector3(0f, -0.7f, 0f), 0.05f, 0.08f, metal);
            Ball(r, new Vector3(0f, -0.78f, 0f), new Vector3(0.09f, 0.11f, 0.09f), bulbWarm);
        }, new Color(1f, 0.68f, 0.38f), 4.5f, 6.5f, -0.85f), $"{ArchiveDir}/HangingBulb.prefab", PrefabKind.Fixture);

        SaveTheme("Archive", t =>
        {
            t.displayName = "Records Archive (B2)";
            t.wallMaterial = walls;
            t.floorMaterial = floor;
            t.ceilingMaterial = ceilingMat;
            t.wallHeight = 3.2f;
            t.roomCount = 4;
            t.roomSizeRange = new Vector2Int(2, 3);
            t.startRoomSize = Vector2Int.zero;
            
            var stacks = Furnishing($"{ArchiveDir}/Stack.prefab", RoomLayout.Grid, new Vector2(4.3f, 2.7f), 1.3f);
            t.featureRooms = new List<RoomFurnishing> { stacks, stacks, stacks, stacks };
            t.wallProps = new List<PropEntry>
            {
                Entry($"{ArchiveDir}/TallShelf.prefab", 4f), Entry($"{ArchiveDir}/FilingBank.prefab", 3f),
                Entry($"{OfficeProps}/FilingCabinet.prefab", 2f, 0.15f), Entry($"{ArchiveDir}/CardCatalog.prefab", 1f),
                Entry($"{ArchiveDir}/FileCart.prefab", 1f, 0.3f), Entry($"{OfficeProps}/BoxStack.prefab", 2f, 0.2f),
            };
            t.roomWallProps = new List<PropEntry>();
            t.clutter = new List<PropEntry>
            {
                Entry($"{OfficeClutter}/PaperSheet.prefab", 3f), Entry($"{ArchiveDir}/ManilaFolder.prefab", 3f),
                Entry($"{ArchiveDir}/PaperPile.prefab", 2f), Entry($"{ArchiveDir}/BoxLid.prefab", 1f),
                Entry($"{OfficeClutter}/CrumpledPaper.prefab", 1f),
            };
            t.wallPropChance = 0.9f;
            t.maxWallPropsPerCell = 3;
            t.maxClutterPerCell = 5;
            t.lightFixture = Load<GameObject>($"{ArchiveDir}/HangingBulb.prefab");
            t.deadFixtureMaterial = fluorescentDead;
            t.lightChance = 0.15f;
            t.flickerChance = 0.45f;
            t.deadChance = 0.3f;
            t.cobwebChance = 0.35f;
            t.cobwebs = new List<GameObject>
            {
                Load<GameObject>($"{PrefabRoot}/Cobwebs/CornerWeb.prefab"), Load<GameObject>($"{PrefabRoot}/Cobwebs/CornerWebDense.prefab"),
            };
            t.ambientColor = new Color(0.016f, 0.013f, 0.009f);
            t.fogColor = new Color(0.01f, 0.008f, 0.005f);
            t.fogDensity = 0.065f;
        });
    }

    
    static void ShelfBoxes(Transform r, float x0, float x1, float y, float z, int seed)
    {
        float x = x0;
        for (int i = 0; x < x1 - 0.3f; i++)
        {
            float pull = Hash(i, seed) < 0.15f ? 0.1f : 0f;
            if (Hash(i + 50, seed) > 0.12f)
            {
                Box(r, new Vector3(x + 0.16f, y + 0.13f, z + pull), new Vector3(0.3f, 0.26f, 0.38f), archiveBox);
                Box(r, new Vector3(x + 0.16f, y + 0.15f, z + pull + 0.192f), new Vector3(0.12f, 0.06f, 0.004f), boxLabel);
            }
            x += 0.33f;
        }
    }

    
    static GameObject TallShelf()
    {
        Transform r = Root("TallShelf");
        foreach (float x in new[] { -0.88f, 0.88f })
        foreach (float z in new[] { -0.22f, 0.22f })
            Box(r, new Vector3(x, 1.2f, z), new Vector3(0.04f, 2.4f, 0.04f), shelfMetal);
        for (int s = 0; s < 5; s++)
        {
            float y = 0.08f + s * 0.55f;
            Box(r, new Vector3(0f, y, 0f), new Vector3(1.8f, 0.03f, 0.48f), shelfMetal);
            if (s < 4) ShelfBoxes(r, -0.85f, 0.85f, y + 0.015f, 0f, s * 7 + 3);
        }
        return r.gameObject;
    }

    
    static GameObject FilingBank()
    {
        Transform r = Root("FilingBank");
        for (int c = 0; c < 3; c++)
        {
            float x = -0.52f + c * 0.52f;
            Box(r, new Vector3(x, 0.78f, 0f), new Vector3(0.5f, 1.56f, 0.65f), shelfMetal);
            for (int d = 0; d < 5; d++)
            {
                float y = 0.17f + d * 0.3f;
                bool open = Hash(c, d + 20) < 0.18f;
                float z = open ? 0.55f : 0.325f;
                Box(r, new Vector3(x, y, z), new Vector3(0.44f, 0.27f, 0.02f), metal);
                Box(r, new Vector3(x, y + 0.06f, z + 0.015f), new Vector3(0.12f, 0.02f, 0.02f), darkPlastic);
                if (open)
                {
                    Box(r, new Vector3(x, y, 0.42f), new Vector3(0.44f, 0.22f, 0.25f), shelfMetal);
                    for (int f = 0; f < 6; f++) Box(r, new Vector3(x, y + 0.12f, 0.32f + f * 0.035f), new Vector3(0.4f, 0.16f, 0.01f), manila);
                }
            }
        }
        return r.gameObject;
    }

    static GameObject CardCatalog()
    {
        Transform r = Root("CardCatalog");
        Box(r, new Vector3(0f, 0.15f, 0f), new Vector3(0.95f, 0.3f, 0.5f), oldWood);
        Box(r, new Vector3(0f, 0.75f, 0f), new Vector3(0.95f, 0.9f, 0.5f), oldWood);
        for (int row = 0; row < 6; row++)
        for (int col = 0; col < 5; col++)
        {
            Vector3 p = new Vector3(-0.36f + col * 0.18f, 0.37f + row * 0.14f, 0.255f);
            Box(r, p, new Vector3(0.16f, 0.12f, 0.01f), oldWood);
            Box(r, p + new Vector3(0f, 0.025f, 0.008f), new Vector3(0.05f, 0.02f, 0.008f), brass);
        }
        return r.gameObject;
    }

    static GameObject FileCart()
    {
        Transform r = Root("FileCart");
        foreach (float y in new[] { 0.2f, 0.6f, 0.95f }) Box(r, new Vector3(0f, y, 0f), new Vector3(0.9f, 0.03f, 0.45f), shelfMetal);
        foreach (float x in new[] { -0.43f, 0.43f })
        foreach (float z in new[] { -0.2f, 0.2f })
        {
            Box(r, new Vector3(x, 0.55f, z), new Vector3(0.03f, 0.9f, 0.03f), shelfMetal);
            Ball(r, new Vector3(x, 0.05f, z), Vector3.one * 0.08f, darkPlastic);
        }
        for (int i = 0; i < 9; i++) Box(r, new Vector3(-0.38f + i * 0.09f, 0.75f, 0f), new Vector3(0.02f, 0.28f, 0.34f), i % 3 == 0 ? binderBlue : manila, new Vector3(0f, 0f, (i % 4) * 4f));
        ShelfBoxes(r, -0.45f, 0.45f, 0.215f, 0f, 9);
        return r.gameObject;
    }

    
    static GameObject Stack()
    {
        Transform r = Root("Stack");
        foreach (float x in new[] { -1.48f, 0f, 1.48f })
        foreach (float z in new[] { -0.43f, 0f, 0.43f })
            Box(r, new Vector3(x, 1.2f, z), new Vector3(0.04f, 2.4f, 0.04f), shelfMetal);
        for (int s = 0; s < 5; s++)
        {
            float y = 0.08f + s * 0.55f;
            Box(r, new Vector3(0f, y, 0f), new Vector3(3f, 0.03f, 0.9f), shelfMetal);
            if (s >= 4) continue;
            ShelfBoxes(r, -1.45f, 1.45f, y + 0.015f, 0.22f, s * 5 + 1);
            ShelfBoxes(r, -1.45f, 1.45f, y + 0.015f, -0.22f, s * 5 + 2);
        }
        return r.gameObject;
    }

    static GameObject ManilaFolder()
    {
        Transform r = Root("ManilaFolder");
        Box(r, new Vector3(0f, 0.004f, 0f), new Vector3(0.24f, 0.006f, 0.31f), manila);
        Box(r, new Vector3(0.03f, 0.009f, 0.02f), new Vector3(0.21f, 0.003f, 0.29f), paper, new Vector3(0f, 12f, 0f));
        return r.gameObject;
    }

    static GameObject PaperPile()
    {
        Transform r = Root("PaperPile");
        for (int i = 0; i < 6; i++)
            Box(r, new Vector3((Hash(i, 1) - 0.5f) * 0.3f, 0.002f + i * 0.003f, (Hash(i, 2) - 0.5f) * 0.3f),
                new Vector3(0.21f, 0.003f, 0.3f), i % 3 == 0 ? manila : paper, new Vector3(0f, Hash(i, 3) * 90f, 0f));
        return r.gameObject;
    }

    static GameObject BoxLid()
    {
        Transform r = Root("BoxLid");
        Box(r, new Vector3(0f, 0.02f, 0f), new Vector3(0.32f, 0.04f, 0.4f), archiveBox, new Vector3(0f, 0f, 4f));
        return r.gameObject;
    }
}
