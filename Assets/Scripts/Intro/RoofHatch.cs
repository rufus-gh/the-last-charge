using UnityEngine;
using UnityEngine.SceneManagement;





public class RoofHatch : MonoBehaviour
{
    [Header("Lid")]
    [Tooltip("Hinge the lid hangs off; it rotates to openEuler")]
    [SerializeField] Transform hinge;
    [SerializeField] Vector3 openEuler = new Vector3(-104f, 0f, 0f);
    [SerializeField] float openDistance = 6f;
    [SerializeField] float openTime = 1.1f;

    [Header("Climbing down")]
    [Tooltip("Where the camera ends up at the bottom of the shaft")]
    [SerializeField] Transform descendTo;
    [SerializeField] CutsceneRig rig;
    [SerializeField] float descendTime = 2.2f;
    [Tooltip("Fraction of the descent spent with the lid slamming shut above you")]
    [Range(0.1f, 0.9f)] [SerializeField] float slamAt = 0.55f;
    [SerializeField] string nextScene = "Level_01";

    [Header("Prompt")]
    [SerializeField] string farPrompt = "THE HATCH IS THE ONLY WAY DOWN";
    [SerializeField] string nearPrompt = "STEP INTO THE HATCH";

    [Header("Light")]
    [Tooltip("Glow from inside the shaft, fades in with the lid")]
    [SerializeField] Light shaftLight;

    float openAmount;
    bool open;
    bool descending;
    float descendStart;
    Vector3 descendFrom;
    Quaternion descendFromRotation;
    float shaftLightIntensity;
    Transform player;
    Collider enterTrigger;
    float nextPlayerSearch;
    bool promptShown;
    bool slammed;

    void Awake()
    {
        if (shaftLight != null)
        {
            shaftLightIntensity = shaftLight.intensity;
            shaftLight.intensity = 0f;
        }
        if (hinge != null) hinge.localRotation = Quaternion.identity;
        
        foreach (Collider c in GetComponentsInChildren<Collider>())
            if (c.isTrigger) { enterTrigger = c; break; }
    }

    void Update()
    {
        if (descending)
        {
            Descend();
            return;
        }

        FindPlayer();
        if (player != null && enterTrigger != null && (rig == null || !rig.Playing) && InsideOpening(player.position))
        {
            StartDescent();
            return;
        }
        if (player != null && rig != null && !rig.Playing)
        {
            float distance = Vector3.Distance(Flat(player.position), Flat(transform.position));
            if (distance < openDistance && !open)
            {
                open = true;
                Sfx.PlayAt("hatch-open", transform.position);
            }
            
            string prompt = distance < openDistance ? nearPrompt : farPrompt;
            if (promptShown || distance < openDistance * 3f)
            {
                rig.SetPrompt(prompt);
                promptShown = true;
            }
        }

        openAmount = Mathf.MoveTowards(openAmount, open ? 1f : 0f, Time.deltaTime / Mathf.Max(openTime, 0.01f));
        float eased = Mathf.SmoothStep(0f, 1f, openAmount);
        if (hinge != null) hinge.localRotation = Quaternion.Euler(openEuler * eased);
        if (shaftLight != null) shaftLight.intensity = shaftLightIntensity * eased;
    }

    
    bool InsideOpening(Vector3 p)
    {
        Bounds b = enterTrigger.bounds;
        return p.x > b.min.x && p.x < b.max.x && p.z > b.min.z && p.z < b.max.z
               && p.y > b.min.y - 1.5f && p.y < b.max.y + 1.5f;
    }

    void OnTriggerEnter(Collider other)
    {
        
        if (other.GetComponentInParent<CharacterController>() == null) return;
        StartDescent();
    }

    void StartDescent()
    {
        if (descending) return;
        if (rig == null)
        {
            LoadNextLevel();
            return;
        }

        descending = true;
        descendStart = Time.time;
        descendFrom = rig.EyePosition;
        descendFromRotation = rig.Camera != null ? rig.Camera.transform.rotation : Quaternion.identity;
        rig.Begin(skippable: false);
        rig.SetPrompt(null);
        rig.PlaceCamera(descendFrom, descendFromRotation);
        rig.Flash("NO WAY BACK UP.", descendTime);
    }

    
    void Descend()
    {
        float t = Mathf.Clamp01((Time.time - descendStart) / Mathf.Max(descendTime, 0.01f));
        Vector3 bottom = descendTo != null ? descendTo.position : transform.position - Vector3.up * 6f;
        Vector3 position = Vector3.Lerp(descendFrom, bottom, Mathf.SmoothStep(0f, 1f, t));

        
        Quaternion lookUp = Quaternion.LookRotation(transform.position + Vector3.up * 2.4f - position);
        Quaternion lookDown = Quaternion.LookRotation(Vector3.Lerp(Vector3.down, transform.forward, 0.4f).normalized);
        float up = Mathf.Sin(Mathf.Clamp01(t / slamAt) * Mathf.PI * 0.5f);
        Quaternion rotation = Quaternion.Slerp(descendFromRotation, lookUp, up);
        if (t > slamAt) rotation = Quaternion.Slerp(lookUp, lookDown, Mathf.InverseLerp(slamAt, 1f, t));
        rig.PlaceCamera(position, rotation);

        
        float closed = Mathf.Clamp01(t / slamAt);
        if (closed >= 1f && !slammed)
        {
            slammed = true;
            Sfx.Play("thump");
        }
        if (hinge != null) hinge.localRotation = Quaternion.Euler(openEuler * (1f - closed * closed));
        if (shaftLight != null) shaftLight.intensity = shaftLightIntensity * (1f - closed * 0.65f);

        
        rig.SetFade(Mathf.Clamp01((transform.position.y - position.y - 0.3f) / 1.2f));
        if (t >= 1f) LoadNextLevel();
    }

    void LoadNextLevel()
    {
        if (string.IsNullOrEmpty(nextScene))
        {
            Debug.LogWarning("RoofHatch has no next scene set.", this);
            return;
        }
        Debug.Log("Player climbed down the roof hatch into " + nextScene, this);
        SceneManager.LoadScene(nextScene);
    }

    void FindPlayer()
    {
        if (player != null || Time.time < nextPlayerSearch) return;
        var controller = FindAnyObjectByType<CharacterController>();
        if (controller != null) player = controller.transform;
        nextPlayerSearch = Time.time + 0.5f;
    }

    static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
}
