using UnityEngine;



[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class TiledSurface : MonoBehaviour
{
    [Tooltip("Size of the visible face in metres (length, height) or (width, depth) for floors")]
    public Vector2 sizeMeters = Vector2.one;

    static readonly int BaseMapStId = Shader.PropertyToID("_BaseMap_ST");

    void OnEnable() => Apply();

    void OnValidate() => Apply();

    public void Apply()
    {
        var r = GetComponent<Renderer>();
        Material mat = r.sharedMaterial;
        if (mat == null || !mat.HasProperty(BaseMapStId)) return;

        Vector4 st = mat.GetVector(BaseMapStId);
        var block = new MaterialPropertyBlock();
        r.GetPropertyBlock(block);
        block.SetVector(BaseMapStId, new Vector4(st.x * sizeMeters.x, st.y * sizeMeters.y, st.z, st.w));
        r.SetPropertyBlock(block);
    }
}
