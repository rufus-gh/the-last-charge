using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;










public class FinaleDirector : MonoBehaviour
{
    
    static readonly Vector3 StageOrigin = new Vector3(4000f, 1000f, 4000f);
    const int CountdownFrom = 6;

    Transform ladderBase;
    VideoClip explosionVideo;

    Transform player;
    Camera cam;
    Transform cameraPivot;
    PlayerHealth health;

    Canvas canvas;
    Image blackFade;
    Image redFlash;
    RawImage video;
    Text objective;
    Text prompt;
    Text bigText;

    readonly System.Collections.Generic.List<ShamblerAdvance> swarm = new System.Collections.Generic.List<ShamblerAdvance>();
    Material asphalt, concrete, windowGlow, emberGlow, shaftDark;
    GameObject[] zombieModels;
    GameObject ladderPrefab, hatchLidPrefab;
    C4Device c4;
    readonly System.Collections.Generic.List<Renderer> hiddenWeaponRenderers = new System.Collections.Generic.List<Renderer>();

    
    const float HatchHalf = 1f;
    const float ShaftDepth = 3.6f;
    
    const float WalkSpeed = 3.2f;

    public static void Begin(Transform ladderBase, VideoClip explosionVideo)
    {
        if (FindAnyObjectByType<FinaleDirector>() != null) return;
        var go = new GameObject("Finale Director");
        var director = go.AddComponent<FinaleDirector>();
        director.ladderBase = ladderBase;
        director.explosionVideo = explosionVideo;
        director.StartCoroutine(director.Run());
    }

    IEnumerator Run()
    {
        var controller = FindAnyObjectByType<CharacterController>();
        player = controller != null ? controller.transform : null;
        health = FindAnyObjectByType<PlayerHealth>();
        cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        cameraPivot = cam != null ? cam.transform : null;

        BuildMaterials();
        BuildUI();
        HoldC4();

        
        
        var spawner = FindAnyObjectByType<ZombieSpawner>();
        if (spawner != null) spawner.enabled = false;
        if (health != null)
        {
            health.Heal(health.MaxHealth);
            
            health.SetInvulnerable(true);
        }
        var flashlight = FindAnyObjectByType<Flashlight>();
        if (flashlight != null) flashlight.AddBattery(flashlight.MaxBattery);

        yield return Escape();
        yield return Climb();
        yield return Street();
        yield return Activate();
        yield return Countdown();
        yield return Detonate();
        yield return Epilogue();
    }

    
    
    IEnumerator Escape()
    {
        objective.text = "THE CURE IS YOURS";
        objective.canvasRenderer.SetAlpha(0f);
        objective.CrossFadeAlpha(1f, 1.2f, true);

        SetGameplayInput(false);
        var controller = player != null ? player.GetComponent<CharacterController>() : null;
        if (controller != null) controller.enabled = false;
        
        yield return new WaitForSeconds(1.2f);

        if (player == null || ladderBase == null || cameraPivot == null) yield break;
        Vector3 front = Flat(ladderBase.forward), right = Flat(ladderBase.right);
        Vector3 pedestal = ladderBase.position + front * 1.75f;
        float side = Vector3.Dot(player.position - pedestal, right) < 0f ? -1f : 1f;
        float y = player.position.y;
        
        var route = new[]
        {
            pedestal + right * side * 2.0f + front * 0.4f,
            ladderBase.position + right * side * 2.0f + front * 0.7f,
            ladderBase.position + front * 0.7f,
        };
        foreach (Vector3 point in route)
        {
            Vector3 target = new Vector3(point.x, y, point.z);
            while (true)
            {
                Vector3 to = target - player.position;
                to.y = 0f;
                if (to.magnitude < 0.05f) break;
                player.position += to.normalized * Mathf.Min(WalkSpeed * Time.deltaTime, to.magnitude);
                player.rotation = Quaternion.Slerp(player.rotation, Quaternion.LookRotation(to, Vector3.up), 6f * Time.deltaTime);
                cameraPivot.localRotation = Quaternion.Slerp(cameraPivot.localRotation, Quaternion.identity, 4f * Time.deltaTime);
                yield return null;
            }
        }
        
        Quaternion from = player.rotation, face = Quaternion.LookRotation(-front, Vector3.up);
        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.5f)
        {
            player.rotation = Quaternion.Slerp(from, face, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        player.rotation = face;
    }

    
    
    IEnumerator Climb()
    {
        objective.CrossFadeAlpha(0f, 0.4f, true);
        if (c4 != null) c4.SetPose(C4Device.Pose.Tucked);

        if (player != null && ladderBase != null && cameraPivot != null)
        {
            float eyeOffset = cam.transform.position.y - player.position.y;
            float startY = player.position.y;
            float endY = ladderBase.position.y + 2.95f - eyeOffset;
            bool opened = false;
            const float duration = 3.4f;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float p = Mathf.SmoothStep(0f, 1f, t / duration);
                float y = Mathf.Lerp(startY, endY, p);
                Vector3 position = player.position;
                position.y = y + RungBob(y);
                player.position = position;
                
                float pitch = Mathf.Lerp(0f, -34f, p * p);
                cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, Mathf.Sin(y * 9f) * 1.1f);
                if (!opened && p > 0.7f)
                {
                    opened = true;
                    Sfx.Play("hatch-open");
                }
                yield return null;
            }
        }
        yield return Fade(blackFade, 0f, 1f, 0.6f);

