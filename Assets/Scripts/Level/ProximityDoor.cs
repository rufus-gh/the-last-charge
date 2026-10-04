using UnityEngine;



public class ProximityDoor : MonoBehaviour
{
    [SerializeField] Transform hinge;
    [Tooltip("Hinge angle when fully open. Negative swings the door out towards the player.")]
    [SerializeField] float openAngle = -105f;
    [SerializeField] float openDistance = 4.5f;
    [Tooltip("Seconds to swing fully open")]
    [SerializeField] float openTime = 0.8f;
    [SerializeField] bool closeWhenPlayerLeaves;
    [Tooltip("Lights that fade in as the door opens (portal glow)")]
    [SerializeField] Light[] revealLights;

    public bool IsOpen => openAmount >= 0.99f;

    Transform player;
    float openAmount;
    bool opening;
    float[] revealIntensities;
    float nextPlayerSearch;

    void Awake()
    {
        revealIntensities = new float[revealLights.Length];
        for (int i = 0; i < revealLights.Length; i++)
        {
            revealIntensities[i] = revealLights[i].intensity;
            revealLights[i].intensity = 0f;
        }
    }

    void Update()
    {
        
        if (player == null && Time.time >= nextPlayerSearch)
        {
            var controller = FindAnyObjectByType<CharacterController>();
            if (controller != null) player = controller.transform;
            nextPlayerSearch = Time.time + 1f;
        }

        if (player != null)
        {
            Vector3 offset = player.position - transform.position;
            offset.y = 0f;
            bool near = offset.magnitude < openDistance;
            if (near) opening = true;
            else if (closeWhenPlayerLeaves) opening = false;
        }

        openAmount = Mathf.MoveTowards(openAmount, opening ? 1f : 0f, Time.deltaTime / Mathf.Max(openTime, 0.01f));
        float eased = Mathf.SmoothStep(0f, 1f, openAmount);
        if (hinge != null) hinge.localRotation = Quaternion.Euler(0f, openAngle * eased, 0f);
        for (int i = 0; i < revealLights.Length; i++)
            if (revealLights[i] != null) revealLights[i].intensity = revealIntensities[i] * eased;
    }
}
