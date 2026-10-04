using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;





public class MenuMazeBackground : MonoBehaviour
{
    [Tooltip("Level loaded additively for the backdrop (the medical floor)")]
    [SerializeField] string backdropScene = "Level_02";
    [SerializeField] float eyeHeight = 1.7f;
    [Tooltip("Metres per second the camera drifts along the maze")]
    [SerializeField] float panSpeed = 1.3f;
    [Tooltip("How far ahead along the path the camera looks")]
    [SerializeField] float lookAhead = 4.5f;
    [Tooltip("Lower = softer blur (fraction of screen resolution the backdrop renders at)")]
    [Range(0.2f, 1f)] [SerializeField] float blurScale = 0.5f;
    [SerializeField] float fieldOfView = 58f;
    [SerializeField] float flashlightIntensity = 6f;

    Camera cam;
    RenderTexture rt;
    RawImage image;
    readonly List<Vector3> path = new List<Vector3>();
    readonly List<float> cumulative = new List<float>();
    float totalLength;
    float travelled;
    int direction = 1;
    bool ready;
    float bobSeed;

    void Awake()
    {
        bobSeed = Mathf.Repeat(Time.realtimeSinceStartup, 100f);
        cam = Camera.main;
        if (cam == null)
        {
            var go = new GameObject("Menu Camera", typeof(Camera), typeof(AudioListener));
            go.tag = "MainCamera";
            cam = go.GetComponent<Camera>();
        }
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.fieldOfView = fieldOfView;
        cam.nearClipPlane = 0.15f;
        AttachFlashlight();
        AddScreenCamera();

        BuildCanvas();
        EnsureRenderTexture();

        SceneManager.LoadSceneAsync(backdropScene, LoadSceneMode.Additive).completed += OnBackdropLoaded;
    }

    
    
    void AttachFlashlight()
    {
        var go = new GameObject("Menu Flashlight", typeof(Light));
        go.transform.SetParent(cam.transform, false);
        go.transform.localPosition = new Vector3(0.15f, -0.1f, 0f);
        var light = go.GetComponent<Light>();
        light.type = LightType.Spot;
        light.color = new Color(1f, 0.95f, 0.85f);
        light.intensity = flashlightIntensity;
        light.range = 30f;
        light.spotAngle = 70f;
        light.innerSpotAngle = 35f;
        light.shadows = LightShadows.None;
    }

    
    
    static void AddScreenCamera()
    {
        var go = new GameObject("Menu Screen Camera", typeof(Camera));
        var screen = go.GetComponent<Camera>();
        screen.clearFlags = CameraClearFlags.SolidColor;
        screen.backgroundColor = Color.black;
        screen.cullingMask = 0;
        screen.depth = -100f;
    }

    void OnBackdropLoaded(AsyncOperation _)
    {
        Scene scene = SceneManager.GetSceneByName(backdropScene);
        if (!scene.IsValid()) return;
        SceneManager.SetActiveScene(scene);
        StripGameplay(scene);
        BuildPath(scene);

        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ready = true;
    }

    
    