        BuildStreet();
        MovePlayerToShaft();
        yield return new WaitForSeconds(0.5f);
    }

    
    static float RungBob(float y) => Mathf.Sin(y / 0.3f * Mathf.PI * 2f) * 0.025f;

    static Vector3 Flat(Vector3 v)
    {
        v.y = 0f;
        return v.normalized;
    }

    
    
    IEnumerator Street()
    {
        StartCoroutine(Fade(blackFade, 1f, 0f, 1.4f));

        if (player != null && cameraPivot != null)
        {
            float eyeOffset = cam.transform.position.y - player.position.y;
            float startEye = cam.transform.position.y;
            float endEye = StageOrigin.y + 1.62f;
            float baseZ = StageOrigin.z + 0.35f;
            const float duration = 3.8f;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float p = Mathf.SmoothStep(0f, 1f, t / duration);
                float eye = Mathf.Lerp(startEye, endEye, p);
                
                float step = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.78f, 1f, t / duration));
                player.position = new Vector3(StageOrigin.x, eye - eyeOffset + RungBob(eye) * (1f - step), baseZ + step * 2.1f);
                
                cameraPivot.localRotation = Quaternion.Euler(Mathf.Lerp(-30f, 0f, p * p), 0f, Mathf.Sin(eye * 9f) * 1.1f * (1f - step));
                if (c4 != null) c4.SetPose(p > 0.8f ? C4Device.Pose.Held : C4Device.Pose.Tucked);
                yield return null;
            }
            player.position = new Vector3(StageOrigin.x, endEye - eyeOffset, baseZ + 2.1f);
            cameraPivot.localRotation = Quaternion.identity;
        }
        if (c4 != null) c4.SetPose(C4Device.Pose.Held);

        for (int i = 0; i < 10; i++) SpawnShambler(24f, 70f);
        yield return new WaitForSeconds(1.8f);
    }

    
    IEnumerator Activate()
    {
        prompt.text = "LEFT-CLICK TO ACTIVATE THE CURE";
        prompt.canvasRenderer.SetAlpha(0f);
        bool clicked = false;
        while (!clicked)
        {
            prompt.canvasRenderer.SetAlpha(0.45f + 0.55f * Mathf.Abs(Mathf.Sin(Time.time * 3f)));
            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame) clicked = true;
            yield return null;
        }
        prompt.text = "";
        if (c4 != null)
        {
            c4.Press();
            c4.SetPose(C4Device.Pose.Raised);
            c4.ShowText(CountdownText(CountdownFrom));
            c4.Arm();
            Sfx.Play("thump", 0.5f, 1.6f);
        }
        
        ZombieVoice.SilenceAll();
    }

    static string CountdownText(int n) => "00:" + n.ToString("00");

    
    
    IEnumerator Countdown()
    {
        for (int n = CountdownFrom; n > 0; n--)
        {
            if (c4 != null) c4.ShowText(CountdownText(n));
            SpawnShambler(14f, 45f);
            SpawnShambler(14f, 45f);
            
            foreach (ShamblerAdvance z in swarm) if (z != null) z.speed = 1.6f + (CountdownFrom - n) * 0.35f;

            float ramp = 0.5f + 0.5f * (CountdownFrom - n) / (CountdownFrom - 1f);
            for (float t = 0f; t < 1f; t += Time.deltaTime)
            {
                
                float beat = Mathf.Pow(1f - t, 3f);
                redFlash.color = new Color(1f, 0.05f, 0.03f, 0.012f + beat * 0.06f * ramp);
                yield return null;
            }
        }
        redFlash.color = Color.clear;
        if (c4 != null) c4.ShowText("00:00");
    }

    
    IEnumerator Detonate()
    {
        redFlash.color = Color.white;
        yield return new WaitForSeconds(0.12f);
        redFlash.color = Color.clear;

        if (explosionVideo != null)
        {
            var rt = new RenderTexture(1920, 1080, 0);
            var vp = gameObject.AddComponent<VideoPlayer>();
            vp.playOnAwake = false;
            vp.renderMode = VideoRenderMode.RenderTexture;
            vp.targetTexture = rt;
            vp.isLooping = false;
            vp.audioOutputMode = VideoAudioOutputMode.None;
            vp.clip = explosionVideo;
            video.texture = rt;
            video.color = Color.white;
            video.gameObject.SetActive(true);
            video.transform.SetAsLastSibling();
            
            vp.Prepare();
            for (float wait = 0f; !vp.isPrepared && wait < 4f; wait += Time.deltaTime) yield return null;
            vp.Play();
            
            Sfx.Play("explosion-final", 1f, 1f, true, 0.9f);
            float length = (float)explosionVideo.length;
            if (length <= 0f) length = 5f;
            yield return new WaitForSeconds(length);
            vp.Stop();
        }
        else
        {
            
            Sfx.Play("explosion-final", 1f, 1f, true);
            blackFade.color = Color.black;
            blackFade.transform.SetAsLastSibling();
            bigText.color = new Color(0.55f, 0.55f, 0.55f);
            bigText.text = "[ city-explosion render goes here\n120 frames · 24 fps · 5s ]";
            bigText.transform.SetAsLastSibling();
            yield return new WaitForSeconds(5f);
            bigText.text = "";
        }
    }

    
    IEnumerator Epilogue()
    {
        blackFade.color = new Color(0f, 0f, 0f, blackFade.color.a);
        blackFade.transform.SetAsLastSibling();
        yield return Fade(blackFade, blackFade.color.a, 1f, 2f);
        video.gameObject.SetActive(false);

        float runTime = Time.time - RunState.StartedAt;
        int minutes = Mathf.FloorToInt(runTime / 60f), seconds = Mathf.FloorToInt(runTime % 60f);

        var center = new Vector2(0.5f, 0.5f);
        RuntimeUI.Text(canvas.transform, "Hero Title", "GAME COMPLETE", 104, new Color(0.35f, 1f, 0.5f),
            center, new Vector2(0f, 250f), new Vector2(1700f, 140f));
        RuntimeUI.Text(canvas.transform, "Hero Body",
            "They say the light was seen three countries away.\n" +
            "The wind carried the cure the rest of the way.\n\n" +
            "The city had lost its power. But for the first time in a month, the dead stayed down.\n\n" +
            "Everything lost its power that night. Everything except The Last Charge.",
            30, new Color(0.82f, 0.88f, 0.82f), center, new Vector2(0f, 10f), new Vector2(1300f, 400f));
        RuntimeUI.Text(canvas.transform, "Hero Stats",
            $"Run time  {minutes}:{seconds:00}      Zombies killed  {RunState.Kills}      Tokens  {RunState.Tokens}",
            26, new Color(1f, 0.85f, 0.5f), center, new Vector2(0f, -220f), new Vector2(1500f, 50f));

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        RuntimeUI.Button(canvas.transform, "Menu Button", "MAIN MENU", new Vector2(0f, -320f), new Vector2(320f, 64f), GoToMainMenu);

        
        foreach (Text t in canvas.GetComponentsInChildren<Text>())
            if (t != bigText && t != objective && t != prompt) { t.canvasRenderer.SetAlpha(0f); t.CrossFadeAlpha(1f, 1.5f, true); }

        while (true)
        {
            Keyboard k = Keyboard.current;
            if (k != null && (k.spaceKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame)) { GoToMainMenu(); break; }
            yield return null;
        }
    }

    void GoToMainMenu()
    {
        StopAllCoroutines();
        RunState.Reset();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    

    void SetGameplayInput(bool on)
    {
        foreach (var fps in FindObjectsByType<FPSController>(FindObjectsSortMode.None)) fps.enabled = on;
        foreach (var wm in FindObjectsByType<WeaponManager>(FindObjectsSortMode.None)) wm.enabled = on;
    }

    
    void MovePlayerToShaft()
    {
        foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            if (!c.transform.IsChildOf(transform)) c.gameObject.SetActive(false);
        foreach (var other in FindObjectsByType<Camera>(FindObjectsSortMode.None))
            if (other != cam) other.gameObject.SetActive(false);

        if (player == null) return;
        var controller = player.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;
        float eyeOffset = cam != null ? cam.transform.position.y - player.position.y : 1.6f;
        player.rotation = Quaternion.identity;
        if (cameraPivot != null) cameraPivot.localRotation = Quaternion.Euler(-30f, 0f, 0f);
        player.position = new Vector3(StageOrigin.x, StageOrigin.y - ShaftDepth + 1.62f - eyeOffset, StageOrigin.z + 0.35f);
    }

    
    void HoldC4()
    {
        var prefab = Resources.Load<GameObject>("Finale/C4Viewmodel");
        if (prefab == null || cam == null) return;
        var wm = FindAnyObjectByType<WeaponManager>();
        if (wm != null)
            foreach (Renderer r in wm.GetComponentsInChildren<Renderer>())
                if (r.enabled)
                {
                    r.enabled = false;
                    hiddenWeaponRenderers.Add(r);
                }
        
        var flashlight = FindAnyObjectByType<Flashlight>();
        if (flashlight != null) flashlight.SetLocked(true);
        c4 = Instantiate(prefab, cam.transform).GetComponent<C4Device>();
        c4.SnapTo(C4Device.Pose.Hidden);
        c4.SetPose(C4Device.Pose.Held);
    }

    void BuildUI()
    {
        canvas = RuntimeUI.Canvas("Finale Canvas", 60);
        canvas.transform.SetParent(transform, false);

        video = NewRawImage("Video");
        video.gameObject.SetActive(false);

        blackFade = FullScreen("Black Fade", new Color(0f, 0f, 0f, 0f));
        redFlash = FullScreen("Red Flash", new Color(1f, 0f, 0f, 0f));

        var center = new Vector2(0.5f, 0.5f);
        objective = RuntimeUI.Text(canvas.transform, "Objective", "", 34, new Color(0.85f, 1f, 0.78f),
            new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(1500f, 120f));
        prompt = RuntimeUI.Text(canvas.transform, "Prompt", "", 42, new Color(1f, 0.92f, 0.6f),
            center, new Vector2(0f, -260f), new Vector2(1400f, 80f));
        bigText = RuntimeUI.Text(canvas.transform, "Big", "", 170, Color.white, center, Vector2.zero, new Vector2(1600f, 400f));
    }

    Image FullScreen(string name, Color color)
    {
        Image image = RuntimeUI.Stretch(canvas.transform, name).gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    RawImage NewRawImage(string name)
    {
        RawImage image = RuntimeUI.Stretch(canvas.transform, name).gameObject.AddComponent<RawImage>();
        image.color = Color.black;
        image.raycastTarget = false;
        return image;
    }

    IEnumerator Fade(Image image, float from, float to, float duration)
    {
        Color c = image.color;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, t / duration);
            image.color = c;
            yield return null;
        }
        c.a = to;
        image.color = c;
    }

    

    void BuildMaterials()
    {
        
        zombieModels = new GameObject[3];
        for (int i = 0; i < zombieModels.Length; i++) zombieModels[i] = Resources.Load<GameObject>($"Zombies/ZombieModel{i + 1}");
        ladderPrefab = Resources.Load<GameObject>("Finale/Ladder");
        hatchLidPrefab = Resources.Load<GameObject>("Finale/HatchLid");
        shaftDark = Lit(new Color(0.05f, 0.05f, 0.055f), null);
        
        if (shaftDark.HasProperty("_Cull")) shaftDark.SetFloat("_Cull", 0f);
        asphalt = Lit(new Color(0.06f, 0.06f, 0.07f), null);
        concrete = Lit(new Color(0.12f, 0.11f, 0.1f), null);
        windowGlow = Lit(new Color(0.1f, 0.1f, 0.12f), new Color(1f, 0.55f, 0.2f) * 1.4f);
        emberGlow = Lit(new Color(0.2f, 0.08f, 0.03f), new Color(1f, 0.4f, 0.12f) * 2.2f);
    }

    static Material Lit(Color albedo, Color? emission)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var m = new Material(shader);
        m.color = albedo;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", albedo);
        if (emission.HasValue)
        {
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            m.SetColor("_EmissionColor", emission.Value);
        }
        return m;
    }

    static GameObject Box(Transform parent, Vector3 pos, Vector3 size, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.Destroy(go.GetComponent<Collider>());
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    void BuildStreet()
    {
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.03f, 0.02f);
        }
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(0.1f, 0.04f, 0.03f);
        RenderSettings.fogDensity = 0.012f;
        RenderSettings.ambientLight = new Color(0.12f, 0.06f, 0.05f);

        var street = new GameObject("Finale Street").transform;
        street.position = StageOrigin;

        
        var keyGo = new GameObject("Key Light");
        keyGo.transform.SetParent(street, false);
        keyGo.transform.localEulerAngles = new Vector3(55f, 20f, 0f);
        var key = keyGo.AddComponent<Light>();
        key.type = LightType.Directional;
        key.color = new Color(1f, 0.4f, 0.3f);
        key.intensity = 0.5f;

        
        
        const float hole = HatchHalf;
        Box(street, new Vector3(0f, -0.05f, (-5f - hole) / 2f), new Vector3(11f, 0.1f, 5f - hole), asphalt);
        Box(street, new Vector3(0f, -0.05f, (85f + hole) / 2f), new Vector3(11f, 0.1f, 85f - hole), asphalt);
        foreach (int side in new[] { -1, 1 })
            Box(street, new Vector3(side * (5.5f + hole) / 2f, -0.05f, 0f), new Vector3(5.5f - hole, 0.1f, hole * 2f), asphalt);
        BuildHatch(street);
        Box(street, new Vector3(-6.5f, 0.05f, 40f), new Vector3(2f, 0.2f, 90f), concrete);
        Box(street, new Vector3(6.5f, 0.05f, 40f), new Vector3(2f, 0.2f, 90f), concrete);
        for (int i = 0; i < 12; i++)
            Box(street, new Vector3(0f, 0.01f, 6f + i * 6f), new Vector3(0.3f, 0.02f, 2f), windowGlow);

        
        for (int i = 0; i < 8; i++)
        {
            float z = 10f + i * 10f;
            foreach (int side in new[] { -1, 1 })
            {
                float h = 10f + (i % 3) * 6f;
                float x = side * (9f + (i % 2) * 1.5f);
                Box(street, new Vector3(x, h * 0.5f, z), new Vector3(5f, h, 8f), concrete);
                
                for (int wy = 0; wy < (int)(h / 3f); wy++)
                    for (int wx = 0; wx < 2; wx++)
                        if ((wy + wx + i) % 3 != 0)
                            Box(street, new Vector3(x - side * 2.55f, 2f + wy * 3f, z - 2.5f + wx * 3f),
                                new Vector3(0.1f, 1.4f, 1.2f), windowGlow);
            }
        }

        
        for (int i = 0; i < 4; i++)
        {
            float z = 14f + i * 14f;
            float x = (i % 2 == 0 ? -1f : 1f) * 3f;
            Box(street, new Vector3(x, 0.4f, z), new Vector3(1.4f, 0.8f, 1.4f), emberGlow);
            var fireGo = new GameObject("Fire");
            fireGo.transform.SetParent(street, false);
            fireGo.transform.localPosition = new Vector3(x, 1f, z);
            var fire = fireGo.AddComponent<Light>();
            fire.type = LightType.Point;
            fire.color = new Color(1f, 0.45f, 0.15f);
            fire.intensity = 3.5f;
            fire.range = 18f;
        }
    }

    
    
    void BuildHatch(Transform street)
    {
        const float h = HatchHalf;
        
        foreach (int side in new[] { -1, 1 })
        {
            Box(street, new Vector3(0f, 0.08f, side * (h + 0.15f)), new Vector3(h * 2f + 0.6f, 0.16f, 0.3f), concrete);
            Box(street, new Vector3(side * (h + 0.15f), 0.08f, 0f), new Vector3(0.3f, 0.16f, h * 2f), concrete);
        }
        
        if (hatchLidPrefab != null)
        {
            var lid = Instantiate(hatchLidPrefab, street);
            lid.name = "Hatch Lid";
            lid.transform.localPosition = new Vector3(0f, 0.21f, -(h + 0.3f));
            lid.transform.localRotation = Quaternion.Euler(-104f, 0f, 0f);
        }

        
        float w = h + 0.05f;
        Box(street, new Vector3(0f, -ShaftDepth / 2f, w), new Vector3(w * 2f, ShaftDepth, 0.1f), shaftDark);
        Box(street, new Vector3(0f, -ShaftDepth / 2f, -w), new Vector3(w * 2f, ShaftDepth, 0.1f), shaftDark);
        Box(street, new Vector3(w, -ShaftDepth / 2f, 0f), new Vector3(0.1f, ShaftDepth, w * 2f), shaftDark);
        Box(street, new Vector3(-w, -ShaftDepth / 2f, 0f), new Vector3(0.1f, ShaftDepth, w * 2f), shaftDark);
        Box(street, new Vector3(0f, -ShaftDepth - 0.05f, 0f), new Vector3(w * 2f, 0.1f, w * 2f), shaftDark);
        var glowGo = new GameObject("Shaft Light");
        glowGo.transform.SetParent(street, false);
        glowGo.transform.localPosition = new Vector3(0f, -1.6f, 0f);
        var glow = glowGo.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.color = new Color(1f, 0.7f, 0.4f);
        glow.intensity = 3.5f;
        glow.range = 8f;

        if (ladderPrefab != null)
        {
            
            var ladder = Instantiate(ladderPrefab, street);
            ladder.name = "Ladder";
            ladder.transform.localPosition = new Vector3(0f, -ShaftDepth, h - 0.1f);
            ladder.transform.localScale = Vector3.one * ((ShaftDepth + 0.5f) / 3.26f);
        }
    }

    

    void SpawnShambler(float minZ, float maxZ)
    {
        if (swarm.Count >= 44) return;
        float z = Random.Range(minZ, maxZ);
        float x = Random.Range(-6f, 6f);
        Vector3 pos = StageOrigin + new Vector3(x, 0f, z);

        var root = new GameObject("Shambler").transform;
        root.position = pos;
        root.rotation = Quaternion.Euler(0f, 180f, 0f);

        GameObject model = zombieModels[Random.Range(0, zombieModels.Length)];
        if (model != null) Instantiate(model, root);

        var adv = root.gameObject.AddComponent<ShamblerAdvance>();
        adv.target = player;
        adv.speed = Random.Range(1.3f, 2.1f);
        swarm.Add(adv);
    }
}
