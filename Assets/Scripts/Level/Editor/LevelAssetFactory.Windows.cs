using UnityEngine;






public static partial class LevelAssetFactory
{
    const string WindowDir = PrefabRoot + "/Windows";

    static Material daySky, overcastSky, cityFacade, cityGround;

    enum WindowStyle { Intact, Boarded, Broken }

    
    static readonly Vector2 TradingWindowSize = new Vector2(2.6f, 2.0f);
    const float TradingSill = 0.55f;
    static readonly Vector2 HospitalWindowSize = new Vector2(1.6f, 1.4f);
    const float HospitalSill = 1.0f;

    static string WindowPath(string theme, WindowStyle style) => $"{WindowDir}/{theme}Window{style}.prefab";

    
    static void BuildWindowAssets()
    {
        daySky = SkyMat("Sky/DaySky", new Color(0.45f, 0.58f, 0.78f), new Color(0.37f, 0.35f, 0.33f), 0.9f, 1.25f, 0.035f);
        overcastSky = SkyMat("Sky/OvercastSky", new Color(0.55f, 0.58f, 0.6f), new Color(0.3f, 0.3f, 0.3f), 1.7f, 0.95f, 0.02f);
        
        
        cityFacade = Mat("Windows/CityFacade", Color.white, 0.5f, new Color(0.32f, 0.34f, 0.36f), baseMap: cityFacadeTex, perMeter: Vector2.one / 4f,
            emissionMap: cityFacadeTex);
        cityGround = Mat("Windows/CityGround", new Color(0.35f, 0.35f, 0.34f), 0.2f, baseMap: concreteTex, perMeter: Vector2.one / 6f);
    }

    static void BuildCobwebs()
    {
        Material web = TransparentMat("Cobweb", new Color(0.8f, 0.8f, 0.78f, 0.85f), 0f, baseMap: cobwebTex, doubleSided: true);
        Material webDense = TransparentMat("CobwebDense", new Color(0.78f, 0.78f, 0.75f, 0.9f), 0f, baseMap: cobwebDenseTex, doubleSided: true);
        SavePrefab(() => CornerWeb("CornerWeb", web, 1f), $"{PrefabRoot}/Cobwebs/CornerWeb.prefab", PrefabKind.Clutter);
        SavePrefab(() => CornerWeb("CornerWebDense", webDense, 1.25f), $"{PrefabRoot}/Cobwebs/CornerWebDense.prefab", PrefabKind.Clutter);
    }

    
    
    
    static GameObject CornerWeb(string name, Material mat, float size)
    {
        Transform r = Root(name);
        Part(r, PrimitiveType.Quad, new Vector3(0f, -0.33f * size, 0.42f * size), new Vector3(1f, 0.7f, 1f) * size, mat, new Vector3(25f, 0f, 0f));
        
        Part(r, PrimitiveType.Quad, new Vector3(0f, -0.05f * size, 0.22f * size), new Vector3(0.6f, 0.45f, 1f) * size, mat, new Vector3(78f, 0f, 0f));
        return r.gameObject;
    }

    
    
    static void BuildWindowPrefabs()
    {
        Material glass = TransparentMat("Windows/Glass", new Color(0.7f, 0.85f, 0.95f, 0.12f), 0.95f);
        Material grimyGlass = TransparentMat("Windows/GrimyGlass", new Color(0.55f, 0.6f, 0.55f, 0.3f), 0.8f);
        Material shard = TransparentMat("Windows/Shard", new Color(0.7f, 0.78f, 0.75f, 0.18f), 0.9f);
        Material bronze = Mat("Windows/BronzeFrame", new Color(0.16f, 0.13f, 0.1f), 0.55f, metallic: 0.7f);
        Material paintedFrame = Mat("Windows/PaintedFrame", new Color(0.78f, 0.79f, 0.76f), 0.35f);
        Material sill = Mat("Windows/StoneSill", new Color(0.62f, 0.6f, 0.56f), 0.4f);
        Material plank = Mat("Windows/Plank", new Color(0.42f, 0.32f, 0.22f), 0.15f);
        Material nail = Mat("Windows/Nail", new Color(0.3f, 0.3f, 0.3f), 0.5f, metallic: 0.8f);

        var daylight = new Color(1f, 0.95f, 0.85f);
        var overcast = new Color(0.82f, 0.85f, 0.9f);
        SavePrefab(() => TradingWindow(bronze, glass, sill, daylight), WindowPath("Trading", WindowStyle.Intact), PrefabKind.Decor);
        SavePrefab(() => HospitalWindow("HospitalWindowIntact", paintedFrame, grimyGlass, sill, plank, nail, overcast, WindowStyle.Intact),
            WindowPath("Hospital", WindowStyle.Intact), PrefabKind.Decor);
        SavePrefab(() => HospitalWindow("HospitalWindowBroken", paintedFrame, shard, sill, plank, nail, overcast, WindowStyle.Broken),
            WindowPath("Hospital", WindowStyle.Broken), PrefabKind.Decor);
        SavePrefab(() => HospitalWindow("HospitalWindowBoarded", paintedFrame, grimyGlass, sill, plank, nail, overcast, WindowStyle.Boarded),
            WindowPath("Hospital", WindowStyle.Boarded), PrefabKind.Decor);
    }

    static void Frame(Transform r, Vector2 size, float depth, float bar, Material frame)
    {
        Box(r, new Vector3(0f, bar / 2f, 0f), new Vector3(size.x, bar, depth), frame);
        Box(r, new Vector3(0f, size.y - bar / 2f, 0f), new Vector3(size.x, bar, depth), frame);
        Box(r, new Vector3(-size.x / 2f + bar / 2f, size.y / 2f, 0f), new Vector3(bar, size.y, depth), frame);
        Box(r, new Vector3(size.x / 2f - bar / 2f, size.y / 2f, 0f), new Vector3(bar, size.y, depth), frame);
    }

