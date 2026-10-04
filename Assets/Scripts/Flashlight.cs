using System;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Light))]
public class Flashlight : MonoBehaviour
{
    [SerializeField] bool startOn = true;

    [Header("Battery")]
    [SerializeField] float maxBattery = 100f;
    [SerializeField] float startBattery = 100f;
    [Tooltip("Battery used per second while the light is on")]
    [SerializeField] float drainPerSecond = 1.2f;

    [Header("Low Battery")]
    [Range(0f, 1f)]
    [Tooltip("Below this fraction the light dims and sputters")]
    [SerializeField] float lowBatteryThreshold = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] float minLowBrightness = 0.25f;
    [SerializeField] float sputterRate = 3f;

    public float Battery { get; private set; }
    public float MaxBattery => maxBattery;
    public float Charge01 => maxBattery > 0f ? Battery / maxBattery : 0f;
    public bool IsOn => lightSource.enabled;

    
    public void SetLocked(bool locked)
    {
        this.locked = locked;
        if (locked && lightSource != null) lightSource.enabled = false;
    }
    public bool IsFull => Battery >= maxBattery;
    public bool IsLow => Charge01 < lowBatteryThreshold;

    
    public event Action<float> BatteryAdded;

    Light lightSource;
    float baseIntensity;
    float sputterUntil;
    bool locked;

    void Awake()
    {
        lightSource = GetComponent<Light>();
        baseIntensity = lightSource.intensity;
        Battery = Mathf.Clamp(startBattery, 0f, maxBattery);
        lightSource.enabled = startOn && Battery > 0f;
    }

    void Update()
    {
        if (locked)
        {
            lightSource.enabled = false;
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.fKey.wasPressedThisFrame)
            lightSource.enabled = !lightSource.enabled && Battery > 0f;

        if (!lightSource.enabled) return;

        float drain = drainPerSecond;
        Battery = Mathf.Max(Battery - drain * Time.deltaTime, 0f);
        if (Battery <= 0f)
        {
            lightSource.enabled = false;
            return;
        }

        float brightness = GetBrightness();
        lightSource.intensity = baseIntensity * brightness;
        
        lightSource.shadowStrength = Mathf.Lerp(0.45f, 0.9f, brightness);
    }

    float GetBrightness()
    {
        if (!IsLow) return 1f;

        
        float t = Charge01 / lowBatteryThreshold;
        float brightness = Mathf.Lerp(minLowBrightness, 1f, t);
        if (Time.time < sputterUntil) return brightness * 0.15f;
        if (UnityEngine.Random.value < sputterRate * (1f - t) * Time.deltaTime)
            sputterUntil = Time.time + UnityEngine.Random.Range(0.05f, 0.2f);
        return brightness;
    }

    
    public void AddMaxBattery(float amount, bool recharge = true)
    {
        maxBattery += amount;
        if (recharge) AddBattery(maxBattery);
    }

    
    public void SetBattery(float value)
    {
        Battery = Mathf.Clamp(value, 0f, maxBattery);
        lightSource.enabled = Battery > 0f;
    }

    public float AddBattery(float amount)
    {
        float added = Mathf.Min(amount, maxBattery - Battery);
        if (added <= 0f) return 0f;

        
        bool wasEmpty = Battery <= 0f;
        Battery += added;
        if (wasEmpty) lightSource.enabled = true;
        BatteryAdded?.Invoke(added);
        return added;
    }
}
