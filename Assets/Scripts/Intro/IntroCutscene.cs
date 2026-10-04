using System;
using UnityEngine;





public class IntroCutscene : MonoBehaviour
{
    [SerializeField] CutsceneRig rig;
    [SerializeField] HelicopterRig helicopter;

    [Header("Shot targets")]
    [Tooltip("Top of the tower the player is being dropped onto")]
    [SerializeField] Transform towerLook;
    [Tooltip("The roof hatch: what the camera stares at while you fall")]
    [SerializeField] Transform hatchLook;

    [Header("Timing")]
    [Tooltip("Flying down the street over the hordes")]
    [SerializeField] float flyTime = 13f;
    [Tooltip("Hovering over the roof before the jump")]
    [SerializeField] float hoverTime = 3f;
    [SerializeField] float leapTime = 1.9f;
    [SerializeField] float landTime = 0.6f;
    [SerializeField] float openFadeTime = 1.4f;

    [Header("Camera")]
    [Tooltip("Where the camera looks during the flight, relative to the camera mount (z is straight ahead, level): down the avenue, then up and in to the building")]
    [SerializeField] Vector3 streetLookOffset = new Vector3(0f, -26f, 48f);
    [SerializeField] float leapArc = 1.4f;
    [SerializeField] float leapRoll = 16f;
    [SerializeField] float leapZoom = 12f;
    [SerializeField] float landShake = 0.7f;

    [Header("Captions")]
    [SerializeField] Caption[] captions =
    {
        new Caption { time = 1.0f, duration = 3.4f, text = "Day thirty-one. The city had lost its power. Then it lost its people." },
        new Caption { time = 4.8f, duration = 3.4f, text = "Every road was overrun. Nothing down there was still alive." },
        new Caption { time = 8.6f, duration = 3.4f, text = "But one signal still came from Spectre Tower, on a loop." },
        new Caption { time = 12.4f, duration = 3.4f, text = "The cure is real, it said. Seven floors down." },
    };
    [SerializeField] string landingCaption = "FIND THE HATCH.";

    [Serializable]
    public class Caption
    {
        public float time;
        public float duration = 3f;
        [TextArea]
        public string text;
    }

    float time;
    int nextCaption;
    bool finished;
    Vector3 leapFrom;
    Vector3 flyLook;
    bool leapStarted;
    bool landed;

    void Start()
    {
        if (rig == null || helicopter == null)
        {
            Debug.LogWarning("IntroCutscene is missing its rig or helicopter; skipping the intro.", this);
            Finish();
            return;
        }
        rig.Begin(skippable: false);
        rig.SetFade(1f);
        helicopter.SetProgress(0f);
    }

    void Update()
    {
        if (finished) return;
        time += Time.deltaTime;
        PlayCaptions();
        
        rig.SetFade(1f - Mathf.Clamp01(time / openFadeTime));

        float hoverStart = flyTime;
        float leapStart = flyTime + hoverTime;
        float landStart = leapStart + leapTime;

        if (time < hoverStart) Fly(time / flyTime);
        else if (time < leapStart) Hover((time - hoverStart) / hoverTime);
        else if (time < landStart) Leap((time - leapStart) / leapTime);
        else if (time < landStart + landTime) Land((time - landStart) / landTime);
        else Finish();
    }

    
    
    void Fly(float t)
    {
        
        helicopter.SetProgress(Mathf.SmoothStep(0f, 1f, t));

        Transform mount = helicopter.DoorMount;
        Vector3 ahead = Vector3.ProjectOnPlane(mount.forward, Vector3.up).normalized;
        flyLook = mount.position + ahead * streetLookOffset.z + Vector3.up * streetLookOffset.y;
        LookFrom(mount.position, flyLook, 0f);
    }

    
    void Hover(float t)
    {
        helicopter.SetProgress(1f);
        Transform door = helicopter.DoorMount;
        Vector3 from = towerLook != null ? towerLook.position : door.position + door.forward * 20f;
        Vector3 to = hatchLook != null ? hatchLook.position : rig.EyePosition;
        Vector3 target = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
        
        target = Vector3.Lerp(flyLook, target, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * 3f)));
        LookFrom(door.position, target, 0f);
    }

    
    void Leap(float t)
    {
        helicopter.SetProgress(1f);
        if (!leapStarted)
        {
            leapFrom = helicopter.DoorMount.position;
            leapStarted = true;
        }

        Vector3 to = rig.EyePosition;
        float across = 1f - Mathf.Pow(1f - t, 1.6f);
        Vector3 position = Vector3.Lerp(leapFrom, to, across);
        
        position.y = Mathf.Lerp(leapFrom.y, to.y, t * t) + Mathf.Sin(t * Mathf.PI) * leapArc;

        Vector3 target = hatchLook != null ? hatchLook.position : to + helicopter.DoorMount.forward * 10f;
        
        float roll = Mathf.Sin(t * Mathf.PI) * leapRoll * (1f - Mathf.InverseLerp(0.8f, 1f, t));
        LookFrom(position, target, roll);
        rig.SetFieldOfView(rig.BaseFieldOfView + leapZoom * Mathf.Sin(t * Mathf.PI * 0.5f));
    }

    
    void Land(float t)
    {
        if (!landed)
        {
            landed = true;
            Sfx.Play("thump");
        }
        float strength = landShake * (1f - t) * (1f - t);
        Vector3 eye = rig.EyePosition;
        Vector3 shake = new Vector3(Mathf.Sin(t * 61f), Mathf.Sin(t * 53f + 1.3f), Mathf.Sin(t * 47f + 2.1f)) * strength * 0.12f;
        Vector3 target = hatchLook != null ? hatchLook.position : eye + helicopter.DoorMount.forward * 10f;
        LookFrom(eye + shake, target, Mathf.Sin(t * 44f) * strength * 4f);
        rig.SetFieldOfView(Mathf.Lerp(rig.BaseFieldOfView + leapZoom, rig.BaseFieldOfView, t));
    }

    void LookFrom(Vector3 position, Vector3 target, float roll)
    {
        Vector3 direction = target - position;
        if (direction.sqrMagnitude < 0.0001f) direction = Vector3.forward;
        Quaternion rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 0f, roll);
        rig.PlaceCamera(position, rotation);
    }

    void PlayCaptions()
    {
        while (nextCaption < captions.Length && time >= captions[nextCaption].time)
        {
            Caption caption = captions[nextCaption++];
            rig.Flash(caption.text, caption.duration);
        }
    }

    
    void Finish()
    {
        if (finished) return;
        finished = true;

        
        if (helicopter != null) helicopter.FadeOutSound(landed ? 3f : 0.5f);

        if (rig != null)
        {
            if (rig.Player != null && hatchLook != null)
            {
                Vector3 toHatch = hatchLook.position - rig.Player.position;
                toHatch.y = 0f;
                if (toHatch.sqrMagnitude > 0.01f) rig.Player.rotation = Quaternion.LookRotation(toHatch);
            }
            rig.End();
            if (!string.IsNullOrEmpty(landingCaption)) rig.Flash(landingCaption, 3.5f);
        }
        enabled = false;
    }
}
