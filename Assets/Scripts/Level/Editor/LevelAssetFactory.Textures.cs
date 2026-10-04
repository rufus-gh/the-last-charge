using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;



public static partial class LevelAssetFactory
{
    static Texture2D ceilingTilesTex, carpetTex, woodPanelTex, concreteTex, cinderBlockTex, hospitalTileTex,
        raisedFloorTex, serverRackTex, serverRackGlowTex, cityFacadeTex, cobwebTex, cobwebDenseTex, staticTex, sootBrickTex,
        diamondPlateTex;

    static void CreateTextures()
    {
        
        ceilingTilesTex = Tex("CeilingTiles", 256, 256, (x, y) =>
        {
            bool grout = x % 128 < 3 || y % 128 < 3;
            float v = grout ? 0.5f : 0.82f + (Hash(x, y) - 0.5f) * 0.08f;
            float stain = Mathf.Clamp01((TileNoise(x, y, 256, 256, 0.025f) - 0.62f) * 4f);
            return Color.Lerp(new Color(v, v, v * 0.97f), new Color(0.55f, 0.47f, 0.33f), stain * 0.6f);
        });

        
        carpetTex = Tex("Carpet", 256, 256, (x, y) =>
        {
            bool line = (x + y) % 64 < 2 || (x - y + 512) % 64 < 2;
            float grain = (Hash(x, y) - 0.5f) * 0.05f;
            Color c = line ? new Color(0.17f, 0.14f, 0.12f) : new Color(0.1f, 0.1f, 0.19f);
            return c * (0.85f + TileNoise(x, y, 256, 256, 0.04f) * 0.3f) + new Color(grain, grain, grain);
        });

        
        woodPanelTex = Tex("WoodPanel", 256, 256, (x, y) =>
        {
            int plank = x / 64;
            if (x % 64 < 3) return new Color(0.05f, 0.025f, 0.015f);
            float grain = Mathf.Sin(x * 0.45f + TileNoise(x, y + plank * 50, 256, 256, 0.02f) * 9f) * 0.5f + 0.5f;
            float tone = 0.85f + Hash(plank, 7) * 0.3f;
            return new Color(0.42f, 0.2f, 0.1f) * tone * (0.8f + grain * 0.25f);
        });

        
        concreteTex = Tex("Concrete", 256, 256, (x, y) =>
        {
            float v = 0.42f + TileNoise(x, y, 256, 256, 0.03f) * 0.14f + (Hash(x, y) - 0.5f) * 0.06f;
            return new Color(v, v, v * 0.98f);
        });

        
        cinderBlockTex = Tex("CinderBlock", 256, 256, (x, y) =>
        {
            int row = y / 32;
            int bx = (x + (row % 2) * 32) % 64;
            bool mortar = y % 32 < 3 || bx < 3;
            float v = mortar ? 0.55f : 0.78f + (Hash(x, y) - 0.5f) * 0.1f - TileNoise(x, y, 256, 256, 0.05f) * 0.08f;
            return new Color(v, v * 0.98f, v * 0.92f);
        });

        
        hospitalTileTex = Tex("HospitalTile", 256, 256, (x, y) =>
        {
            if (x % 64 < 2 || y % 64 < 2) return new Color(0.55f, 0.57f, 0.55f);
            float dirty = Hash(x / 64, y / 64 + 31) < 0.12f ? 0.12f : 0f;
            float v = 0.93f + (Hash(x, y) - 0.5f) * 0.04f - dirty;
            return new Color(v, v, v - dirty * 0.5f);
        });

        
        raisedFloorTex = Tex("RaisedFloor", 256, 256, (x, y) =>
        {
            int tx = x % 128, ty = y % 128;
            if (tx < 3 || ty < 3) return new Color(0.06f, 0.06f, 0.07f);
            bool vents = tx > 20 && tx < 108 && ty > 20 && ty < 108;
            bool hole = vents && (tx % 8 - 4) * (tx % 8 - 4) + (ty % 8 - 4) * (ty % 8 - 4) < 4;
            float v = hole ? 0.05f : 0.24f + (Hash(x, y) - 0.5f) * 0.04f;
            return new Color(v, v, v * 1.05f);
        });

        
        
        const int rackW = 256, rackH = 768;
        int rackTop = Mathf.RoundToInt(rackH * 2.2f / 3.5f);
        serverRackTex = Tex("ServerRack", rackW, rackH, (x, y) => RackPixel(x, y, rackTop, false));
        serverRackGlowTex = Tex("ServerRackGlow", rackW, rackH, (x, y) => RackPixel(x, y, rackTop, true));

        
        
        cityFacadeTex = Tex("CityFacade", 256, 256, (x, y) =>
        {
            if (y < 44) return new Color(0.62f, 0.62f, 0.6f) * (0.9f + Hash(x / 8, y / 8) * 0.1f);
            if (x % 64 < 5) return new Color(0.7f, 0.71f, 0.72f);
            float pane = Hash(x / 64, 3);
            float sky = Mathf.Lerp(0.35f, 0.75f, (float)y / 256f);
            Color glass = Color.Lerp(new Color(0.18f, 0.24f, 0.32f), new Color(0.55f, 0.68f, 0.8f), sky * (0.6f + pane * 0.5f));
            
            if (pane > 0.86f) glass = Color.Lerp(glass, new Color(0.85f, 0.83f, 0.75f), 0.5f);
            return glass;
        });

        
        cobwebTex = Tex("Cobweb", 256, 256, (x, y) => CobwebPixel(x, y, 256, 11, 9, 1), TextureWrapMode.Clamp, alpha: true);
        cobwebDenseTex = Tex("CobwebDense", 256, 256, (x, y) => CobwebPixel(x, y, 256, 15, 13, 2), TextureWrapMode.Clamp, alpha: true);

        
        staticTex = Tex("Static", 128, 128, (x, y) =>
        {
            float v = Hash(x * 3 + 1, y * 7 + 5);
            v = v * v * (y % 2 == 0 ? 1f : 0.65f);
            return new Color(v, v, v * 1.05f);
        }, filter: FilterMode.Point);

        
        sootBrickTex = Tex("SootBrick", 256, 256, (x, y) =>
        {
            int row = y / 16;
            int bx = (x + (row % 2) * 32) % 64;
            bool mortar = y % 16 < 2 || bx < 2;
            float n = TileNoise(x, y, 256, 256, 0.04f);
            float soot = Mathf.Clamp01(n * 1.3f - 0.25f);
            Color brick = new Color(0.42f, 0.17f, 0.11f) * (0.75f + Hash(bx / 64 + row * 13, row) * 0.4f + (Hash(x, y) - 0.5f) * 0.12f);
            Color c = mortar ? new Color(0.28f, 0.26f, 0.23f) : brick;
            return Color.Lerp(c, new Color(0.05f, 0.045f, 0.04f), soot * 0.75f);
        });

        
        diamondPlateTex = Tex("DiamondPlate", 256, 256, (x, y) =>
        {
            int cx = x % 32, cy = y % 32;
            int row = y / 32;
            
            float ax = (cx - 16f) / 10f, ay = (cy - 16f) / 3.5f;
            if (row % 2 == 1) { ax = (cx - 16f) / 3.5f; ay = (cy - 16f) / 10f; }
            bool raised = ax * ax + ay * ay < 1f;
            float v = 0.36f + (raised ? 0.14f : 0f) + (Hash(x, y) - 0.5f) * 0.05f;
            float oil = Mathf.Clamp01((TileNoise(x, y, 256, 256, 0.03f) - 0.55f) * 3f);
            v *= 1f - oil * 0.6f;
            return new Color(v, v * 0.98f, v * 0.95f);
        });
    }

    
    
