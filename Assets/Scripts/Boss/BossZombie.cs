using System;
using UnityEngine;
using UnityEngine.AI;






[RequireComponent(typeof(NavMeshAgent), typeof(EnemyHealth))]
public class BossZombie : MonoBehaviour
{
    enum State { Guard, Chase, WindUp, Return }

    [SerializeField] string displayName = "PATIENT ZERO";

    [Header("Guarding")]
    [SerializeField] float aggroRange = 12f;
    [Tooltip("Always notices the player this close, even without line of sight")]
    [SerializeField] float senseRange = 5f;
    [Tooltip("Gives up and walks back when the player gets this far from its post")]
    [SerializeField] float leashRange = 32f;
    [SerializeField] LayerMask sightBlockers = ~0;

    [Header("Arena")]
    [Tooltip("Room the fight happens in (world centre and size). Zero size = no arena: guard by distance instead.")]
    [SerializeField] Vector3 arenaCenter;
    [SerializeField] Vector3 arenaSize;
    [Tooltip("How far past the arena walls it still chases (through the doorways)")]
    [SerializeField] float arenaLeash = 4f;

    [Header("Movement")]
    [SerializeField] float walkSpeed = 2f;
    [SerializeField] float chaseSpeed = 3.3f;
    [SerializeField] float enragedSpeed = 4.4f;
    [Tooltip("Enrages below this fraction of its health")]
    [Range(0f, 1f)] [SerializeField] float enrageAt = 0.4f;
    [SerializeField] float repathInterval = 0.2f;

    [Header("Attack")]
    [SerializeField] float attackRange = 2.5f;
    [Tooltip("Seconds of rearing back before the swipe lands")]
    [SerializeField] float windUpTime = 0.6f;
    [SerializeField] float attackDamage = 32f;
    [SerializeField] float attackCooldown = 1.1f;

    [Header("Look")]
    [Tooltip("The zombie model's animator: walks and runs with the agent, swings on the wind-up, roars when it enrages, falls when it dies")]
    [SerializeField] Animator animator;
    [Tooltip("Optional: leans forward for the swipe and bobs while walking, for a model with no animations")]
    [SerializeField] Transform body;
    [SerializeField] Renderer[] skin;
    [SerializeField] Light eyeLight;
    [SerializeField] Color windUpColor = new Color(1f, 0.15f, 0.1f);
    [SerializeField] Color hitColor = Color.white;

    public string DisplayName => displayName;
    public EnemyHealth Health { get; private set; }
    public bool Engaged { get; private set; }
    public bool Enraged { get; private set; }
    
    public event Action Engage;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int SpeedId = Animator.StringToHash("Speed");
    static readonly int AttackId = Animator.StringToHash("Attack1");
    static readonly int RoarId = Animator.StringToHash("Roar");
    static readonly int DeathId = Animator.StringToHash("Death1");

    bool HasArena => arenaSize.x > 0f && arenaSize.z > 0f;

    NavMeshAgent agent;
    PlayerHealth target;
    State state;
    Vector3 post;
    Quaternion postRotation;
    float nextRepath;
    float windUpEnd;
    float nextAttack;
    float hitFlashUntil;
    float baseEyeIntensity;
    Color[] skinColors;
    MaterialPropertyBlock block;
    Quaternion bodyRest;
    ZombieVoice voice;

    bool announced;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        Health = GetComponent<EnemyHealth>();
        Health.SetDestroyDelay(5f);
        
