using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;




public class LevelProgress : MonoBehaviour
{
    [SerializeField] WeaponManager weapons;
    [SerializeField] PlayerHealth health;
    [SerializeField] Flashlight flashlight;
    [Tooltip("Between-level token shop")]
    [SerializeField] TokenShop shop;
    [Tooltip("Loaded after the last level's exit")]
    [SerializeField] string mainMenuScene = "MainMenu";

    ExitDoor exit;

    void Start()
    {
        exit = FindAnyObjectByType<ExitDoor>();
        if (exit != null) exit.onPlayerExited.AddListener(OnPlayerExited);

        if (!RunState.HasSave)
        {
            RunState.Reset();
            return;
        }
        Restore();
        
        if (shop != null) shop.Open($"LEVEL {CurrentLevel() - 1} CLEARED");
    }

    void OnDestroy()
    {
        if (exit != null) exit.onPlayerExited.RemoveListener(OnPlayerExited);
    }

    void Restore()
    {
        RestoreWeapons();

        foreach (var kv in RunState.Ammo)
            if (weapons.FindWeapon(kv.Key) is Gun gun) gun.SetAmmo(kv.Value.x, kv.Value.y);

        health.SetHealth(RunState.Health);
        if (flashlight != null) flashlight.SetBattery(RunState.Battery);

        Weapon current = weapons.FindWeapon(RunState.CurrentWeapon);
        int index = current != null ? IndexOf(current) : -1;
        weapons.Equip(index >= 0 ? index : 0, instant: true);
    }

    void RestoreWeapons()
    {
        foreach (string name in RunState.OwnedWeapons) weapons.Unlock(weapons.FindWeapon(name), equip: false);
    }

    int IndexOf(Weapon w)
    {
        for (int i = 0; i < weapons.Owned.Count; i++) if (weapons.Owned[i] == w) return i;
        return -1;
    }

    void OnPlayerExited()
    {
        RunState.AddTokens(TokenBank.LevelReward(CurrentLevel()));
        bool lastLevel = string.IsNullOrEmpty(exit.NextSceneName);
        if (lastLevel)
        {
            
            RunState.Reset();
            SceneManager.LoadScene(mainMenuScene);
            return;
        }
        RunState.Save(weapons, health, flashlight);
    }

    
    
    static int CurrentLevel()
    {
        Match m = Regex.Match(SceneManager.GetActiveScene().name, @"(\d+)$");
        return m.Success ? int.Parse(m.Groups[1].Value) : RunState.LevelsCleared + 1;
    }
}