    static Color CobwebPixel(int x, int y, int size, int spokes, int rings, int seed)
    {
        float u = x / (float)size, v = y / (float)size;
        
        float halfWidth = Mathf.Lerp(0.02f, 0.5f, v);
        float edge = halfWidth - Mathf.Abs(u - 0.5f);
        if (edge <= 0f) return new Color(0.85f, 0.85f, 0.82f, 0f);

        Vector2 hub = new Vector2(0.5f, 0.9f);
        Vector2 d = new Vector2(u, v) - hub;
        float r = d.magnitude;
        
        float angle = Mathf.Atan2(d.x, -d.y);
        float span = Mathf.PI * 0.95f;
        float step = span * 2f / spokes;
        float a = (angle + span) / step;
        int spoke = Mathf.FloorToInt(a);
        float frac = a - spoke;
        float px = 1f / size;

        float alpha = 0f;
        
        float spokeDistance = Mathf.Min(frac, 1f - frac) * step * r;
        if (r < 0.95f && spoke >= 0 && spoke < spokes + 1) alpha = Mathf.Max(alpha, Thread(spokeDistance, px * 1.8f) * 0.95f);

        
        for (int k = 1; k <= rings; k++)
        {
            if (Hash(k * 7 + seed, spoke * 3 + seed) < 0.13f) continue;
            float ring = 0.03f + k * (0.82f / rings) * (1f + (Hash(k, seed) - 0.5f) * 0.1f);
            float sag = ring * (1f + 0.12f * Mathf.Sin(frac * Mathf.PI));
            alpha = Mathf.Max(alpha, Thread(Mathf.Abs(r - sag), px * 1.4f) * 0.8f);
        }

        
        alpha = Mathf.Max(alpha, Mathf.Clamp01(1f - r / 0.06f) * 0.7f);
        alpha = Mathf.Max(alpha, 0.12f * Mathf.Clamp01(1f - r / 0.85f) * TileNoise(x, y, size, size, 0.06f) * 2f);
        alpha *= Mathf.Clamp01(edge * 12f) * Mathf.Clamp01((0.98f - r) * 8f);
        alpha *= 0.75f + Hash(x, y + seed) * 0.25f;
        return new Color(0.86f, 0.86f, 0.83f, Mathf.Clamp01(alpha));
    }