        voice = ZombieVoice.Add(gameObject, 0.6f, 1f);
        if (animator == null) animator = GetComponentInChildren<Animator>();
        block = new MaterialPropertyBlock();
        skinColors = new Color[skin.Length];
        for (int i = 0; i < skin.Length; i++) skinColors[i] = skin[i].sharedMaterial.GetColor(BaseColorId);
        if (body != null) bodyRest = body.localRotation;
        if (eyeLight != null) baseEyeIntensity = eyeLight.intensity;
    }

    public void SetArena(Vector3 center, Vector3 size)
    {
        arenaCenter = center;
        arenaSize = size;
    }

    bool InArena(Vector3 p)
    {
        Vector3 half = arenaSize / 2f + new Vector3(arenaLeash, 0f, arenaLeash);
        return Mathf.Abs(p.x - arenaCenter.x) <= half.x && Mathf.Abs(p.z - arenaCenter.z) <= half.z;
    }

    void OnEnable()
    {
        var health = GetComponent<EnemyHealth>();
        health.Damaged += OnDamaged;
        health.Died += OnDied;
    }

    void OnDisable()
    {
        var health = GetComponent<EnemyHealth>();
        health.Damaged -= OnDamaged;
        health.Died -= OnDied;
    }

    
    void OnDied()
    {
        agent.enabled = false;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
        if (eyeLight != null) eyeLight.enabled = false;
        if (animator != null) animator.SetTrigger(DeathId);
        voice.Groan();
        voice.enabled = false;
        enabled = false;
    }

    void Start()
    {
        target = FindAnyObjectByType<PlayerHealth>();
        post = transform.position;
        postRotation = transform.rotation;
        agent.speed = walkSpeed;
        agent.stoppingDistance = attackRange * 0.7f;
    }

    void OnDamaged()
    {
        hitFlashUntil = Time.time + 0.08f;
        
        bool reachable = !HasArena || (target != null && InArena(target.transform.position));
        if ((state == State.Guard || state == State.Return) && reachable) StartChase();
        if (!Enraged && Health.CurrentHealth <= Health.MaxHealth * enrageAt)
        {
            Enraged = true;
            if (eyeLight != null) baseEyeIntensity *= 2f;
            if (animator != null) animator.SetTrigger(RoarId);
            voice.Groan();
        }
    }

    void Update()
    {
        if (target == null || !agent.isOnNavMesh) return;
        Vector3 toPlayer = target.transform.position - transform.position;
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;
        float playerFromPost = Vector3.Distance(target.transform.position, post);

        switch (state)
        {
            case State.Guard:
                if (Vector3.Distance(transform.position, post) > 1f) agent.SetDestination(post);
                else transform.rotation = Quaternion.Slerp(transform.rotation, postRotation, 2f * Time.deltaTime);
                if (target.IsDead) break;
                if (HasArena ? InArena(target.transform.position) : distance < senseRange || (distance < aggroRange && CanSee())) StartChase();
                break;

            case State.Chase:
                if (target.IsDead || (HasArena ? !InArena(target.transform.position) : playerFromPost > leashRange))
                {
                    state = State.Return;
                    agent.speed = walkSpeed;
                    agent.SetDestination(post);
                    break;
                }
                if (Time.time >= nextRepath)
                {
                    agent.SetDestination(target.transform.position);
                    nextRepath = Time.time + repathInterval;
                }
                if (distance <= agent.stoppingDistance + 0.5f) Face(toPlayer, 8f);
                if (distance <= attackRange && Time.time >= nextAttack) StartWindUp();
                break;

            case State.WindUp:
                Face(toPlayer, 6f);
                if (Time.time >= windUpEnd) Swipe(distance);
                break;

            case State.Return:
                bool comeBack = HasArena ? InArena(target.transform.position) : distance < aggroRange && playerFromPost < leashRange * 0.8f;
                if (!target.IsDead && comeBack) StartChase();
                else if (!agent.pathPending && agent.remainingDistance < 1f) state = State.Guard;
                break;
        }
        Animate();
    }

    void StartChase()
    {
        
        if (!announced)
        {
            announced = true;
            LevelSubtitles.Show(LevelSubtitles.BossLines);
        }
        state = State.Chase;
        agent.isStopped = false;
        agent.speed = (Enraged ? enragedSpeed : chaseSpeed);
        if (Engaged) return;
        Engaged = true;
        Engage?.Invoke();
    }

    void StartWindUp()
    {
        state = State.WindUp;
        windUpEnd = Time.time + windUpTime;
        agent.isStopped = true;
        if (animator != null) animator.SetTrigger(AttackId);
        voice.Snarl();
    }

    void Swipe(float distance)
    {
        
        if (distance <= attackRange * 1.2f && !target.IsInvulnerable) target.TakeDamage(attackDamage);
        nextAttack = Time.time + attackCooldown * (Enraged ? 0.7f : 1f);
        agent.isStopped = false;
        state = State.Chase;
        agent.speed = (Enraged ? enragedSpeed : chaseSpeed);
    }

    bool CanSee()
    {
        Vector3 eye = transform.position + Vector3.up * 2.2f;
        Vector3 player = target.transform.position + Vector3.up * 0.6f;
        return !Physics.Linecast(eye, player, out RaycastHit hit, sightBlockers, QueryTriggerInteraction.Ignore)
               || hit.collider.GetComponentInParent<PlayerHealth>() != null;
    }

    void Face(Vector3 direction, float speed)
    {
        if (direction.sqrMagnitude < 0.001f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), speed * Time.deltaTime);
    }

    void Animate()
    {
        if (animator != null) animator.SetFloat(SpeedId, agent.velocity.magnitude, 0.1f, Time.deltaTime);
        bool windingUp = state == State.WindUp;
        float windUp = windingUp ? 1f - Mathf.Clamp01((windUpEnd - Time.time) / windUpTime) : 0f;

        if (body != null)
        {
            
            float speed01 = agent.velocity.magnitude / Mathf.Max(0.1f, enragedSpeed);
            float sway = Mathf.Sin(Time.time * 6f) * 4f * speed01;
            float lean = windingUp ? Mathf.Lerp(0f, -16f, windUp) : 6f * speed01;
            body.localRotation = bodyRest * Quaternion.Euler(lean, 0f, sway);
        }

        Color tint = Time.time < hitFlashUntil ? hitColor : Color.clear;
        for (int i = 0; i < skin.Length; i++)
        {
            Color c = tint != Color.clear ? tint : Color.Lerp(skinColors[i], windUpColor, windUp * 0.8f);
            skin[i].GetPropertyBlock(block);
            block.SetColor(BaseColorId, c);
            skin[i].SetPropertyBlock(block);
        }
        if (eyeLight != null) eyeLight.intensity = baseEyeIntensity * (1f + windUp * 2f);
    }
}
