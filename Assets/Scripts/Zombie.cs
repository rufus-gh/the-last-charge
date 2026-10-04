using UnityEngine;
using UnityEngine.AI;




[RequireComponent(typeof(NavMeshAgent))]
public class Zombie : MonoBehaviour
{
    [Header("Chase")]
    [SerializeField] float moveSpeed = 3f;
    [Tooltip("How often the path to the player is recalculated")]
    [SerializeField] float repathInterval = 0.2f;

    [Header("Attack")]
    [SerializeField] float attackRange = 1.6f;
    [SerializeField] float attackDamage = 15f;
    [SerializeField] float attackCooldown = 1f;
    [Tooltip("Seconds into the swing when the claws land")]
    [SerializeField] float attackHitDelay = 0.45f;

    [Header("Look")]
    [SerializeField] Animator animator;
    [Tooltip("One is picked at random for each zombie")]
    [SerializeField] Material[] skins;
    [Tooltip("How long the body lies there after dying")]
    [SerializeField] float deathLinger = 4.5f;

    [Header("Feedback")]
    [SerializeField] Color attackFlashColor = Color.red;
    [SerializeField] float attackFlashDuration = 0.15f;
    [Tooltip("Brighter than white, so the textured skin visibly flashes")]
    [SerializeField] Color hitFlashColor = new Color(2.2f, 2.2f, 2.2f);

    NavMeshAgent agent;
    PlayerHealth target;
    float nextRepathTime;
    float nextAttackTime;
    float attackLandsAt = -1f;
    float nextHitFlinch;
    float flashUntil;
    Color flashColor;
    bool flashing;
    EnemyHealth health;
    Renderer[] renderers;
    MaterialPropertyBlock propertyBlock;
    ZombieVoice voice;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int SpeedId = Animator.StringToHash("Speed");
    static readonly int HitId = Animator.StringToHash("Hit");
    static readonly int Attack1Id = Animator.StringToHash("Attack1");
    static readonly int Attack2Id = Animator.StringToHash("Attack2");
    static readonly int Death1Id = Animator.StringToHash("Death1");
    static readonly int Death2Id = Animator.StringToHash("Death2");

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange * 0.8f;

        if (animator == null) animator = GetComponentInChildren<Animator>();
        renderers = GetComponentsInChildren<Renderer>();
        if (skins != null && skins.Length > 0)
        {
            Material skin = skins[Random.Range(0, skins.Length)];
            foreach (Renderer r in renderers)
                if (r is SkinnedMeshRenderer) r.sharedMaterial = skin;
        }
        propertyBlock = new MaterialPropertyBlock();
        voice = ZombieVoice.Add(gameObject);

        health = GetComponent<EnemyHealth>();
        if (health != null) health.SetDestroyDelay(deathLinger);
    }

    void OnEnable()
    {
        if (health == null) return;
        health.Damaged += OnDamaged;
        health.Died += OnDied;
    }

    void OnDisable()
    {
        if (health == null) return;
        health.Damaged -= OnDamaged;
        health.Died -= OnDied;
    }

    void OnDamaged()
    {
        Flash(hitFlashColor);
        if (animator != null && !health.IsDead && Time.time >= nextHitFlinch)
        {
            animator.SetTrigger(HitId);
            nextHitFlinch = Time.time + 0.8f;
            voice.Snarl();
        }
    }

    
    void OnDied()
    {
        agent.enabled = false;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
        FloatingHealthBar bar = GetComponentInChildren<FloatingHealthBar>();
        if (bar != null) bar.gameObject.SetActive(false);
        ClearFlash();
        if (animator != null) animator.SetTrigger(Random.value < 0.5f ? Death1Id : Death2Id);
        
        voice.Groan();
        voice.enabled = false;
        enabled = false;
    }

    void Flash(Color color)
    {
        flashColor = color;
        flashUntil = Time.time + attackFlashDuration;
    }

    void Start() => target = FindAnyObjectByType<PlayerHealth>();

    void Update()
    {
        if (target == null || !agent.isOnNavMesh) return;
        agent.speed = moveSpeed;

        if (Time.time >= nextRepathTime)
        {
            agent.SetDestination(target.transform.position);
            nextRepathTime = Time.time + repathInterval;
        }

        Vector3 toTarget = target.transform.position - transform.position;
        toTarget.y = 0f;

        
        if (toTarget.sqrMagnitude > 0.001f && toTarget.magnitude <= agent.stoppingDistance + 0.5f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toTarget), 10f * Time.deltaTime);

        if (toTarget.magnitude <= attackRange && Time.time >= nextAttackTime)
            StartAttack();

        if (attackLandsAt > 0f && Time.time >= attackLandsAt)
        {
            attackLandsAt = -1f;
            
            if (toTarget.magnitude <= attackRange * 1.4f && !target.IsInvulnerable)
            {
                target.TakeDamage(attackDamage);
                Flash(attackFlashColor);
            }
        }

        if (animator != null) animator.SetFloat(SpeedId, agent.velocity.magnitude, 0.1f, Time.deltaTime);
        UpdateFlash();
    }

    void StartAttack()
    {
        nextAttackTime = Time.time + attackCooldown;
        attackLandsAt = Time.time + attackHitDelay;
        voice.Snarl();
        if (animator != null) animator.SetTrigger(Random.value < 0.5f ? Attack1Id : Attack2Id);
    }

    
    void UpdateFlash()
    {
        bool active = Time.time < flashUntil;
        if (active)
        {
            propertyBlock.SetColor(BaseColorId, flashColor);
            foreach (Renderer r in renderers) r.SetPropertyBlock(propertyBlock);
            flashing = true;
        }
        else if (flashing)
        {
            ClearFlash();
        }
    }

    void ClearFlash()
    {
        flashing = false;
        foreach (Renderer r in renderers) r.SetPropertyBlock(null);
    }
}
