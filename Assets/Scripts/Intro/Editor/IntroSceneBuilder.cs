using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;





public static class IntroSceneBuilder
{
    const string ScenePath = "Assets/Scenes/Intro.unity";
    const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    const string PlayerRigPath = "Assets/Prefabs/Player Rig.prefab";
    const string MaterialRoot = "Assets/Materials/Level";
    const string FirstLevel = "Level_01";

    
    
    const float RoofHalfX = 20f;
    const float RoofHalfZ = 8f;
    const float StreetY = -92f;
    
    static readonly Vector3 Landing = new Vector3(12.4f, 1.5f, 0.5f);
    
    static readonly Vector3 HatchAt = new Vector3(-17.5f, 0f, -4.5f);
    const float HatchHalf = 1f;

    
    
    
    const float StreetLine = 42.9f;
    
    const float ModelRoadX = -132f;
    static readonly Vector2 ModelRoofCentre = new Vector2(-89.1f, -270.5f);
    const float StripLength = 285f;

    [MenuItem("Losing Power/Build Intro Cutscene")]
    static void BuildMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Build();
        EditorUtility.DisplayDialog("Intro cutscene built",
            "Assets/Scenes/Intro.unity is the new first scene of a run: the helicopter drop onto the tower roof.\n\n" +
            "Open MainMenu and press Play, or press Play in the Intro scene itself. Space skips the cutscene.", "OK");
    }

    public static void Build()
    {
        LevelAssetFactory.EnsureAssets();
        AssetDatabase.Refresh();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        BuildAtmosphere();
        BuildCity();
        Transform rooftop = BuildRooftop(out Transform landingMarker, out RoofHatch hatch);
        BuildStreetLife();
        HelicopterRig helicopter = BuildHelicopter();
        BuildCutscene(scene, rooftop, landingMarker, hatch, helicopter);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettings();
        PointMainMenuAtIntro();
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Debug.Log("Built " + ScenePath);
    }

    
    static void BuildAtmosphere()
    {
        var atmosphere = new GameObject("Atmosphere").AddComponent<LevelAtmosphere>();
        LevelAssetFactory.SetFields(atmosphere,
            ("ambientColor", new Color(0.16f, 0.17f, 0.19f)),
            ("fog", true),
            ("fogColor", new Color(0.46f, 0.44f, 0.42f)),
            ("fogDensity", 0.0019f),
            ("reflectionIntensity", 0.35f),
            ("skybox", Mat("Sky/DaySky")));

        var sun = new GameObject("Sun").AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = new Color(1f, 0.93f, 0.82f);
        sun.intensity = 1.9f;
        sun.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(34f, 35f, 0f);

        var volume = new GameObject("Global Volume").AddComponent<Volume>();
        volume.isGlobal = true;
        volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(LevelAssetFactory.VolumeProfilePath);
    }

    
    
    static void BuildCity()
    {
        Transform city = new GameObject("City").transform;

        
        Block(city, "Ground", new Vector3(0f, StreetY - 1.2f, 0f), new Vector3(2600f, 2f, 2600f), LevelAssetFactory.CityMaterial("Ground"), false, shadows: false, tiled: false);

        GameObject strip = CityStripBuilder.Build();
        if (strip != null)
        {
            
            
            
            for (int column = -1; column <= 1; column++)
            {
                float slide = column * StripLength;
                PlaceStrip(city, strip, "Street", 90f, new Vector3(-ModelRoofCentre.y + slide, StreetY, ModelRoofCentre.x));
                PlaceStrip(city, strip, "Street North", -90f, new Vector3(-150f + slide, StreetY, StreetLine + 180f - ModelRoadX));
                PlaceStrip(city, strip, "Street South", 90f, new Vector3(190f + slide, StreetY, StreetLine - 180f + ModelRoadX));
            }
        }
        Static(city.gameObject);
    }

    
    static void PlaceStrip(Transform parent, GameObject strip, string name, float yaw, Vector3 offset)
    {
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(strip);
        instance.name = name;
        instance.transform.SetParent(parent, false);
        instance.transform.SetPositionAndRotation(offset, Quaternion.Euler(0f, yaw, 0f));
    }

    
    
    
    static Transform BuildRooftop(out Transform landingMarker, out RoofHatch hatch)
    {
        Transform roof = new GameObject("Rooftop").transform;

        
        Transform wall = new GameObject("Invisible Wall").transform;
        wall.SetParent(roof, false);
        for (int side = 0; side < 4; side++)
        {
            bool alongX = side % 2 == 0;
            float sign = side < 2 ? 1f : -1f;
            var go = new GameObject(alongX ? "Wall Z" : "Wall X");
            go.transform.SetParent(wall, false);
            var box = go.AddComponent<BoxCollider>();
            float offset = sign * ((alongX ? RoofHalfZ : RoofHalfX) - 0.2f);
            box.center = alongX ? new Vector3(0f, 4f, offset) : new Vector3(offset, 4f, 0f);
            box.size = alongX ? new Vector3(RoofHalfX * 2f, 8f, 0.4f) : new Vector3(0.4f, 8f, RoofHalfZ * 2f);
        }

        landingMarker = new GameObject("Landing Point").transform;
        landingMarker.SetParent(roof, false);
        landingMarker.localPosition = Landing + Vector3.up * 1.1f;

        
        GameObject hatchGo = Place(Prefab(LevelAssetFactory.RoofHatchPath), roof, HatchAt, 0f, "Roof Hatch");
        hatch = hatchGo.GetComponent<RoofHatch>();
        LevelAssetFactory.SetFields(hatch, ("nextScene", FirstLevel));

        return roof;
    }

    
    static void BuildStreetLife()
    {
        Transform street = new GameObject("Street Life").transform;
        GameObject[] zombies = Enumerable.Range(1, LevelAssetFactory.CrowdZombieVariants)
            .Select(i => Prefab(LevelAssetFactory.CrowdZombiePath(i))).ToArray();

        float[] crowdSpots = { -215f, -165f, -115f, -65f, -20f, 20f };
        for (int i = 0; i < crowdSpots.Length; i++)
        {
            var go = new GameObject($"Horde {i + 1}");
            go.transform.SetParent(street, false);
            go.transform.position = new Vector3(crowdSpots[i], StreetY, StreetLine + (i % 2 == 0 ? 3f : -3f));
            var crowd = go.AddComponent<ZombieCrowd>();
            LevelAssetFactory.SetFields(crowd,
                ("zombiePrefabs", zombies),
                ("count", i == 3 ? 40 : 34),
                ("radius", 13f),
                ("seed", 101 + i * 7));
        }
    }

    
    
    static HelicopterRig BuildHelicopter()
    {
        Vector3[] path =
        {
            
            new Vector3(-250f, -64f, StreetLine),
            new Vector3(-190f, -64f, StreetLine),
            new Vector3(-130f, -64f, StreetLine),
            new Vector3(-70f, -64f, StreetLine),
            new Vector3(-25f, -62f, StreetLine),
            
            new Vector3(-8f, -35f, StreetLine),
            new Vector3(2f, -5f, StreetLine - 1f),
            
            new Vector3(8f, 10f, 36f),
            new Vector3(10f, 11f, 28f),
            new Vector3(10f, 11f, 23f),
        };

        GameObject go = Place(Prefab(LevelAssetFactory.HelicopterPath), null, path[0], 90f, "Helicopter");
        Transform pathRoot = new GameObject("Flight Path").transform;
        var waypoints = new Transform[path.Length];
        for (int i = 0; i < path.Length; i++)
        {
            var wp = new GameObject($"Waypoint {i:00}").transform;
            wp.SetParent(pathRoot, false);
            wp.position = path[i];
            waypoints[i] = wp;
        }

        var rig = go.GetComponent<HelicopterRig>();
        LevelAssetFactory.SetFields(rig, ("waypoints", waypoints));
        return rig;
    }

    
    static void BuildCutscene(Scene scene, Transform rooftop, Transform landingMarker, RoofHatch hatch, HelicopterRig helicopter)
    {
        var rigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerRigPath);
        var playerRig = (GameObject)PrefabUtility.InstantiatePrefab(rigPrefab, scene);
        Transform player = playerRig.transform.Find("Player");
        var hud = playerRig.transform.Find("HUD").GetComponent<Canvas>();
        Camera playerCamera = player.Find("Main Camera").GetComponent<Camera>();
        Transform weapons = playerCamera.transform.Find("Weapons");

        
        player.position = landingMarker.position;
        Vector3 toHatch = HatchAt - Landing;
        toHatch.y = 0f;
        player.rotation = Quaternion.LookRotation(toHatch);
        
        playerCamera.farClipPlane = 1600f;

        
        foreach (string child in new[] { "Zombie Spawner", "Puddle Reflections" })
        {
            Transform t = playerRig.transform.Find(child);
            if (t != null) t.gameObject.SetActive(false);
        }
        Transform minimap = hud.transform.Find("Minimap");
        if (minimap != null) minimap.gameObject.SetActive(false);

        var cutscene = new GameObject("Cutscene").transform;
        Camera cutsceneCamera = BuildCutsceneCamera(cutscene);
        BuildOverlay(cutscene, out Image fade, out Text caption, out Text prompt, out GameObject skipHint, out GameObject[] letterbox);

        Transform towerLook = Marker(cutscene, "Tower Look", new Vector3(4f, 0f, 0f));
        Transform hatchLook = Marker(cutscene, "Hatch Look", HatchAt + Vector3.up * 0.6f);

        var rig = cutscene.gameObject.AddComponent<CutsceneRig>();
        LevelAssetFactory.SetFields(rig,
            ("cutsceneCamera", cutsceneCamera),
            ("playerCamera", playerCamera),
            ("hud", hud),
            ("fade", fade),
            ("caption", caption),
            ("prompt", prompt),
            ("skipHint", skipHint),
            ("letterbox", letterbox),
            ("player", player),
            
            ("pauseWhilePlaying", new Object[]
            {
                player.GetComponent<FPSController>(), player.GetComponent<WeaponManager>(),
                hud.GetComponentInChildren<SettingsMenu>(true),
            }),
            ("hideWhilePlaying", new Object[] { weapons.gameObject }));

        var intro = cutscene.gameObject.AddComponent<IntroCutscene>();
        LevelAssetFactory.SetFields(intro,
            ("rig", rig),
            ("helicopter", helicopter),
            ("towerLook", towerLook),
            ("hatchLook", hatchLook));

        LevelAssetFactory.SetFields(hatch, ("rig", rig));

        var bounds = rooftop.gameObject.AddComponent<RoofBounds>();
        LevelAssetFactory.SetFields(bounds,
            ("halfExtents", new Vector2(RoofHalfX, RoofHalfZ)),
            ("returnTo", landingMarker),
            ("rig", rig));
    }

    static Camera BuildCutsceneCamera(Transform parent)
    {
        var go = new GameObject("Cutscene Camera");
        go.transform.SetParent(parent, false);
        var camera = go.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.fieldOfView = 62f;
        camera.nearClipPlane = 0.08f;
        camera.farClipPlane = 1600f;
        
        camera.depth = 10f;
        camera.enabled = false;
        var urp = go.GetComponent<UniversalAdditionalCameraData>() ?? go.AddComponent<UniversalAdditionalCameraData>();
        urp.renderPostProcessing = true;
        return camera;
    }

    
    static void BuildOverlay(Transform parent, out Image fade, out Text caption, out Text prompt, out GameObject skipHint, out GameObject[] letterbox)
    {
        var canvasGo = new GameObject("Cutscene Overlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasGo.transform.SetParent(parent, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        canvas.sortingOrder = 50;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        letterbox = new[]
        {
            Bar(canvasGo.transform, "Letterbox Top", 1f),
            Bar(canvasGo.transform, "Letterbox Bottom", 0f),
        };

        fade = Full(canvasGo.transform, "Fade", new Color(0f, 0f, 0f, 0f));

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        caption = Label(canvasGo.transform, font, "Caption", 34, new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(1500f, 90f));
        caption.color = new Color(0.94f, 0.94f, 0.9f, 0f);

        prompt = Label(canvasGo.transform, font, "Prompt", 28, new Vector2(0.5f, 0f), new Vector2(0f, 260f), new Vector2(1200f, 60f));
        prompt.color = new Color(1f, 0.85f, 0.35f, 0f);

        Text skip = Label(canvasGo.transform, font, "Skip Hint", 20, new Vector2(1f, 0f), new Vector2(-40f, 46f), new Vector2(520f, 40f));
        skip.alignment = TextAnchor.MiddleRight;
        skip.text = "PRESS SPACE TO SKIP";
        skip.color = new Color(0.85f, 0.85f, 0.82f, 0.75f);
        skipHint = skip.gameObject;
        skipHint.SetActive(false);
    }

    static GameObject Bar(Transform parent, string name, float anchorY)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.anchorMin = new Vector2(0f, anchorY);
        rect.anchorMax = new Vector2(1f, anchorY);
        rect.pivot = new Vector2(0.5f, anchorY);
        rect.sizeDelta = new Vector2(0f, 76f);
        rect.anchoredPosition = Vector2.zero;
        go.GetComponent<Image>().color = Color.black;
        go.SetActive(false);
        return go;
    }

    static Image Full(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    static Text Label(Transform parent, Font font, string name, int size, Vector2 anchor, Vector2 position, Vector2 sizeDelta)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Outline));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = sizeDelta;
        var text = go.GetComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.raycastTarget = false;
        go.GetComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.8f);
        return text;
    }

    static Transform Marker(Transform parent, string name, Vector3 position)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        return go.transform;
    }

    
    static GameObject Block(Transform parent, string name, Vector3 centre, Vector3 size, Material mat, bool collider,
        bool shadows = true, bool tiled = true)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.localPosition = centre;
        go.transform.localScale = size;
        var renderer = go.GetComponent<Renderer>();
        if (mat != null) renderer.sharedMaterial = mat;
        renderer.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
        if (tiled) go.AddComponent<TiledSurface>().sizeMeters = new Vector2(Mathf.Max(size.x, size.z), Mathf.Max(size.y, size.z));
        return go;
    }

    static GameObject Place(GameObject prefab, Transform parent, Vector3 position, float yaw, string name)
    {
        if (prefab == null) return null;
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name = name;
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        return go;
    }

    static GameObject Prefab(string path)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) Debug.LogError("Intro scene builder could not load " + path);
        return prefab;
    }

    static Material Mat(string name) => AssetDatabase.LoadAssetAtPath<Material>($"{MaterialRoot}/{name}.mat");

    static void Static(GameObject root)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>())
        {
            
            if (t.GetComponentInParent<ZombieCrowd>() != null || t.GetComponentInParent<RoofHatch>() != null) continue;
            
            
            if (PrefabUtility.IsPartOfPrefabInstance(t.gameObject)) continue;
            GameObjectUtility.SetStaticEditorFlags(t.gameObject,
                StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
        }
    }

    static float Range(System.Random rng, float min, float max) => min + (float)rng.NextDouble() * (max - min);

    
    static void AddToBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.Any(s => s.path == ScenePath)) return;
        int menu = scenes.FindIndex(s => s.path == MainMenuScenePath);
        scenes.Insert(menu >= 0 ? menu + 1 : 0, new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    
    static void PointMainMenuAtIntro()
    {
        Scene menu = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
        var mainMenu = Object.FindAnyObjectByType<MainMenu>();
        if (mainMenu == null)
        {
            Debug.LogWarning("No MainMenu component in " + MainMenuScenePath);
            return;
        }
        var so = new SerializedObject(mainMenu);
        so.FindProperty("gameScene").stringValue = "Intro";
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(menu);
        EditorSceneManager.SaveScene(menu);
    }
}
