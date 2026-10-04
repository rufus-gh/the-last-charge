using UnityEngine;




public class RoofBounds : MonoBehaviour
{
    [Tooltip("Half the roof size in metres, measured from this object")]
    [SerializeField] Vector2 halfExtents = new Vector2(24f, 24f);
    [Tooltip("Slack outside the roof before the player is pulled back")]
    [SerializeField] float margin = 1.5f;
    [Tooltip("Height below this object that counts as having fallen off")]
    [SerializeField] float fallDistance = 6f;
    [SerializeField] Transform returnTo;
    [SerializeField] CutsceneRig rig;
    [SerializeField] string warning = "THERE IS NO WAY DOWN BUT THE HATCH";

    CharacterController player;
    float nextPlayerSearch;

    void Update()
    {
        
        if (rig != null && rig.Playing) return;

        if (player == null)
        {
            if (Time.time < nextPlayerSearch) return;
            player = FindAnyObjectByType<CharacterController>();
            nextPlayerSearch = Time.time + 0.5f;
            if (player == null) return;
        }

        Vector3 local = transform.InverseTransformPoint(player.transform.position);
        bool off = Mathf.Abs(local.x) > halfExtents.x + margin
                   || Mathf.Abs(local.z) > halfExtents.y + margin
                   || local.y < -fallDistance;
        if (!off || returnTo == null) return;

        
        player.enabled = false;
        player.transform.position = returnTo.position;
        player.enabled = true;
        if (rig != null && !rig.Playing && !string.IsNullOrEmpty(warning)) rig.Flash(warning, 2.5f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.6f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(new Vector3(0f, -fallDistance / 2f, 0f),
            new Vector3((halfExtents.x + margin) * 2f, fallDistance, (halfExtents.y + margin) * 2f));
    }
}
