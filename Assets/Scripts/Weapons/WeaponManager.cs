using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



public class WeaponManager : MonoBehaviour
{
    [SerializeField] Camera viewCamera;
    [SerializeField] FPSController controller;
    [SerializeField] GameObject crosshair;
    [Tooltip("Every weapon the player can ever have, in hotbar order. Ones not owned at start are bought in the token shop.")]
    [SerializeField] List<Weapon> weapons = new List<Weapon>();
    [Tooltip("Index into the owned weapons")]
    [SerializeField] int startingWeapon = 0;

    [Header("Switching")]
    [SerializeField] float raiseTime = 0.25f;
    [SerializeField] float raiseDrop = 0.35f;

    [Header("Flashlight")]
    [Tooltip("Moved onto each weapon's flashlight mount when it's equipped")]
    [SerializeField] Transform flashlight;
    [Tooltip("Camera-local position for weapons without a mount (melee)")]
    [SerializeField] Vector3 chestLightPosition = new Vector3(-0.12f, -0.1f, 0.25f);

    
    public IReadOnlyList<Weapon> Weapons => weapons;
    
    public IReadOnlyList<Weapon> Owned => owned;
    public Weapon Current { get; private set; }
    public int CurrentIndex => Current != null ? owned.IndexOf(Current) : -1;

    
    public event Action<int> WeaponChanged;
    public event Action OwnedChanged;

    readonly List<Weapon> owned = new List<Weapon>();
    float raiseStart = float.NegativeInfinity;
    bool wasCursorLocked;

    void Awake()
    {
        if (viewCamera == null) viewCamera = GetComponentInChildren<Camera>();
        if (controller == null) controller = GetComponent<FPSController>();

        foreach (Weapon w in weapons)
        {
            w.Init(viewCamera, controller, crosshair);
            w.gameObject.SetActive(false);
            if (w.OwnedAtStart) owned.Add(w);
        }
        Equip(Mathf.Clamp(startingWeapon, 0, owned.Count - 1), instant: true);
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (keyboard == null || mouse == null || Current == null) return;

        for (int i = 0; i < owned.Count && i < 9; i++)
            if (keyboard[Key.Digit1 + i].wasPressedThisFrame) Equip(i);

        float scroll = mouse.scroll.ReadValue().y;
        if (scroll > 0.1f) Equip((CurrentIndex - 1 + owned.Count) % owned.Count);
        else if (scroll < -0.1f) Equip((CurrentIndex + 1) % owned.Count);

        
        float raise = Mathf.Clamp01((Time.time - raiseStart) / raiseTime);
        Current.transform.localPosition = Vector3.down * raiseDrop * (1f - Mathf.SmoothStep(0f, 1f, raise));

        
        bool cursorLocked = Cursor.lockState == CursorLockMode.Locked;
        bool canAct = cursorLocked && wasCursorLocked && raise >= 1f;
        wasCursorLocked = cursorLocked;

        WeaponInput input = default;
        if (canAct)
        {
            input.FirePressed = mouse.leftButton.wasPressedThisFrame;
            input.FireHeld = mouse.leftButton.isPressed;
            input.AimHeld = mouse.rightButton.isPressed;
            input.ReloadPressed = keyboard.rKey.wasPressedThisFrame;
        }
        Current.HandleInput(input);
    }

    void OnDisable()
    {
        
        if (Current != null) Current.HandleInput(default);
        wasCursorLocked = false;
    }

    public Weapon FindWeapon(string weaponName) => weapons.Find(w => w.name == weaponName);

    public bool Owns(Weapon weapon) => owned.Contains(weapon);

    
    public void Unlock(Weapon weapon, bool equip = true)
    {
        if (weapon == null || owned.Contains(weapon)) return;

        var updated = new List<Weapon>();
        foreach (Weapon w in weapons)
            if (w == weapon || owned.Contains(w)) updated.Add(w);
        owned.Clear();
        owned.AddRange(updated);
        OwnedChanged?.Invoke();

        if (equip) Equip(owned.IndexOf(weapon));
        else WeaponChanged?.Invoke(CurrentIndex);
    }

    public void Equip(int ownedIndex, bool instant = false)
    {
        if (ownedIndex < 0 || ownedIndex >= owned.Count || owned[ownedIndex] == Current) return;

        if (Current != null)
        {
            Current.OnUnequip();
            Current.gameObject.SetActive(false);
        }

        Current = owned[ownedIndex];
        Current.gameObject.SetActive(true);
        Current.OnEquip();
        AttachFlashlight(Current.FlashlightMount);
        raiseStart = instant ? float.NegativeInfinity : Time.time;
        WeaponChanged?.Invoke(ownedIndex);
    }

    void AttachFlashlight(Transform mount)
    {
        if (flashlight == null) return;
        if (mount != null)
        {
            flashlight.SetParent(mount, false);
            flashlight.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
        else
        {
            flashlight.SetParent(viewCamera.transform, false);
            flashlight.SetLocalPositionAndRotation(chestLightPosition, Quaternion.identity);
        }
    }
}