    static float Thread(float distance, float width) => Mathf.Clamp01(1f - distance / width);

    static Color RackPixel(int x, int y, int rackTop, bool glow)
    {
        Color black = Color.black;
        if (y >= rackTop)
        {
            
            bool tray = y > rackTop + 40 && y < rackTop + 52;
            return glow ? black : (tray ? new Color(0.22f, 0.22f, 0.2f) : new Color(0.035f, 0.035f, 0.04f));
        }
        int rx = x % 128;
        if (rx < 6 || rx > 121 || y < 10) return glow ? black : new Color(0.02f, 0.02f, 0.025f);

        
        int unit = (y - 10) / 24, uy = (y - 10) % 24, rack = x / 128;
        if (uy < 2) return glow ? black : new Color(0.03f, 0.03f, 0.035f);
        float seed = Hash(unit, rack + 3);

        if (glow)
        {
            
            for (int i = 0; i < 3; i++)
            {
                float on = Hash(unit * 3 + i, rack + 11);
                if (on < 0.25f) continue;
                int lx = 12 + i * 7, ly = 11;
                if (Mathf.Abs(rx - lx) <= 1 && Mathf.Abs(uy - ly) <= 1)
                    return on > 0.85f ? new Color(1f, 0.55f, 0.05f) : on > 0.6f ? new Color(0.15f, 0.45f, 1f) : new Color(0.1f, 1f, 0.3f);
            }
            if (seed > 0.7f && rx > 90 && rx < 112 && uy > 9 && uy < 13) return new Color(0.1f, 0.35f, 1f);
            return black;
        }

        
        float shade = 0.1f + seed * 0.08f;
        bool vent = rx > 34 && rx < 86 && (rx % 4 == 0) && (uy % 4 == 2);
        bool handle = (rx > 8 && rx < 10) || (rx > 117 && rx < 119);
        float v = vent ? shade * 0.4f : handle ? shade * 1.8f : shade;
        return new Color(v, v, v * 1.08f);
    }

    static Texture2D Tex(string name, int w, int h, Func<int, int, Color> pixel, TextureWrapMode wrapV = TextureWrapMode.Repeat,
        FilterMode filter = FilterMode.Bilinear, bool alpha = false)
    {
        string path = $"{TextureDir}/{name}.png";
        var existing = Load<Texture2D>(path);
        if (existing != null && !overwrite) return existing;

        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var pixels = new Color[w * h];
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
            pixels[y * w + x] = pixel(x, y);
        tex.SetPixels(pixels);
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.wrapModeU = TextureWrapMode.Repeat;
        importer.wrapModeV = wrapV;
        importer.filterMode = filter;
        importer.mipmapEnabled = filter != FilterMode.Point;
        importer.anisoLevel = 4;
        importer.alphaIsTransparency = alpha;
        importer.SaveAndReimport();
        return Load<Texture2D>(path);
    }

    
    static float Hash(int x, int y)
    {
        float h = Mathf.Sin(x * 127.1f + y * 311.7f) * 43758.5453f;
        return h - Mathf.Floor(h);
    }

    
    static float TileNoise(int x, int y, int w, int h, float scale)
    {
        float fx = (float)x / w, fy = (float)y / h;
        float a = Mathf.PerlinNoise(x * scale, y * scale);
        float b = Mathf.PerlinNoise((x - w) * scale, y * scale);
        float c = Mathf.PerlinNoise(x * scale, (y - h) * scale);
        float d = Mathf.PerlinNoise((x - w) * scale, (y - h) * scale);
        return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
    }
}