    static void Barrier(Transform r, Vector2 size)
    {
        
        var box = r.gameObject.AddComponent<BoxCollider>();
        box.center = new Vector3(0f, size.y / 2f, 0f);
        box.size = new Vector3(size.x, size.y, 0.2f);
    }

    
    static void WindowLight(Transform r, Vector2 size, Color color, float intensity, float range = 8f)
    {
        SpotLight(Live(r), "Daylight", new Vector3(0f, size.y * 0.9f, -0.25f), new Vector3(28f, 0f, 0f), color, intensity, range, 125f, 60f);
    }

    static GameObject TradingWindow(Material frame, Material glass, Material sill, Color light)
    {
        Transform r = Root("TradingWindowIntact");
        Vector2 size = TradingWindowSize;
        Frame(r, size, 0.16f, 0.07f, frame);
        Box(r, new Vector3(0f, size.y / 2f, 0f), new Vector3(0.06f, size.y, 0.14f), frame);
        Box(r, new Vector3(0f, size.y / 2f, -0.02f), new Vector3(size.x - 0.1f, size.y - 0.1f, 0.012f), glass);
        
        Box(r, new Vector3(0f, 0.015f, 0.17f), new Vector3(size.x + 0.1f, 0.03f, 0.26f), sill);
        Barrier(r, size);
        WindowLight(r, size, light, 6f, 9f);
        return r.gameObject;
    }

    static GameObject HospitalWindow(string name, Material frame, Material glass, Material sill, Material plank, Material nail, Color light, WindowStyle style)
    {
        Transform r = Root(name);
        Vector2 size = HospitalWindowSize;
        Frame(r, size, 0.14f, 0.06f, frame);
        
        Box(r, new Vector3(0f, size.y / 2f, 0f), new Vector3(0.05f, size.y, 0.12f), frame);
        Box(r, new Vector3(0f, size.y / 2f, 0f), new Vector3(size.x, 0.05f, 0.12f), frame);
        Box(r, new Vector3(0f, 0.015f, 0.14f), new Vector3(size.x + 0.08f, 0.03f, 0.2f), sill);

        float paneW = size.x / 2f - 0.06f, paneH = size.y / 2f - 0.06f;
        var panes = new[]
        {
            new Vector3(-size.x / 4f, size.y * 0.25f, -0.02f), new Vector3(size.x / 4f, size.y * 0.25f, -0.02f),
            new Vector3(-size.x / 4f, size.y * 0.75f, -0.02f), new Vector3(size.x / 4f, size.y * 0.75f, -0.02f),
        };

        if (style == WindowStyle.Broken)
        {
            
            for (int p = 0; p < panes.Length; p++)
            {
                Vector3 c = panes[p];
                for (int i = 0; i < 7; i++)
                {
                    float h = Hash(p * 7 + i, 13), k = Hash(p * 7 + i, 17);
                    int side = i % 4;
                    
                    Vector3 inward = side == 0 ? Vector3.up : side == 1 ? Vector3.down : side == 2 ? Vector3.right : Vector3.left;
                    Vector3 along = side < 2 ? Vector3.right : Vector3.up;
                    float span = side < 2 ? paneW : paneH;
                    float reach = (side < 2 ? paneH : paneW) * (0.2f + k * 0.45f);
                    Vector3 root = c - inward * ((side < 2 ? paneH : paneW) / 2f) + along * (h - 0.5f) * span * 0.8f;
                    float lean = (h - 0.5f) * 40f;
                    float width = 0.03f + h * 0.05f;
                    Box(r, root + inward * reach / 2f, side < 2 ? new Vector3(width, reach, 0.008f) : new Vector3(reach, width, 0.008f), glass,
                        new Vector3(0f, 0f, lean));
                }
            }
            for (int i = 0; i < 9; i++)
            {
                float a = Hash(i, 41), b = Hash(i, 42);
                Box(r, new Vector3((a - 0.5f) * 1.6f, -HospitalSill + 0.004f, 0.25f + b * 0.9f), new Vector3(0.06f + a * 0.1f, 0.006f, 0.04f + b * 0.08f), glass,
                    new Vector3(0f, a * 180f, 0f));
            }
            WindowLight(r, size, light, 3.5f);
        }
        else
        {
            foreach (Vector3 c in panes) Box(r, c, new Vector3(paneW, paneH, 0.01f), glass);
        }

        if (style == WindowStyle.Boarded)
        {
            
            float[] heights = { 0.18f, 0.5f, 0.86f, 1.2f };
            for (int i = 0; i < heights.Length; i++)
            {
                float tilt = (Hash(i, 61) - 0.5f) * 14f;
                float w = size.x + 0.35f + Hash(i, 62) * 0.2f;
                Box(r, new Vector3((Hash(i, 63) - 0.5f) * 0.1f, heights[i], 0.11f), new Vector3(w, 0.22f, 0.03f), plank, new Vector3(0f, 0f, tilt));
                foreach (float x in new[] { -w / 2f + 0.08f, w / 2f - 0.08f })
                {
                    float y = heights[i] + Mathf.Tan(tilt * Mathf.Deg2Rad) * x;
                    Box(r, new Vector3(x, y, 0.128f), new Vector3(0.015f, 0.015f, 0.01f), nail);
                }
            }
            WindowLight(r, size, light, 2.2f);
        }
        else if (style == WindowStyle.Intact)
        {
            WindowLight(r, size, light, 3.5f);
        }

        Barrier(r, size);
        return r.gameObject;
    }
}
