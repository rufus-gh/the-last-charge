using System.Collections.Generic;
using UnityEngine;


public class MeleeWeapon : Weapon
{
    [Header("References")]
    [SerializeField] Transform model;

    [Header("Hit")]
    [SerializeField] float damage = 50f;
    [SerializeField] float range = 2.4f;
    [SerializeField] float hitRadius = 0.45f;
    [SerializeField] float impactForce = 4f;
    [SerializeField] LayerMask hitMask = ~0;

    [Header("Timing")]
    [SerializeField] float swingDuration = 0.5f;
    [Tooltip("Time between swings while the button is held")]
    [SerializeField] float swingCooldown = 0.65f;
    [Range(0f, 1f)]
    [Tooltip("Point in the swing where the hit lands")]
    [SerializeField] float hitMoment = 0.45f;

    [Header("Swing Pose (relative to rest)")]
    [SerializeField] Vector3 windupEuler = new Vector3(-40f, 0f, 10f);
    [SerializeField] Vector3 strikeEuler = new Vector3(85f, 0f, 15f);
    [SerializeField] Vector3 strikeOffset = new Vector3(-0.18f, 0.05f, 0.05f);

    [Header("Sound")]
    [Tooltip("File names in Resources/Sfx, one picked at random per swing")]
    [SerializeField] string[] swingSounds = { "knife-slash-1", "knife-slash-2" };
    [Tooltip("Lower for a heavier weapon (the axe is slowed down by name)")]
    [SerializeField] float swingPitch = 1f;

    Vector3 restPosition;
    Quaternion restRotation;
    float swingStart = float.NegativeInfinity;
    float nextSwingTime;
    bool hitApplied = true;
    readonly HashSet<EnemyHealth> hitThisSwing = new HashSet<EnemyHealth>();

    void Awake()
    {
        if (DisplayName.ToLowerInvariant().Contains("axe")) swingPitch = Mathf.Min(swingPitch, 0.75f);
        if (model != null)
        {
            restPosition = model.localPosition;
            restRotation = model.localRotation;
        }
    }

    public override void HandleInput(WeaponInput input)
    {
        if (input.FireHeld && Time.time >= nextSwingTime) StartSwing();
    }

    public override void OnUnequip()
    {
        swingStart = float.NegativeInfinity;
        hitApplied = true;
        if (model != null) model.SetLocalPositionAndRotation(restPosition, restRotation);
    }

    void StartSwing()
    {
        swingStart = Time.time;
        nextSwingTime = Time.time + swingCooldown;
        hitApplied = false;
        if (swingSounds != null && swingSounds.Length > 0)
            Sfx.Play(swingSounds[Random.Range(0, swingSounds.Length)], 0.9f, swingPitch * Random.Range(0.95f, 1.05f));
    }

    void Update()
    {
        float t = (Time.time - swingStart) / swingDuration;
        if (t >= 0f && t < 1f)
        {
            Pose(t);
            if (!hitApplied && t >= hitMoment)
            {
                hitApplied = true;
                ApplyHit();
            }
        }
        else if (model != null)
        {
            model.SetLocalPositionAndRotation(restPosition, restRotation);
        }
    }

    
    void Pose(float t)
    {
        if (model == null) return;

        Quaternion windup = restRotation * Quaternion.Euler(windupEuler);
        Quaternion strike = restRotation * Quaternion.Euler(strikeEuler);
        Vector3 strikePosition = restPosition + strikeOffset;

        if (t < 0.3f)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / 0.3f);
            model.SetLocalPositionAndRotation(restPosition, Quaternion.Slerp(restRotation, windup, k));
        }
        else if (t < 0.55f)
        {
            float k = Mathf.SmoothStep(0f, 1f, (t - 0.3f) / 0.25f);
            model.SetLocalPositionAndRotation(Vector3.Lerp(restPosition, strikePosition, k), Quaternion.Slerp(windup, strike, k));
        }
        else
        {
            float k = Mathf.SmoothStep(0f, 1f, (t - 0.55f) / 0.45f);
            model.SetLocalPositionAndRotation(Vector3.Lerp(strikePosition, restPosition, k), Quaternion.Slerp(strike, restRotation, k));
        }
    }

    void ApplyHit()
    {
        Transform cam = ViewCamera.transform;
        hitThisSwing.Clear();
        RaycastHit[] hits = Physics.SphereCastAll(cam.position, hitRadius, cam.forward, range, hitMask, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit hit in hits)
        {
            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemy != null && hitThisSwing.Add(enemy)) enemy.TakeDamage(damage);

            if (hit.rigidbody != null) hit.rigidbody.AddForce(cam.forward * impactForce, ForceMode.Impulse);
        }
    }
}
