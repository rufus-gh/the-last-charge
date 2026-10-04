using UnityEngine;






[RequireComponent(typeof(ReflectionProbe))]
public class FollowReflectionProbe : MonoBehaviour
{
    [SerializeField] Transform target;
    [Tooltip("Probe position relative to the target. Low down so floor reflections look up at the ceiling.")]
    [SerializeField] Vector3 offset = new Vector3(0f, -0.5f, 0f);
    [SerializeField] float refreshInterval = 0.2f;
    [Tooltip("Horizontal size of the area around the player that gets reflections")]
    [SerializeField] float reach = 14f;
    [Tooltip("Used when the level has no MazeGenerator to read the floor and ceiling from")]
    [SerializeField] float fallbackFloorY = 0f;
    [SerializeField] float fallbackCeilingHeight = 3f;

    ReflectionProbe probe;
    int renderId = -1;
    float nextRefresh;
    float floorY;
    float ceilingHeight;

    void Awake()
    {
        probe = GetComponent<ReflectionProbe>();
        probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
        probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting;
        probe.boxProjection = true;
        if (target == null) target = transform.parent;

        MazeGenerator maze = FindAnyObjectByType<MazeGenerator>();
        floorY = maze != null ? maze.transform.position.y : fallbackFloorY;
        ceilingHeight = maze != null ? maze.WallHeight : fallbackCeilingHeight;
    }

    void LateUpdate()
    {
        if (target != null) transform.SetPositionAndRotation(target.position + offset, Quaternion.identity);

        
        probe.size = new Vector3(reach, ceilingHeight, reach);
        probe.center = new Vector3(0f, floorY + ceilingHeight * 0.5f - transform.position.y, 0f);

        bool previousDone = renderId < 0 || probe.IsFinishedRendering(renderId);
        if (Time.unscaledTime >= nextRefresh && previousDone)
        {
            renderId = probe.RenderProbe();
            nextRefresh = Time.unscaledTime + refreshInterval;
        }
    }
}
