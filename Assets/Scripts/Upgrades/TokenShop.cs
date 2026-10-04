using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;






public class TokenShop : MonoBehaviour
{
    [Serializable]
    public class GunOffer
    {
        [Tooltip("Name of the weapon object under Player/Main Camera/Weapons")]
        public string weaponName;
        [Tooltip("Tokens to buy the gun; 0 for one the player starts with")]
        public int price;
        [Tooltip("Tokens to refill it from empty to a full magazine plus its starting reserve; partial refills cost the matching fraction")]
        public int fullAmmoPrice;
    }

    [SerializeField] Flashlight flashlight;
    [Tooltip("Player scripts switched off while the shop is open (movement, weapons)")]
    [SerializeField] Behaviour[] disableWhileOpen;
    [Tooltip("Tokens for a full charge from empty; partial top-ups cost the matching fraction")]
    [SerializeField] int fullTopUpPrice = 20;

    [Header("Guns")]
    [SerializeField] WeaponManager weapons;
    [Tooltip("One card each, left to right, with its ammo refill underneath")]
    [SerializeField] GunOffer[] guns =
    {
        new GunOffer { weaponName = "Pistol", price = 0, fullAmmoPrice = 20 },
        new GunOffer { weaponName = "Shotgun", price = 40, fullAmmoPrice = 35 },
        new GunOffer { weaponName = "Rifle", price = 40, fullAmmoPrice = 35 },
    };

    [Header("Supplies")]
    [SerializeField] PlayerHealth health;
    [SerializeField] int medkitPrice = 10;
    [SerializeField] float medkitHealth = 50f;

    public bool IsOpen { get; private set; }

    static readonly Color Panel = new Color(0.04f, 0.04f, 0.05f, 0.94f);
    static readonly Color Card = new Color(0.1f, 0.1f, 0.12f, 1f);
    static readonly Color Gold = new Color(1f, 0.82f, 0.25f);

    GameObject root;
    
    int shownFrame;
    Text titleLabel, tokensLabel, messageLabel, topUpLabel;
    Button topUpButton, medkitButton;
    Text medkitLabel;
    Button[] gunButtons, ammoButtons;
    Text[] gunLabels, ammoLabels;
    Font font;

