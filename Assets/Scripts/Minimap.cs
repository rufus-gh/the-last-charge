using UnityEngine;
using UnityEngine.UI;



public class Minimap : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform player;
    [Tooltip("Rotates the map around the minimap centre")]
    [SerializeField] RectTransform rotator;
    [SerializeField] RawImage mapImage;
    [Tooltip("Player arrow, only rotated when the map itself doesn't rotate")]
    [SerializeField] RectTransform playerMarker;

    [Header("Scanning")]
    [SerializeField] Vector3 worldCenter = Vector3.zero;
    [SerializeField] Vector2 worldSize = new Vector2(48f, 48f);
    [SerializeField] float pixelsPerMeter = 4f;
    [SerializeField] LayerMask wallLayers = ~0;
    [Tooltip("Height walls are sampled at (above the floor, below the ceiling)")]
    [SerializeField] float scanHeight = 1.5f;
    [SerializeField] Color wallColor = new Color(0.8f, 0.72f, 0.62f, 0.95f);
    [SerializeField] Color floorColor = new Color(0f, 0f, 0f, 0f);
    [Tooltip("Size the scan area to the level's MazeGenerator (and scan just under its ceiling) instead of the values above")]
    [SerializeField] bool fitToMaze = true;

    [Header("View")]
    [Tooltip("Metres visible from the centre to the edge of the minimap")]
    [SerializeField] float viewRadius = 10f;
    [SerializeField] bool rotateWithPlayer = true;

    Texture2D texture;
    RectTransform mapRect;
    float unitsPerMeter;

    void Start()
    {
        if (player == null)
        {
            PlayerHealth p = FindAnyObjectByType<PlayerHealth>();
            if (p != null) player = p.transform;
        }
        if (fitToMaze) FitToMaze();
        Rebake();
    }

    void FitToMaze()
    {
        MazeGenerator maze = FindAnyObjectByType<MazeGenerator>();
        if (maze == null || !maze.IsGenerated) return;
        var size = new Vector3(maze.GridSize.x * maze.CellSize, 0f, maze.GridSize.y * maze.CellSize);
        worldCenter = maze.transform.TransformPoint(size * 0.5f);
        worldSize = new Vector2(size.x + 2f, size.z + 2f);
        
        scanHeight = maze.transform.position.y + maze.WallHeight - 0.35f;
    }

    public void Rebake()
    {
        int width = Mathf.CeilToInt(worldSize.x * pixelsPerMeter);
        int height = Mathf.CeilToInt(worldSize.y * pixelsPerMeter);
        if (texture == null || texture.width != width || texture.height != height)
        {
            if (texture != null) Destroy(texture);
            texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "Minimap",
            };
        }

        float cell = 1f / pixelsPerMeter;
        Vector3 halfExtents = new Vector3(cell * 0.5f, 0.25f, cell * 0.5f);
        Vector3 origin = worldCenter - new Vector3(worldSize.x * 0.5f, 0f, worldSize.y * 0.5f);
        Color32 wall = wallColor, floor = floorColor;
        var pixels = new Color32[width * height];

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            Vector3 p = origin + new Vector3((x + 0.5f) * cell, scanHeight, (y + 0.5f) * cell);
            bool isWall = Physics.CheckBox(p, halfExtents, Quaternion.identity, wallLayers, QueryTriggerInteraction.Ignore);
            pixels[y * width + x] = isWall ? wall : floor;
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        mapImage.texture = texture;

        
        mapRect = mapImage.rectTransform;
        float minimapSize = ((RectTransform)rotator.parent).rect.width;
        unitsPerMeter = minimapSize * 0.5f / viewRadius;
        mapRect.sizeDelta = worldSize * unitsPerMeter;
    }

    void LateUpdate()
    {
        if (player == null || mapRect == null) return;

        Vector3 offset = player.position - worldCenter;
        mapRect.anchoredPosition = new Vector2(-offset.x, -offset.z) * unitsPerMeter;

        float yaw = player.eulerAngles.y;
        if (rotateWithPlayer)
        {
            rotator.localEulerAngles = new Vector3(0f, 0f, yaw);
            if (playerMarker != null) playerMarker.localEulerAngles = Vector3.zero;
        }
        else
        {
            rotator.localEulerAngles = Vector3.zero;
            if (playerMarker != null) playerMarker.localEulerAngles = new Vector3(0f, 0f, -yaw);
        }
    }

    void OnDestroy()
    {
        if (texture != null) Destroy(texture);
    }
}
