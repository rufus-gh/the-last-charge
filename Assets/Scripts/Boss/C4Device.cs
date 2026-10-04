using UnityEngine;





public class C4Device : MonoBehaviour
{
    public enum Pose { Hidden, Held, Tucked, Raised }

    [SerializeField] TextMesh display;
    [SerializeField] Renderer redLed;
    [SerializeField] Renderer[] greenLeds;
    [SerializeField] Transform model;

    [Header("Poses (relative to the camera). Euler = tilt about the long side, turn, roll")]
    [SerializeField] Vector3 heldPosition = new Vector3(0.02f, -0.17f, 0.42f);
    [SerializeField] Vector3 heldEuler = new Vector3(-35f, 90f, 0f);
    [SerializeField] Vector3 tuckedPosition = new Vector3(0.2f, -0.34f, 0.32f);
    [SerializeField] Vector3 tuckedEuler = new Vector3(-12f, 100f, 10f);
    [Tooltip("Lifted up close once it is armed, so the countdown on its screen is easy to read")]
    [SerializeField] Vector3 raisedPosition = new Vector3(0.03f, -0.07f, 0.33f);
    [SerializeField] Vector3 raisedEuler = new Vector3(-50f, 90f, 0f);
    [SerializeField] Vector3 hiddenPosition = new Vector3(0.02f, -0.75f, 0.4f);
    [SerializeField] float poseSpeed = 4f;

    Pose pose = Pose.Hidden;
    bool armed;
    float armedAt;
    float pressStart = float.NegativeInfinity;
    MaterialPropertyBlock block;

    static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
    static readonly Color RedOn = new Color(0.9f, 0.05f, 0.03f);

    void Awake()
    {
        block = new MaterialPropertyBlock();
        transform.localPosition = hiddenPosition;
        transform.localRotation = Rotation(heldEuler);
        SetRed(0.15f);
    }

    void Update()
    {
        
        GetPose(out Vector3 position, out Quaternion rotation);
        if (pose == Pose.Held || pose == Pose.Raised)
        {
            position += new Vector3(Mathf.Sin(Time.time * 0.9f) * 0.003f, Mathf.Sin(Time.time * 1.3f) * 0.004f, 0f);
        }
        float press = Mathf.Clamp01((Time.time - pressStart) / 0.35f);
        if (press < 1f) position += new Vector3(0f, -0.012f, 0.04f) * Mathf.Sin(press * Mathf.PI);

        float k = 1f - Mathf.Exp(-poseSpeed * Time.deltaTime);
        transform.localPosition = Vector3.Lerp(transform.localPosition, position, k);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, rotation, k);

        UpdateLed();
    }

    void GetPose(out Vector3 position, out Quaternion rotation)
    {
        switch (pose)
        {
            case Pose.Held: position = heldPosition; rotation = Rotation(heldEuler); break;
            case Pose.Tucked: position = tuckedPosition; rotation = Rotation(tuckedEuler); break;
            case Pose.Raised: position = raisedPosition; rotation = Rotation(raisedEuler); break;
            default: position = hiddenPosition; rotation = Rotation(heldEuler); break;
        }
    }

    
    static Quaternion Rotation(Vector3 e) => Quaternion.Euler(e.x, 0f, 0f) * Quaternion.Euler(0f, e.y, 0f) * Quaternion.Euler(0f, 0f, e.z);

    public void SetPose(Pose next) => pose = next;

    
    public void SnapTo(Pose next)
    {
        pose = next;
        GetPose(out Vector3 position, out Quaternion rotation);
        transform.localPosition = position;
        transform.localRotation = rotation;
    }

    public void ShowText(string text)
    {
        if (display != null) display.text = text;
    }

    
    public void Arm()
    {
        armed = true;
        armedAt = Time.time;
    }

    
    public void Press() => pressStart = Time.time;

    
    void UpdateLed()
    {
        float level = 0.15f;
        if (armed) level = 0.1f + 0.9f * Mathf.Pow(1f - Mathf.Repeat(Time.time - armedAt, 1f), 3f);
        SetRed(level);
    }

    void SetRed(float level)
    {
        if (redLed == null) return;
        redLed.GetPropertyBlock(block);
        block.SetColor(EmissionId, RedOn * (level * 2.2f));
        redLed.SetPropertyBlock(block);
    }
}