    void Awake()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (health == null) health = FindAnyObjectByType<PlayerHealth>();
        if (weapons == null) weapons = FindAnyObjectByType<WeaponManager>();
        BuildUI();
        root.SetActive(false);
    }

    void OnEnable() => RunState.TokensChanged += OnTokensChanged;

    void OnDisable() => RunState.TokensChanged -= OnTokensChanged;

    void OnTokensChanged(int _)
    {
        if (IsOpen) Refresh();
    }

    public void Open(string title)
    {
        IsOpen = true;
        titleLabel.text = title;
        messageLabel.text = "";
        Pause();
        root.SetActive(true);
        shownFrame = Time.frameCount;
        Refresh();
    }

    void Pause()
    {
        Time.timeScale = 0f;
        foreach (Behaviour b in disableWhileOpen) if (b != null) b.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    Gun GunFor(int slot) => weapons != null ? weapons.FindWeapon(guns[slot].weaponName) as Gun : null;

    bool Owned(int slot)
    {
        Gun gun = GunFor(slot);
        return gun != null && weapons.Owns(gun);
    }

    void BuyGun(int slot)
    {
        Gun gun = GunFor(slot);
        if (gun == null || weapons.Owns(gun)) return;
        if (!RunState.TrySpend(guns[slot].price))
        {
            messageLabel.text = $"Not enough tokens for the {gun.DisplayName}";
            return;
        }
        weapons.Unlock(gun, equip: false);
        
        if (!RunState.OwnedWeapons.Contains(gun.name)) RunState.OwnedWeapons.Add(gun.name);
        messageLabel.text = "";
        Bought();
    }

    void TopUp()
    {
        if (!TopUpQuote(out int cost, out float charge)) return;
        if (!RunState.TrySpend(cost)) return;
        flashlight.AddBattery(charge);
        Bought();
    }

    
    bool TopUpQuote(out int cost, out float charge)
    {
        cost = 0;
        charge = 0f;
        if (flashlight == null) return false;
        float missing = flashlight.MaxBattery - flashlight.Battery;
        if (missing <= 0.01f) return false;

        int fullCost = Mathf.CeilToInt(missing / flashlight.MaxBattery * fullTopUpPrice);
        cost = Mathf.Min(fullCost, RunState.Tokens);
        if (cost <= 0) return false;
        charge = cost == fullCost ? missing : cost * flashlight.MaxBattery / fullTopUpPrice;
        return true;
    }

    

    bool MedkitUseful() => health != null && health.CurrentHealth < health.MaxHealth - 0.5f;

    void RefillAmmo(int slot)
    {
        Gun gun = GunFor(slot);
        if (!Owned(slot) || !AmmoQuote(gun, guns[slot].fullAmmoPrice, out int cost, out int rounds) || !RunState.TrySpend(cost)) return;
        gun.AddReserve(rounds);
        Bought();
    }

    
    static bool AmmoQuote(Gun gun, int fullPrice, out int cost, out int rounds)
    {
        cost = 0;
        rounds = 0;
        if (gun == null || gun.FullAmmo <= 0) return false;
        int missing = gun.FullAmmo - gun.AmmoInMagazine - gun.ReserveAmmo;
        if (missing <= 0) return false;

        int fullCost = Mathf.CeilToInt((float)missing / gun.FullAmmo * fullPrice);
        cost = Mathf.Min(fullCost, RunState.Tokens);
        if (cost <= 0) return false;
        rounds = cost == fullCost ? missing : Mathf.Max(1, Mathf.FloorToInt((float)cost * gun.FullAmmo / fullPrice));
        return true;
    }

    void RefreshGuns()
    {
        for (int i = 0; i < guns.Length; i++)
        {
            Gun gun = GunFor(i);
            gunButtons[i].gameObject.SetActive(gun != null);
            ammoButtons[i].gameObject.SetActive(gun != null);
            if (gun == null) continue;

            string name = gun.DisplayName.ToUpperInvariant();
            bool owned = weapons.Owns(gun);
            int price = guns[i].price;
            string status = owned ? "<color=#7FE08A>OWNED</color>"
                : RunState.Tokens >= price ? $"<color=#FFD040>{price} TOKENS</color>"
                : $"<color=#FF6050>{price} TOKENS</color>";
            gunLabels[i].text = $"<size=40>{name}</size>\n\n<size=34>{status}</size>\n\n<size=22>{(owned ? "Equip it with its number key" : $"Press {i + 1} or click to buy")}</size>";
            gunButtons[i].interactable = !owned && RunState.Tokens >= price;

            int have = gun.AmmoInMagazine + gun.ReserveAmmo;
            if (!owned) ammoLabels[i].text = "<color=#8A8A90>BUY THE GUN FIRST</color>";
            else if (have >= gun.FullAmmo) ammoLabels[i].text = $"AMMO FULL   {have}/{gun.FullAmmo}";
            else if (AmmoQuote(gun, guns[i].fullAmmoPrice, out int cost, out int rounds))
                ammoLabels[i].text = $"REFILL AMMO   {have} → {have + rounds}   <color=#FFD040>{cost}</color>";
            else ammoLabels[i].text = $"REFILL AMMO   {have}/{gun.FullAmmo}   <color=#FF6050>no tokens</color>";
            ammoButtons[i].interactable = owned && AmmoQuote(gun, guns[i].fullAmmoPrice, out _, out _);
        }
    }

    void BuyMedkit()
    {
        if (!MedkitUseful() || !RunState.TrySpend(medkitPrice)) return;
        health.Heal(medkitHealth);
        Bought();
    }

    
    void Bought()
    {
        Sfx.Play("level-up", 1f, 1f, ui: true);
        Refresh();
    }

    void Continue()
    {
        IsOpen = false;
        root.SetActive(false);
        Time.timeScale = 1f;
        
        foreach (Behaviour b in disableWhileOpen) if (b != null) b.enabled = true;
    }

    void Refresh()
    {
        tokensLabel.text = $"{RunState.Tokens} TOKENS";
        RefreshGuns();

        int now = flashlight != null ? Mathf.RoundToInt(flashlight.Charge01 * 100f) : 0;
        if (flashlight == null) topUpLabel.text = "NO FLASHLIGHT";
        else if (flashlight.Battery >= flashlight.MaxBattery - 0.01f) topUpLabel.text = $"FLASHLIGHT FULL  ({now}%)";
        else if (TopUpQuote(out int cost, out float charge))
        {
            int after = Mathf.RoundToInt((flashlight.Battery + charge) / flashlight.MaxBattery * 100f);
            topUpLabel.text = $"TOP UP FLASHLIGHT   {now}% → {after}%   <color=#FFD040>{cost} TOKENS</color>";
        }
        else topUpLabel.text = $"TOP UP FLASHLIGHT   {now}%   <color=#FF6050>no tokens</color>";
        topUpButton.interactable = TopUpQuote(out _, out _);

        bool medkit = MedkitUseful();
        medkitLabel.text = $"MEDKIT  +{medkitHealth:0} HP\n{(medkit ? $"<color=#FFD040>{medkitPrice} TOKENS</color>" : "HEALTH FULL")}";
        medkitButton.interactable = medkit && RunState.Tokens >= medkitPrice;
    }

    void Update()
    {
        if (!IsOpen || !root.activeSelf || Time.frameCount == shownFrame) return;
        Keyboard k = Keyboard.current;
        if (k == null) return;
        if (k.digit1Key.wasPressedThisFrame) BuyGun(0);
        else if (k.digit2Key.wasPressedThisFrame) BuyGun(1);
        else if (k.digit3Key.wasPressedThisFrame) BuyGun(2);
        else if (k.fKey.wasPressedThisFrame) TopUp();
        else if (k.digit5Key.wasPressedThisFrame) BuyMedkit();
        else if (k.enterKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame) Continue();
    }

    

    void BuildUI()
    {
        root = new GameObject("Token Shop Panel", typeof(RectTransform), typeof(Image));
        root.transform.SetParent(transform, false);
        Stretch((RectTransform)root.transform);
        root.GetComponent<Image>().color = Panel;

        titleLabel = Label(root.transform, "", 64, Color.white, new Vector2(0f, 380f), new Vector2(1400f, 90f));
        tokensLabel = Label(root.transform, "", 46, Gold, new Vector2(0f, 240f), new Vector2(900f, 64f));

        
        gunButtons = new Button[guns.Length];
        gunLabels = new Text[guns.Length];
        ammoButtons = new Button[guns.Length];
        ammoLabels = new Text[guns.Length];
        for (int i = 0; i < guns.Length; i++)
        {
            int slot = i;
            float x = (i - (guns.Length - 1) / 2f) * 400f;
            gunButtons[i] = MakeButton(root.transform, new Vector2(x, 75f), new Vector2(360f, 220f), out gunLabels[i]);
            gunButtons[i].onClick.AddListener(() => BuyGun(slot));
            ammoButtons[i] = MakeButton(root.transform, new Vector2(x, -82f), new Vector2(360f, 64f), out ammoLabels[i]);
            ammoLabels[i].fontSize = 22;
            ammoButtons[i].onClick.AddListener(() => RefillAmmo(slot));
        }

        topUpButton = MakeButton(root.transform, new Vector2(-145f, -170f), new Vector2(870f, 64f), out topUpLabel);
        topUpLabel.fontSize = 26;
        topUpButton.onClick.AddListener(TopUp);
        medkitButton = MakeButton(root.transform, new Vector2(440f, -170f), new Vector2(280f, 64f), out medkitLabel);
        medkitLabel.fontSize = 22;
        medkitButton.onClick.AddListener(BuyMedkit);

        messageLabel = Label(root.transform, "", 26, new Color(1f, 0.45f, 0.4f), new Vector2(0f, -240f), new Vector2(1200f, 36f));

        Button next = MakeButton(root.transform, new Vector2(0f, -310f), new Vector2(420f, 70f), out Text nextLabel);
        nextLabel.text = "CONTINUE";
        nextLabel.fontSize = 36;
        next.GetComponent<Image>().color = new Color(0.12f, 0.32f, 0.18f, 1f);
        next.onClick.AddListener(Continue);

        Label(root.transform, "1 / 2 / 3 buy a gun    click under a gun to refill its ammo    F top up flashlight    5 medkit    Enter continue", 22, new Color(0.55f, 0.55f, 0.6f),
            new Vector2(0f, -380f), new Vector2(1200f, 34f));
    }

    Button MakeButton(Transform parent, Vector2 position, Vector2 size, out Text label)
    {
        var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        go.GetComponent<Image>().color = Card;
        var button = go.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1.4f, 1.4f, 1.4f, 1f);
        colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.6f);
        button.colors = colors;
        label = Label(go.transform, "", 30, Color.white, Vector2.zero, size - new Vector2(20f, 10f));
        return button;
    }

    Text Label(Transform parent, string text, int size, Color color, Vector2 position, Vector2 box)
    {
        var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.anchoredPosition = position;
        rect.sizeDelta = box;
        var t = go.GetComponent<Text>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.supportRichText = true;
        t.raycastTarget = false;
        return t;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
