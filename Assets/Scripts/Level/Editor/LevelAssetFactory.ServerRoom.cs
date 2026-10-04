using System.Collections.Generic;
using UnityEngine;




public static partial class LevelAssetFactory
{
    const string ServerDir = PrefabRoot + "/ServerRoom";

    static Material stripBlue;

    static void BuildServerRoom()
    {
        Material walls = Mat("ServerRoom/RackWall", Color.white, 0.5f, Color.white * 2.2f, 0.3f,
            serverRackTex, new Vector2(1f / 1.2f, 1f / 3.5f), serverRackGlowTex);
        Material floor = Mat("ServerRoom/RaisedFloor", Color.white, 0.35f, baseMap: raisedFloorTex, perMeter: Vector2.one / 1.2f);
        Material ceilingMat = Mat("ServerRoom/Ceiling", new Color(0.06f, 0.06f, 0.07f), 0.1f);
        stripBlue = Mat("ServerRoom/StripBlue", new Color(0.7f, 0.85f, 1f), 0.4f, new Color(0.45f, 0.7f, 1f) * 4f);

        Material[] cableColors =
        {
            Mat("ServerRoom/CableBlack", new Color(0.04f, 0.04f, 0.045f), 0.4f),
            Mat("ServerRoom/CableBlue", new Color(0.08f, 0.2f, 0.55f), 0.4f),
            Mat("ServerRoom/CableYellow", new Color(0.7f, 0.6f, 0.08f), 0.4f),
            Mat("ServerRoom/CableOrange", new Color(0.75f, 0.3f, 0.05f), 0.4f),
            Mat("ServerRoom/CableGrey", new Color(0.4f, 0.42f, 0.44f), 0.4f),
        };

        var clutter = new List<string>();
        for (int i = 0; i < 6; i++)
        {
            int variant = i;
            string path = $"{ServerDir}/CableBundle{i + 1}.prefab";
            SavePrefab(() => CableBundle(variant, cableColors), path, PrefabKind.Clutter);
            clutter.Add(path);
        }
        SavePrefab(() => Fixture("BlueStripLight", r =>
        {
            Box(r, new Vector3(0f, -0.02f, 0f), new Vector3(1.7f, 0.04f, 0.1f), metal);
            Box(r, new Vector3(0f, -0.045f, 0f), new Vector3(1.6f, 0.02f, 0.05f), stripBlue);
        }, new Color(0.55f, 0.75f, 1f), 4.5f, 7.5f), $"{ServerDir}/BlueStripLight.prefab", PrefabKind.Fixture);

        SaveTheme("ServerRoom", t =>
        {
            t.displayName = "Server Room (B1)";
            t.wallMaterial = walls;
            t.floorMaterial = floor;
            t.ceilingMaterial = ceilingMat;
            t.wallHeight = 3.5f;
            t.roomCount = 4;
            t.roomSizeRange = new Vector2Int(2, 3);
            t.startRoomSize = Vector2Int.zero;
            t.featureRooms = new List<RoomFurnishing>();
            
            t.wallProps = new List<PropEntry>();
            t.roomWallProps = new List<PropEntry>();
            t.clutter = clutter.ConvertAll(p => Entry(p));
            t.wallPropChance = 0f;
            t.maxWallPropsPerCell = 0;
            t.maxClutterPerCell = 4;
            t.lightFixture = Load<GameObject>($"{ServerDir}/BlueStripLight.prefab");
            t.deadFixtureMaterial = fluorescentDead;
            t.lightChance = 0.17f;
            t.flickerChance = 0.3f;
            t.deadChance = 0.3f;
            t.ambientColor = new Color(0.016f, 0.02f, 0.032f);
            t.fogColor = new Color(0.008f, 0.012f, 0.025f);
            t.fogDensity = 0.07f;
        });
    }

    
    static GameObject CableBundle(int variant, Material[] colors)
    {
        Transform r = Root($"CableBundle{variant + 1}");
        int cables = 2 + variant % 3;
        for (int c = 0; c < cables; c++)
        {
            Material mat = colors[(variant * 2 + c) % colors.Length];
            float thickness = 0.022f + Hash(variant, c) * 0.02f;
            Vector3 p = new Vector3(-0.9f, thickness / 2f, (c - cables / 2f) * 0.06f);
            float heading = (Hash(variant + 10, c) - 0.5f) * 30f;
            int segments = 8 + variant % 4;
            for (int s = 0; s < segments; s++)
            {
                
                heading += (Hash(variant * 31 + s, 5) - 0.5f) * 50f + (Hash(c, s + 40) - 0.5f) * 12f;
                float length = 0.22f + Hash(s, variant + c) * 0.1f;
                Vector3 dir = Quaternion.Euler(0f, heading, 0f) * Vector3.right;
                Vector3 next = p + dir * length;
                Cyl(r, (p + next) / 2f, thickness, length + thickness, mat, new Vector3(0f, heading, 90f));
                p = next;
            }
        }
        return r.gameObject;
    }
}
