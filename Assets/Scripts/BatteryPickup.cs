using UnityEngine;


[RequireComponent(typeof(Collider))]
public class BatteryPickup : MonoBehaviour
{
    [SerializeField] float chargeAmount = 30f;
    [SerializeField] Transform visual;

    [Header("Idle Motion")]
    [SerializeField] float bobHeight = 0.08f;
    [SerializeField] float bobSpeed = 2f;
    [SerializeField] float spinSpeed = 90f;

    Vector3 visualStartPosition;
    float bobOffset;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        if (visual != null) visualStartPosition = visual.localPosition;
        bobOffset = Random.value * Mathf.PI * 2f;
    }

    void Update()
    {
        if (visual == null) return;
        visual.localPosition = visualStartPosition + Vector3.up * Mathf.Sin(Time.time * bobSpeed + bobOffset) * bobHeight;
        visual.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        Flashlight flashlight = other.GetComponentInChildren<Flashlight>();
        if (flashlight == null || flashlight.IsFull) return;

        flashlight.AddBattery(chargeAmount);
        Destroy(gameObject);
    }
}