    static void StripGameplay(Scene scene)
    {
        var player = FindAnyObjectByType<PlayerHealth>(FindObjectsInactive.Include);
        if (player != null && player.gameObject.scene == scene)
            player.transform.root.gameObject.SetActive(false);

        foreach (var listener in FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (listener.gameObject.scene == scene) listener.enabled = false;

        foreach (var es in FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (es.gameObject.scene == scene) es.gameObject.SetActive(false);

        foreach (var spawner in FindObjectsByType<ZombieSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (spawner.gameObject.scene == scene) spawner.gameObject.SetActive(false);
    }

    void BuildPath(Scene scene)
    {
        MazeGenerator maze = null;
        foreach (var m in FindObjectsByType<MazeGenerator>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (m.gameObject.scene == scene) { maze = m; break; }

        path.Clear();
        cumulative.Clear();
        totalLength = 0f;

        if (maze != null && maze.SolutionPath != null)
            foreach (Vector3 p in maze.SolutionPath)
                path.Add(new Vector3(p.x, eyeHeight, p.z));

        
        if (path.Count < 2)
        {
            Vector3 origin = maze != null && maze.SpawnPoint != null
                ? maze.SpawnPoint.position
                : (cam != null ? cam.transform.position : Vector3.zero);
            origin.y = eyeHeight;
            path.Clear();
            path.Add(origin);
            cumulative.Add(0f);
            return;
        }

        cumulative.Add(0f);
        for (int i = 1; i < path.Count; i++)
        {
            totalLength += Vector3.Distance(path[i - 1], path[i]);
            cumulative.Add(totalLength);
        }
        travelled = 0f;
        direction = 1;
    }

    void Update()
    {
        EnsureRenderTexture();
        if (!ready || cam == null || path.Count == 0) return;

        if (path.Count < 2)
        {
            
            cam.transform.position = path[0] + Vector3.up * Mathf.Sin((Time.time + bobSeed) * 0.6f) * 0.05f;
            cam.transform.rotation = Quaternion.Euler(4f, (Time.time + bobSeed) * 6f, 0f);
            return;
        }

        travelled += panSpeed * direction * Time.deltaTime;
        if (travelled >= totalLength) { travelled = totalLength; direction = -1; }
        else if (travelled <= 0f) { travelled = 0f; direction = 1; }

        Vector3 pos = SamplePath(travelled);
        Vector3 look = SamplePath(travelled + lookAhead * direction);
        pos.y += Mathf.Sin((Time.time + bobSeed) * 0.7f) * 0.06f;

        cam.transform.position = pos;
        Vector3 forward = look - pos;
        if (forward.sqrMagnitude > 0.0001f)
        {
            Quaternion target = Quaternion.LookRotation(forward);
            
            target *= Quaternion.Euler(Mathf.Sin((Time.time + bobSeed) * 0.5f) * 1.5f, Mathf.Sin((Time.time + bobSeed) * 0.37f) * 2f, 0f);
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, target, 1f - Mathf.Exp(-2f * Time.deltaTime));
        }
    }

    Vector3 SamplePath(float distance)
    {
        distance = Mathf.Clamp(distance, 0f, totalLength);
        for (int i = 1; i < cumulative.Count; i++)
        {
            if (distance <= cumulative[i])
            {
                float segment = cumulative[i] - cumulative[i - 1];
                float t = segment > 0.0001f ? (distance - cumulative[i - 1]) / segment : 0f;
                return Vector3.Lerp(path[i - 1], path[i], t);
            }
        }
        return path[path.Count - 1];
    }

    void EnsureRenderTexture()
    {
        int w = Mathf.Max(16, Mathf.RoundToInt(Screen.width * blurScale));
        int h = Mathf.Max(16, Mathf.RoundToInt(Screen.height * blurScale));
        if (rt != null && rt.width == w && rt.height == h) return;

        if (rt != null) { if (cam != null) cam.targetTexture = null; rt.Release(); Destroy(rt); }
        rt = new RenderTexture(w, h, 24) { filterMode = FilterMode.Bilinear };
        rt.Create();
        if (cam != null) cam.targetTexture = rt;
        if (image != null) image.texture = rt;
    }

    void BuildCanvas()
    {
        var canvasGo = new GameObject("Menu Background", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -10; 
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        image = NewImage<RawImage>(canvasGo.transform, "Backdrop");
        image.color = new Color(0.9f, 0.92f, 0.96f); 
        image.texture = rt;

        
        var dark = NewImage<Image>(canvasGo.transform, "Darken");
        dark.color = new Color(0f, 0f, 0f, 0.28f);

        
        var vignette = NewImage<Image>(canvasGo.transform, "Vignette");
        vignette.sprite = Sprite.Create(VignetteTexture(), new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
        vignette.color = new Color(0f, 0f, 0f, 0.8f);
        vignette.type = Image.Type.Simple;
    }

    static T NewImage<T>(Transform parent, string name) where T : Graphic
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(T));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var graphic = go.GetComponent<T>();
        graphic.raycastTarget = false;
        return graphic;
    }

    static Texture2D VignetteTexture()
    {
        const int size = 256;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        var pixels = new Color[size * size];
        Vector2 centre = new Vector2(size / 2f, size / 2f);
        float maxDist = centre.magnitude;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), centre) / maxDist;
                float a = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((d - 0.45f) / 0.55f));
                pixels[y * size + x] = new Color(1f, 1f, 1f, a);
            }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    void OnDestroy()
    {
        if (cam != null) cam.targetTexture = null;
        if (rt != null) { rt.Release(); Destroy(rt); }
    }
}
