using UnityEngine;


public struct WeaponInput
{
    public bool FirePressed;
    public bool FireHeld;
    public bool AimHeld;
    public bool ReloadPressed;
}


public abstract class Weapon : MonoBehaviour
{
    [SerializeField] string displayName = "Weapon";
    [SerializeField] Sprite icon;
    [Tooltip("In the hotbar from the start. Others are bought in the token shop.")]
    [SerializeField] bool ownedAtStart = true;
    [Tooltip("Where the flashlight attaches while this weapon is out. Put it in front of the model so the beam doesn't light the weapon itself. Leave empty for a chest-mounted light.")]
    [SerializeField] Transform flashlightMount;

    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public Transform FlashlightMount => flashlightMount;
    public bool OwnedAtStart => ownedAtStart;

    public virtual bool UsesAmmo => false;
    public virtual int AmmoInMagazine => 0;
    public virtual int ReserveAmmo => 0;
    public virtual int MagazineSize => 0;
    public virtual bool IsReloading => false;

    protected Camera ViewCamera { get; private set; }
    protected FPSController Controller { get; private set; }
    protected GameObject Crosshair { get; private set; }
    protected float HipFieldOfView { get; private set; }

    public void Init(Camera viewCamera, FPSController controller, GameObject crosshair)
    {
        ViewCamera = viewCamera;
        Controller = controller;
        Crosshair = crosshair;
        HipFieldOfView = viewCamera.fieldOfView;
    }

    public abstract void HandleInput(WeaponInput input);

    public virtual void OnEquip() { }
    public virtual void OnUnequip() { }
}
