using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] float maxHealth = 100f;
    [Tooltip("How long the body stays after dying (time for a death animation)")]
    [SerializeField] float destroyDelay;

    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0f;

    
    public event Action<float, float> HealthChanged;
    public event Action Damaged;
    public event Action Died;
    
    public static event Action<EnemyHealth> AnyDied;

    void Awake() => CurrentHealth = maxHealth;

    public void SetDestroyDelay(float seconds) => destroyDelay = seconds;

    void Start() => HealthChanged?.Invoke(CurrentHealth, maxHealth);

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0f);
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
        Damaged?.Invoke();

        if (IsDead)
        {
            Died?.Invoke();
            AnyDied?.Invoke(this);
            Destroy(gameObject, destroyDelay);
        }
    }
}
