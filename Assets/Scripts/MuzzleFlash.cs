using UnityEngine;


public class MuzzleFlash : MonoBehaviour
{
    [SerializeField] Transform visual;
    [SerializeField] Light flashLight;
    [SerializeField] float duration = 0.05f;
    [SerializeField] Vector2 scaleRange = new Vector2(0.8f, 1.25f);
    [SerializeField] Vector2 lightIntensityRange = new Vector2(4f, 8f);

    Vector3 baseScale;
    float hideTime;
    bool showing;

    void Awake()
    {
        if (visual != null) baseScale = visual.localScale;
        Hide();
    }

    public void Play()
    {
        if (visual != null)
        {
            visual.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            visual.localScale = baseScale * Random.Range(scaleRange.x, scaleRange.y);
            visual.gameObject.SetActive(true);
        }
        if (flashLight != null)
        {
            flashLight.intensity = Random.Range(lightIntensityRange.x, lightIntensityRange.y);
            flashLight.enabled = true;
        }
        hideTime = Time.time + duration;
        showing = true;
    }

    void Update()
    {
        if (showing && Time.time >= hideTime) Hide();
    }

    void Hide()
    {
        if (visual != null) visual.gameObject.SetActive(false);
        if (flashLight != null) flashLight.enabled = false;
        showing = false;
    }
}
