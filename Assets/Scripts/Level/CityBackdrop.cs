using UnityEngine;



[RequireComponent(typeof(MazeGenerator))]
public class CityBackdrop : MonoBehaviour
{
    [SerializeField] int ringCount = 3;
    [SerializeField] float firstRingGap = 35f;
    [SerializeField] float ringSpacing = 60f;
    [SerializeField] Vector2 buildingWidth = new Vector2(14f, 34f);
    [Tooltip("Building heights above the street in the nearest ring; each ring further out is taller, " +
             "so a high floor looks over the rooftops to a skyline")]
    [SerializeField] Vector2 nearHeight = new Vector2(20f, 60f);
    [SerializeField] Vector2 heightGrowthPerRing = new Vector2(15f, 45f);

    public void Build(MazeGenerator maze)
    {
        Transform parent = maze.ResetGeneratedChild("City");
        LevelTheme theme = maze.Theme;
        if (theme == null || !theme.outdoors) return;

        var rng = new System.Random(maze.Seed + 7);
        Vector2 size = (Vector2)maze.GridSize * maze.CellSize;
        Vector3 center = new Vector3(size.x / 2f, 0f, size.y / 2f);
        float street = -theme.cityElevation;

        
        float groundSize = Mathf.Max(size.x, size.y) + 2f * (firstRingGap + ringCount * ringSpacing + 60f);
        Block(parent, "Ground", center + Vector3.up * (street - 0.5f), new Vector3(groundSize, 1f, groundSize), theme.cityGroundMaterial);
        
        Block(parent, "Building Below", center + Vector3.up * (street / 2f - 0.15f), new Vector3(size.x + 0.4f, -street - 0.3f, size.y + 0.4f), theme.cityFacadeMaterial);

        for (int ring = 0; ring < ringCount; ring++)
        {
            float inset = firstRingGap + ring * ringSpacing;
            Vector2 half = size / 2f + Vector2.one * inset;
            
            for (int side = 0; side < 4; side++)
            {
                float length = (side % 2 == 0 ? half.x : half.y) * 2f;
                for (float t = 0f; t < length;)
                {
                    float w = Range(rng, buildingWidth.x, buildingWidth.y);
                    float d = Range(rng, buildingWidth.x, buildingWidth.y);
                    float top = street + Range(rng, nearHeight.x + ring * heightGrowthPerRing.x, nearHeight.y + ring * heightGrowthPerRing.y);
                    float along = t + w / 2f - length / 2f;
                    Vector3 p = side switch
                    {
                        0 => new Vector3(along, 0f, -half.y),
                        1 => new Vector3(half.x, 0f, along),
                        2 => new Vector3(-along, 0f, half.y),
                        _ => new Vector3(-half.x, 0f, -along),
                    };
                    float height = top - street;
                    Block(parent, "Building", center + p + Vector3.up * (street + height / 2f),
                        side % 2 == 0 ? new Vector3(w, height, d) : new Vector3(d, height, w), theme.cityFacadeMaterial);
                    t += w + Range(rng, 4f, 14f);
                }
            }
        }
    }

    void Block(Transform parent, string name, Vector3 localPos, Vector3 size, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.isStatic = true;
        
        MazeGenerator.DestroySafe(go.GetComponent<Collider>());
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = size;
        var r = go.GetComponent<Renderer>();
        if (mat != null) r.sharedMaterial = mat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        
        go.AddComponent<TiledSurface>().sizeMeters = new Vector2(Mathf.Max(size.x, size.z), size.y);
    }

    static float Range(System.Random rng, float min, float max) => min + (float)rng.NextDouble() * (max - min);
}
