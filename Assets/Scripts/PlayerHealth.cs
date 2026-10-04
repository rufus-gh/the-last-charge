using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] float maxHealth = 100f;
    [SerializeField] Transform spawnPoint;
    [Tooltip("Seconds of invulnerability after respawning")]
    [SerializeField] float respawnGracePeriod = 1.5f;

    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }
    public bool IsInvulnerable => forcedInvulnerable || Time.time < invulnerableUntil;

    
    public void SetInvulnerable(bool on) => forcedInvulnerable = on;
    public bool IsDead => CurrentHealth <= 0f;

    
    public event Action<float, float> HealthChanged;
    
    public event Action<float> Damaged;
    
    public event Action Died;

    CharacterController controller;
    float invulnerableUntil;
    bool forcedInvulnerable;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        CurrentHealth = maxHealth;
    }

    void Start() => HealthChanged?.Invoke(CurrentHealth, maxHealth);

    public void TakeDamage(float amount)
    {
        if (IsDead || IsInvulnerable) return;

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0f);
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
        Damaged?.Invoke(amount);

        if (IsDead) Died?.Invoke();
    }

    public void SetHealth(float value)
    {
        CurrentHealth = Mathf.Clamp(value, 1f, maxHealth);
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    
    public void AddMaxHealth(float amount)
    {
        maxHealth += amount;
        CurrentHealth += amount;
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void Respawn()
    {
        if (spawnPoint != null)
        {
            
            controller.enabled = false;
            transform.SetPositionAndRotation(spawnPoint.position, Quaternion.Euler(0f, spawnPoint.eulerAngles.y, 0f));
            controller.enabled = true;
        }

        CurrentHealth = maxHealth;
        invulnerableUntil = Time.time + respawnGracePeriod;
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}
