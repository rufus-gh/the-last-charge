using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;




public static class PropModelPreview
{
    public static void Run()
    {
        string dir = ArgAfter("-previewDir") ?? "Temp/PropPreviews";
        Directory.CreateDirectory(dir);
        PropModels.ApplyImportSettings();
        foreach (string model in PropModels.AllModelPaths())
        {
            GameObject go = PropModels.Instantiate(model);
            if (go == null) { Debug.LogWarning("Preview: could not load " + model); continue; }
            Bounds b = PropModels.Bounds(go);
            RenderSheet(go, Path.Combine(dir, Path.GetFileNameWithoutExtension(model) + ".png"));
            Debug.Log($"PREVIEW {go.name}: size {b.size.x:0.###} x {b.size.y:0.###} x {b.size.z:0.###} renderers {go.GetComponentsInChildren<Renderer>().Length} " +
                $"materials [{string.Join(", ", go.GetComponentsInChildren<Renderer>().SelectMany(r => r.sharedMaterials).Where(m => m != null).Select(m => m.name).Distinct())}]");
            Object.DestroyImmediate(go);
        }
        Debug.Log("PREVIEW DONE");
    }

    static Camera cam;
    static RenderTexture rt;

    
    public static void RenderSheet(GameObject go, string file, int size = 384)
    {
        if (cam == null) SetUpStage(size);
        Bounds b = PropModels.Bounds(go);
        var sheet = new Texture2D(size * 3, size, TextureFormat.RGB24, false);
        Vector3[] dirs = { Vector3.forward, Vector3.right, Vector3.up };
        float extent = Mathf.Max(b.size.x, b.size.y, b.size.z) * 0.6f + 0.05f;
        for (int i = 0; i < 3; i++)
        {
            cam.orthographicSize = extent;
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = extent * 10f;
            cam.transform.position = b.center + dirs[i] * extent * 4f;
            cam.transform.rotation = Quaternion.LookRotation(-dirs[i], i == 2 ? Vector3.forward : Vector3.up);
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(size, size, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            tex.Apply();
            sheet.SetPixels(i * size, 0, size, size, tex.GetPixels());
            Object.DestroyImmediate(tex);
        }
        RenderTexture.active = null;
        sheet.Apply();
        File.WriteAllBytes(file, sheet.EncodeToPNG());
        Object.DestroyImmediate(sheet);
    }

    static void SetUpStage(int size)
    {
        var lightGo = new GameObject("Preview Light", typeof(Light));
        var sun = lightGo.GetComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.3f;
        lightGo.transform.rotation = Quaternion.Euler(40f, 150f, 0f);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.6f);
        var camGo = new GameObject("Preview Camera", typeof(Camera));
        cam = camGo.GetComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.82f, 0.84f, 0.88f);
        cam.cullingMask = ~0;
        camGo.AddComponent<UniversalAdditionalCameraData>();
        rt = new RenderTexture(size, size, 24);
        cam.targetTexture = rt;
    }

    public static string ArgAfter(string flag)
    {
        string[] args = System.Environment.GetCommandLineArgs();
        int i = System.Array.IndexOf(args, flag);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }
}
