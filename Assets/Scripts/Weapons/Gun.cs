using UnityEngine;


public class Gun : Weapon
{
    public enum FireMode { SemiAuto, FullAuto }

    [Header("References")]
    [SerializeField] Transform model;
    [SerializeField] MuzzleFlash muzzleFlash;

    [Header("Shooting")]
    [SerializeField] FireMode fireMode = FireMode.SemiAuto;
    [Tooltip("Damage per pellet")]
    [SerializeField] float damage = 25f;
    [SerializeField] int pelletsPerShot = 1;
    [Tooltip("Random spread cone in degrees when firing from the hip")]
    [SerializeField] float hipSpread = 0.5f;
    [SerializeField] float aimSpread = 0f;
    [SerializeField] float range = 100f;
    [Tooltip("Shots per second")]
    [SerializeField] float fireRate = 6f;
    [SerializeField] float impactForce = 5f;
    [SerializeField] LayerMask hitMask = ~0;

    [Header("Aiming")]
    [Tooltip("Model local position when aiming, lining the sights up with the screen centre")]
    [SerializeField] Vector3 aimPosition = new Vector3(0f, -0.076f, 0.4f);
    [SerializeField] float aimFieldOfView = 45f;
    [SerializeField] float aimSpeed = 7f;
    [Range(0f, 1f)]
    [SerializeField] float aimRecoilMultiplier = 0.4f;

    [Header("Ammo")]
    [SerializeField] int magazineSize = 12;
    [SerializeField] int reserveAmmo = 96;
    [SerializeField] float reloadTime = 1.5f;

    [Header("Sound")]
    [Tooltip("File name in Resources/Sfx. Empty = by weapon name (pistol, assault rifle, shotgun)")]
    [SerializeField] string fireSound;
    [SerializeField] string dryFireSound = "out-of-ammo";
    [SerializeField] string reloadSound = "reload";

    [Header("Feedback")]
    [SerializeField] float recoilKick = 0.08f;
    [SerializeField] float recoilRecoverSpeed = 12f;
    [SerializeField] float reloadTiltAngle = 40f;
    [SerializeField] Color impactColor = new Color(1f, 0.85f, 0.3f);

    public override bool UsesAmmo => true;
    public override int AmmoInMagazine => ammoInMagazine;
    public override int ReserveAmmo => reserveAmmo;
    public override int MagazineSize => magazineSize;
    public override bool IsReloading => isReloading;
    public bool IsAiming { get; private set; }
    
    public int FullAmmo { get { RememberFullAmmo(); return fullAmmo; } }

    int ammoInMagazine;
    int fullAmmo = -1;
    bool ammoSet;
    bool isReloading;
    float nextFireTime;
    float nextDryClick;
    AudioSource reloadSource;
    float reloadEndTime;
    float reloadDuration;
    Vector3 modelHipPosition;
    Quaternion modelRestRotation;
    Vector3 recoilOffset;
    float aimBlend;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    void Awake()
    {
        if (string.IsNullOrEmpty(fireSound)) fireSound = DefaultFireSound();
        RememberFullAmmo();
        if (!ammoSet) ammoInMagazine = magazineSize;
        if (model != null)
        {
            modelHipPosition = model.localPosition;
            modelRestRotation = model.localRotation;
        }
    }

    public override void HandleInput(WeaponInput input)
    {
        if (input.ReloadPressed) StartReload();

        bool trigger = fireMode == FireMode.FullAuto ? input.FireHeld : input.FirePressed;
        if (trigger) TryShoot();
        
        if (trigger && ammoInMagazine <= 0 && !isReloading && Time.time >= nextDryClick)
        {
            nextDryClick = Time.time + 0.45f;
            Sfx.Play(dryFireSound);
        }

        SetAiming(input.AimHeld);
    }

    void Update()
    {
        if (isReloading && Time.time >= reloadEndTime) FinishReload();
        UpdateAim();
        AnimateModel();
    }

    public override void OnUnequip()
    {
        
        isReloading = false;
        if (reloadSource != null) Destroy(reloadSource.gameObject);
        SetAiming(false);
        aimBlend = 0f;
        recoilOffset = Vector3.zero;
        if (ViewCamera != null) ViewCamera.fieldOfView = HipFieldOfView;
        if (model != null) model.SetLocalPositionAndRotation(modelHipPosition, modelRestRotation);
    }

