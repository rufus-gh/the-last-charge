using UnityEngine;
using UnityEngine.UI;


public class AmmoHUD : MonoBehaviour
{
    [SerializeField] WeaponManager weapons;
    [SerializeField] Text ammoText;
    [SerializeField] Text reloadPrompt;
    [SerializeField] Color promptColor = Color.white;
    [SerializeField] Color emptyColor = new Color(1f, 0.3f, 0.3f);

    Weapon lastWeapon;
    int lastAmmo = -1;
    int lastReserve = -1;
    int lastPromptState = -1;

    void Update()
    {
        Weapon weapon = weapons != null ? weapons.Current : null;
        if (weapon != lastWeapon)
        {
            
            lastWeapon = weapon;
            lastAmmo = lastReserve = lastPromptState = -1;
        }

        bool show = weapon != null && weapon.UsesAmmo;
        ammoText.enabled = show;
        if (!show)
        {
            reloadPrompt.enabled = false;
            return;
        }

        if (weapon.AmmoInMagazine != lastAmmo || weapon.ReserveAmmo != lastReserve)
        {
            lastAmmo = weapon.AmmoInMagazine;
            lastReserve = weapon.ReserveAmmo;
            ammoText.text = $"{lastAmmo} / {lastReserve}";
            ammoText.color = lastAmmo == 0 ? emptyColor : Color.white;
        }

        int promptState = GetPromptState(weapon);
        if (promptState == lastPromptState) return;
        lastPromptState = promptState;

        reloadPrompt.enabled = promptState != 0;
        switch (promptState)
        {
            case 1: reloadPrompt.text = "Reloading..."; reloadPrompt.color = promptColor; break;
            case 2: reloadPrompt.text = "Press R to reload"; reloadPrompt.color = promptColor; break;
            case 3: reloadPrompt.text = "Out of ammo! Press R to reload"; reloadPrompt.color = emptyColor; break;
            case 4: reloadPrompt.text = "No ammo left"; reloadPrompt.color = emptyColor; break;
        }
    }

    
    static int GetPromptState(Weapon w)
    {
        if (w.IsReloading) return 1;
        if (w.AmmoInMagazine >= w.MagazineSize) return 0;
        if (w.ReserveAmmo <= 0) return w.AmmoInMagazine == 0 ? 4 : 0;
        return w.AmmoInMagazine == 0 ? 3 : 2;
    }
}
