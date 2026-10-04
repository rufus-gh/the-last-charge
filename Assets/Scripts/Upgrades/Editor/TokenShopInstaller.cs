using UnityEditor;
using UnityEngine;
using UnityEngine.UI;




public static class TokenShopInstaller
{
    const string RigPath = "Assets/Prefabs/Player Rig.prefab";

    [MenuItem("Losing Power/Install Token Shop In Player Rig")]
    public static void Install()
    {
        GameObject rig = PrefabUtility.LoadPrefabContents(RigPath);
        try
        {
            if (rig.GetComponent<TokenBank>() == null) rig.AddComponent<TokenBank>();

            Transform hud = rig.transform.Find("HUD");
            var progress = rig.GetComponent<LevelProgress>();
            var flashlight = new SerializedObject(progress).FindProperty("flashlight").objectReferenceValue;

            if (hud.Find("Tokens") == null) BuildCounter(hud);

            Transform shopTransform = hud.Find("Token Shop");
            if (shopTransform == null)
            {
                var shopGo = new GameObject("Token Shop", typeof(RectTransform));
                shopGo.transform.SetParent(hud, false);
                var rect = (RectTransform)shopGo.transform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                shopTransform = shopGo.transform;
            }
            if (!shopTransform.TryGetComponent(out TokenShop shop)) shop = shopTransform.gameObject.AddComponent<TokenShop>();
            var shopSo = new SerializedObject(shop);
            shopSo.FindProperty("flashlight").objectReferenceValue = flashlight;
            
            Behaviour[] pause = { rig.GetComponentInChildren<FPSController>(true), rig.GetComponentInChildren<WeaponManager>(true) };
            SerializedProperty shopDisables = shopSo.FindProperty("disableWhileOpen");
            shopDisables.arraySize = pause.Length;
            for (int i = 0; i < pause.Length; i++) shopDisables.GetArrayElementAtIndex(i).objectReferenceValue = pause[i];
            shopSo.ApplyModifiedPropertiesWithoutUndo();

            var progressSo = new SerializedObject(progress);
            progressSo.FindProperty("shop").objectReferenceValue = shop;
            progressSo.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(rig, RigPath);
            Debug.Log("Token shop installed in " + RigPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(rig);
        }
    }

    
    
    [MenuItem("Losing Power/Install Settings Menu In Player Rig")]
    public static void InstallSettings()
    {
        GameObject rig = PrefabUtility.LoadPrefabContents(RigPath);
        try
        {
            Transform hud = rig.transform.Find("HUD");
            var shop = hud.GetComponentInChildren<TokenShop>(true);
            Transform menuTransform = hud.Find("Settings Menu");
            if (menuTransform == null)
            {
                var go = new GameObject("Settings Menu", typeof(RectTransform));
                go.transform.SetParent(hud, false);
                var rect = (RectTransform)go.transform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                menuTransform = go.transform;
            }
            menuTransform.SetAsLastSibling();
            if (!menuTransform.TryGetComponent(out SettingsMenu menu)) menu = menuTransform.gameObject.AddComponent<SettingsMenu>();

            var menuSo = new SerializedObject(menu);
            var shopDisables = new SerializedObject(shop).FindProperty("disableWhileOpen");
            SerializedProperty disables = menuSo.FindProperty("disableWhileOpen");
            disables.arraySize = shopDisables.arraySize;
            for (int i = 0; i < shopDisables.arraySize; i++)
                disables.GetArrayElementAtIndex(i).objectReferenceValue = shopDisables.GetArrayElementAtIndex(i).objectReferenceValue;
            menuSo.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(rig, RigPath);
            Debug.Log("Settings menu installed in " + RigPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(rig);
        }
    }

    
    static void BuildCounter(Transform hud)
    {
        var health = (RectTransform)hud.Find("Health Bar");
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var plate = new GameObject("Tokens", typeof(RectTransform), typeof(Image), typeof(TokenHUD));
        plate.transform.SetParent(hud, false);
        var rect = (RectTransform)plate.transform;
        rect.anchorMin = rect.anchorMax = health.anchorMin;
        rect.pivot = health.pivot;
        rect.sizeDelta = new Vector2(220f, health.sizeDelta.y);
        rect.anchoredPosition = health.anchoredPosition + new Vector2(health.sizeDelta.x * (1f - health.pivot.x) + 20f + 220f * rect.pivot.x, 0f);
        plate.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

        var coin = new GameObject("Coin", typeof(RectTransform), typeof(Image));
        coin.transform.SetParent(plate.transform, false);
        var coinRect = (RectTransform)coin.transform;
        coinRect.anchorMin = coinRect.anchorMax = new Vector2(0f, 0.5f);
        coinRect.pivot = new Vector2(0f, 0.5f);
        coinRect.anchoredPosition = new Vector2(8f, 0f);
        coinRect.sizeDelta = Vector2.one * (health.sizeDelta.y - 12f);
        var coinImage = coin.GetComponent<Image>();
        coinImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        coinImage.color = new Color(1f, 0.8f, 0.2f);

        Text label = MakeText(plate.transform, font, "TOKENS  0", 20, new Color(1f, 0.85f, 0.35f));
        var labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(health.sizeDelta.y, 0f);
        labelRect.offsetMax = Vector2.zero;
        label.alignment = TextAnchor.MiddleLeft;

        Text popup = MakeText(plate.transform, font, "+1", 22, new Color(1f, 0.82f, 0.25f));
        popup.rectTransform.anchoredPosition = new Vector2(0f, health.sizeDelta.y);
        popup.rectTransform.sizeDelta = new Vector2(220f, 30f);

        var hudSo = new SerializedObject(plate.GetComponent<TokenHUD>());
        hudSo.FindProperty("label").objectReferenceValue = label;
        hudSo.FindProperty("popup").objectReferenceValue = popup;
        hudSo.ApplyModifiedPropertiesWithoutUndo();
    }

    static Text MakeText(Transform parent, Font font, string text, int size, Color color)
    {
        var go = new GameObject("Label", typeof(RectTransform), typeof(Text), typeof(Shadow));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.fontStyle = FontStyle.Bold;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        return t;
    }
}