    void TryShoot()
    {
        if (isReloading || ammoInMagazine <= 0 || Time.time < nextFireTime) return;

        ammoInMagazine--;
        nextFireTime = Time.time + 1f / fireRate;
        Sfx.Play(fireSound, 0.9f);
        if (muzzleFlash != null) muzzleFlash.Play();
        recoilOffset -= Vector3.forward * recoilKick * (IsAiming ? aimRecoilMultiplier : 1f);

        Transform cam = ViewCamera.transform;
        float spread = Mathf.Lerp(hipSpread, aimSpread, aimBlend);
        for (int i = 0; i < pelletsPerShot; i++)
        {
            Vector2 offset = Random.insideUnitCircle * Mathf.Tan(spread * Mathf.Deg2Rad);
            Vector3 direction = (cam.forward + cam.right * offset.x + cam.up * offset.y).normalized;
            FireRay(new Ray(cam.position, direction));
        }
    }

    void FireRay(Ray ray)
    {
        if (!Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore)) return;

        EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
        if (enemy != null) enemy.TakeDamage(damage);

        if (hit.rigidbody != null) hit.rigidbody.AddForceAtPosition(ray.direction * impactForce, hit.point, ForceMode.Impulse);

        SpawnImpact(hit.point);
    }

    void StartReload()
    {
        if (isReloading || ammoInMagazine >= magazineSize || reserveAmmo <= 0) return;
        isReloading = true;
        reloadDuration = reloadTime;
        reloadEndTime = Time.time + reloadDuration;

        
        AudioClip clip = Sfx.Clip(reloadSound);
        float pitch = clip != null ? Mathf.Clamp(clip.length / reloadDuration, 1f, 1.6f) : 1f;
        reloadSource = Sfx.Play(reloadSound, 0.9f, pitch);
    }

    
    string DefaultFireSound()
    {
        string weapon = DisplayName.ToLowerInvariant();
        if (weapon.Contains("shotgun")) return "shotgun";
        if (weapon.Contains("rifle")) return "assault-rifle-single-shot";
        return "pistol";
    }

    void FinishReload()
    {
        int needed = magazineSize - ammoInMagazine;
        int loaded = Mathf.Min(needed, reserveAmmo);
        ammoInMagazine += loaded;
        reserveAmmo -= loaded;
        isReloading = false;
    }

    public void AddMagazines(int count)
    {
        RememberFullAmmo();
        reserveAmmo += magazineSize * count;
    }

    public void AddReserve(int rounds) => reserveAmmo += Mathf.Max(rounds, 0);

    void RememberFullAmmo()
    {
        if (fullAmmo < 0) fullAmmo = magazineSize + reserveAmmo;
    }

    
    public void SetAmmo(int magazine, int reserve)
    {
        RememberFullAmmo();
        ammoInMagazine = Mathf.Clamp(magazine, 0, magazineSize);
        reserveAmmo = Mathf.Max(reserve, 0);
        ammoSet = true;
    }

    void SetAiming(bool aiming)
    {
        if (aiming == IsAiming) return;
        IsAiming = aiming;
        if (Controller != null) Controller.IsAiming = aiming;
        if (Crosshair != null) Crosshair.SetActive(!aiming);
    }

    void UpdateAim()
    {
        aimBlend = Mathf.MoveTowards(aimBlend, IsAiming ? 1f : 0f, aimSpeed * Time.deltaTime);
        float t = Mathf.SmoothStep(0f, 1f, aimBlend);
        ViewCamera.fieldOfView = Mathf.Lerp(HipFieldOfView, aimFieldOfView, t);
    }

    void AnimateModel()
    {
        if (model == null) return;

        recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, recoilRecoverSpeed * Time.deltaTime);
        float t = Mathf.SmoothStep(0f, 1f, aimBlend);
        model.localPosition = Vector3.Lerp(modelHipPosition, aimPosition, t) + recoilOffset;

        
        float tilt = 0f;
        if (isReloading)
        {
            float progress = 1f - (reloadEndTime - Time.time) / reloadDuration;
            tilt = Mathf.Sin(Mathf.Clamp01(progress) * Mathf.PI) * reloadTiltAngle;
        }
        model.localRotation = modelRestRotation * Quaternion.Euler(tilt, 0f, 0f);
    }

    void SpawnImpact(Vector3 point)
    {
        GameObject impact = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(impact.GetComponent<Collider>());
        impact.transform.position = point;
        impact.transform.localScale = Vector3.one * 0.1f;
        var block = new MaterialPropertyBlock();
        block.SetColor(BaseColorId, impactColor);
        impact.GetComponent<Renderer>().SetPropertyBlock(block);
        Destroy(impact, 0.1f);
    }
}
