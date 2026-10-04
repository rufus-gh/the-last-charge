using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class WeaponHotbar : MonoBehaviour
{
    [System.Serializable]
    public class Slot
    {
        public RectTransform root;
        public Image background;
        public Image icon;
        public Text keyLabel;
    }

    [SerializeField] WeaponManager weapons;
    [SerializeField] List<Slot> slots = new List<Slot>();
    [SerializeField] Text nameLabel;

    [Header("Look")]
    [SerializeField] float slotSpacing = 152f;
    [SerializeField] Color selectedBackground = new Color(0.9f, 0.45f, 0.2f, 0.45f);
    [SerializeField] Color normalBackground = new Color(0f, 0f, 0f, 0.5f);
    [Range(0f, 1f)]
    [SerializeField] float unselectedIconAlpha = 0.4f;
    [SerializeField] float selectedScale = 1.08f;
    [SerializeField] float nameShowTime = 1.5f;
    [SerializeField] float nameFadeTime = 0.5f;

    int selected = -1;
    float nameShownAt = float.NegativeInfinity;

    void OnEnable()
    {
        if (weapons == null) return;
        weapons.WeaponChanged += OnWeaponChanged;
        weapons.OwnedChanged += Rebuild;
    }

    void OnDisable()
    {
        if (weapons == null) return;
        weapons.WeaponChanged -= OnWeaponChanged;
        weapons.OwnedChanged -= Rebuild;
    }

    void Start()
    {
        Rebuild();
        Select(weapons.CurrentIndex, showName: false);
    }

    void Rebuild()
    {
        int count = weapons.Owned.Count;
        for (int i = 0; i < slots.Count; i++)
        {
            Slot slot = slots[i];
            bool used = i < count;
            slot.root.gameObject.SetActive(used);
            if (!used) continue;

            
            slot.root.anchorMin = slot.root.anchorMax = new Vector2(0.5f, 0.5f);
            slot.root.anchoredPosition = new Vector2((i - (count - 1) / 2f) * slotSpacing, 0f);
            slot.icon.sprite = weapons.Owned[i].Icon;
            slot.icon.enabled = slot.icon.sprite != null;
            slot.keyLabel.text = (i + 1).ToString();
        }
        Select(weapons.CurrentIndex, showName: false);
    }

    void OnWeaponChanged(int index) => Select(index, showName: true);

    void Select(int index, bool showName)
    {
        selected = index;
        for (int i = 0; i < slots.Count && i < weapons.Owned.Count; i++)
        {
            bool isSelected = i == index;
            slots[i].background.color = isSelected ? selectedBackground : normalBackground;
            Color c = slots[i].icon.color;
            c.a = isSelected ? 1f : unselectedIconAlpha;
            slots[i].icon.color = c;
        }

        if (showName && weapons.Current != null)
        {
            nameLabel.text = weapons.Current.DisplayName.ToUpperInvariant();
            nameShownAt = Time.time;
        }
    }

    void Update()
    {
        for (int i = 0; i < slots.Count && i < weapons.Owned.Count; i++)
        {
            float target = i == selected ? selectedScale : 1f;
            slots[i].root.localScale = Vector3.Lerp(slots[i].root.localScale, Vector3.one * target, 15f * Time.deltaTime);
        }

        float age = Time.time - nameShownAt;
        float alpha = age < nameShowTime ? 1f : 1f - (age - nameShowTime) / nameFadeTime;
        Color nc = nameLabel.color;
        nc.a = Mathf.Clamp01(alpha);
        nameLabel.color = nc;
    }
}
