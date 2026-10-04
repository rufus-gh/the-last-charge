using System;
using System.Collections.Generic;
using UnityEngine;



public static class RunState
{
    public static int LevelsCleared { get; private set; }
    public static bool HasSave => LevelsCleared > 0;

    
    public static readonly List<string> OwnedWeapons = new List<string>();
    
    public static readonly Dictionary<string, Vector2Int> Ammo = new Dictionary<string, Vector2Int>();
    public static string CurrentWeapon;
    public static float Health;
    public static float Battery;
    
    public static float StartedAt;
    public static int Kills;

    
    
    public static int Tokens { get; private set; }
    public static event Action<int> TokensChanged;

    public static void AddTokens(int amount)
    {
        Tokens = Mathf.Max(0, Tokens + amount);
        TokensChanged?.Invoke(Tokens);
    }

    public static bool TrySpend(int amount)
    {
        if (amount > Tokens) return false;
        AddTokens(-amount);
        return true;
    }

    
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ClearStatics()
    {
        TokensChanged = null;
        LevelsCleared = 0;
        OwnedWeapons.Clear();
        Ammo.Clear();
        CurrentWeapon = null;
        Tokens = 0;
        Kills = 0;
    }

    public static void Reset()
    {
        LevelsCleared = 0;
        OwnedWeapons.Clear();
        Ammo.Clear();
        CurrentWeapon = null;
        Tokens = 0;
        Kills = 0;
        StartedAt = Time.time;
        TokensChanged?.Invoke(Tokens);
    }

    public static void Save(WeaponManager weapons, PlayerHealth health, Flashlight flashlight)
    {
        LevelsCleared++;
        OwnedWeapons.Clear();
        foreach (Weapon w in weapons.Owned) OwnedWeapons.Add(w.name);
        Ammo.Clear();
        
        
        foreach (Weapon w in weapons.Owned)
            if (w is Gun gun) Ammo[w.name] = new Vector2Int(gun.AmmoInMagazine, gun.ReserveAmmo);
        CurrentWeapon = weapons.Current != null ? weapons.Current.name : null;
        Health = health.CurrentHealth;
        Battery = flashlight != null ? flashlight.Battery : 0f;
    }
}
