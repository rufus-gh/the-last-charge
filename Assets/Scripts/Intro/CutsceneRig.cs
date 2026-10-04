using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;





public class CutsceneRig : MonoBehaviour
{
    [Header("Cameras")]
    [Tooltip("Cinematic camera, higher depth than the player's. Switched on while a shot plays.")]
    [SerializeField] Camera cutsceneCamera;
    [SerializeField] Camera playerCamera;

    [Header("Overlay")]
    [Tooltip("The player's normal HUD, hidden while a shot plays")]
    [SerializeField] Canvas hud;
    [SerializeField] Image fade;
    [SerializeField] Text caption;
    [SerializeField] Text prompt;
    [SerializeField] GameObject skipHint;
    [Tooltip("Black bars top and bottom")]
    [SerializeField] GameObject[] letterbox;

    [Header("Player")]
    [SerializeField] Transform player;
    [Tooltip("Frozen while a shot plays (movement, weapon input)")]
    [SerializeField] Behaviour[] pauseWhilePlaying;
    [Tooltip("Hidden while a shot plays, so the gun and its flashlight don't show up in the shot (the player's own body is hidden too, so it casts no shadow)")]
    [SerializeField] GameObject[] hideWhilePlaying;

    public Camera Camera => cutsceneCamera;
    public Transform Player => player;
    public bool Playing { get; private set; }

    
    public Vector3 EyePosition => playerCamera != null ? playerCamera.transform.position : player.position + Vector3.up * 1.6f;

    float captionUntil;
    float captionFade = 0.4f;
    float captionShown;
    float baseFieldOfView;
    readonly List<Renderer> hiddenRenderers = new List<Renderer>();

    void Awake()
    {
        if (cutsceneCamera != null)
        {
            baseFieldOfView = cutsceneCamera.fieldOfView;
            cutsceneCamera.enabled = false;
        }
        SetFade(0f);
        SetPrompt(null);
        if (caption != null) caption.text = "";
        ShowLetterbox(false);
        if (skipHint != null) skipHint.SetActive(false);
    }

    void Update()
    {
        if (caption == null) return;
        
        float alpha = 0f;
        if (Time.time < captionUntil)
        {
            float inAlpha = Mathf.Clamp01((Time.time - captionShown) / captionFade);
            float outAlpha = Mathf.Clamp01((captionUntil - Time.time) / captionFade);
            alpha = Mathf.Min(inAlpha, outAlpha);
        }
        SetAlpha(caption, alpha);
    }

    public void Begin(bool skippable)
    {
        Playing = true;
        if (cutsceneCamera != null)
        {
            cutsceneCamera.enabled = true;
            cutsceneCamera.fieldOfView = baseFieldOfView;
        }
        if (playerCamera != null) playerCamera.enabled = false;
        if (hud != null) hud.enabled = false;
        if (skipHint != null) skipHint.SetActive(skippable);
        ShowLetterbox(true);
        SetPrompt(null);

        
        hiddenRenderers.Clear();
        if (player != null)
            foreach (Renderer r in player.GetComponentsInChildren<Renderer>())
                if (r.enabled)
                {
                    r.enabled = false;
                    hiddenRenderers.Add(r);
                }

        foreach (Behaviour b in pauseWhilePlaying) if (b != null) b.enabled = false;
        foreach (GameObject go in hideWhilePlaying) if (go != null) go.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    
    public void End()
    {
        Playing = false;
        if (cutsceneCamera != null)
        {
            cutsceneCamera.enabled = false;
            cutsceneCamera.fieldOfView = baseFieldOfView;
        }
        if (playerCamera != null) playerCamera.enabled = true;
        if (hud != null) hud.enabled = true;
        if (skipHint != null) skipHint.SetActive(false);
        ShowLetterbox(false);
        SetFade(0f);

        foreach (Renderer r in hiddenRenderers) if (r != null) r.enabled = true;
        hiddenRenderers.Clear();
        foreach (GameObject go in hideWhilePlaying) if (go != null) go.SetActive(true);
        
        foreach (Behaviour b in pauseWhilePlaying) if (b != null) b.enabled = true;
    }

    public void PlaceCamera(Vector3 position, Quaternion rotation)
    {
        if (cutsceneCamera == null) return;
        cutsceneCamera.transform.SetPositionAndRotation(position, rotation);
    }

    public void SetFieldOfView(float fov)
    {
        if (cutsceneCamera != null) cutsceneCamera.fieldOfView = fov;
    }

    public float BaseFieldOfView => baseFieldOfView;

    
    public void Flash(string text, float seconds, float fadeTime = 0.4f)
    {
        if (caption == null) return;
        caption.text = text;
        captionShown = Time.time;
        captionUntil = Time.time + Mathf.Max(seconds, fadeTime * 2f);
        captionFade = fadeTime;
    }

    
    public void SetPrompt(string text)
    {
        if (prompt == null) return;
        prompt.text = text ?? "";
        SetAlpha(prompt, string.IsNullOrEmpty(text) ? 0f : 1f);
    }

    public void SetFade(float alpha)
    {
        if (fade != null) SetAlpha(fade, Mathf.Clamp01(alpha));
    }

    void ShowLetterbox(bool show)
    {
        foreach (GameObject bar in letterbox) if (bar != null) bar.SetActive(show);
    }

    static void SetAlpha(Graphic graphic, float alpha)
    {
        Color c = graphic.color;
        c.a = alpha;
        graphic.color = c;
    }
}
