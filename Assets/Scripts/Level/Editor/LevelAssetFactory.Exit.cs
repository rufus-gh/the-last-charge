using UnityEditor;
using UnityEngine;



public static partial class LevelAssetFactory
{
    static Material exitGreen, signHousing, portalGlow, doorPaint;

    static void BuildExitDoor()
    {
        exitGreen = Mat("ExitGreen", new Color(0.1f, 1f, 0.2f), 0.5f, new Color(0.04f, 1f, 0.12f) * 1.8f);
        signHousing = Mat("SignHousing", new Color(0.03f, 0.03f, 0.03f), 0.35f);
        portalGlow = Mat("PortalGlow", new Color(0.3f, 1f, 0.45f), 0.2f, new Color(0.08f, 1f, 0.3f) * 1.6f);
        doorPaint = Mat("DoorPaint", new Color(0.16f, 0.19f, 0.18f), 0.4f, metallic: 0.3f);
        Material frame = Mat("DoorFrame", new Color(0.42f, 0.45f, 0.46f), 0.55f, metallic: 0.6f);

        SavePrefab(() => ExitDoorPrefab(frame), $"{PrefabRoot}/ExitDoor.prefab", PrefabKind.Fixture);
    }

    
    static GameObject ExitDoorPrefab(Material frame)
    {
        Transform r = Root("ExitDoor");
        Box(r, new Vector3(-0.56f, 1.1f, 0.06f), new Vector3(0.12f, 2.2f, 0.12f), frame);
        Box(r, new Vector3(0.56f, 1.1f, 0.06f), new Vector3(0.12f, 2.2f, 0.12f), frame);
        Box(r, new Vector3(0f, 2.16f, 0.06f), new Vector3(1.24f, 0.12f, 0.12f), frame);

        
        Box(r, new Vector3(0f, 1.05f, 0.005f), new Vector3(1.0f, 2.1f, 0.01f), portalGlow);

        
        Transform hinge = Group(r, "Hinge", new Vector3(-0.5f, 0f, 0.06f));
        Transform leaf = Group(hinge, "Leaf", new Vector3(0.5f, 0f, 0f));
        Box(leaf, new Vector3(0f, 1.05f, 0f), new Vector3(0.99f, 2.08f, 0.05f), doorPaint);
        Box(leaf, new Vector3(0f, 1.0f, 0.045f), new Vector3(0.7f, 0.05f, 0.04f), frame);
        Box(leaf, new Vector3(0.38f, 1.0f, 0.045f), new Vector3(0.05f, 0.12f, 0.05f), frame);
        Box(leaf, new Vector3(0f, 1.55f, 0.027f), new Vector3(0.3f, 0.4f, 0.005f), signHousing);
        var leafCollider = leaf.gameObject.AddComponent<BoxCollider>();
        leafCollider.center = new Vector3(0f, 1.05f, 0f);
        leafCollider.size = new Vector3(0.99f, 2.08f, 0.05f);

        
        foreach (float x in new[] { -0.56f, 0.56f })
        {
            var jamb = r.gameObject.AddComponent<BoxCollider>();
            jamb.center = new Vector3(x, 1.1f, 0.06f);
            jamb.size = new Vector3(0.12f, 2.2f, 0.12f);
        }

        Transform sign = BuildExitSign(r, new Vector3(0f, 2.5f, 0f));

        
        var portalLightGo = new GameObject("Portal Light");
        portalLightGo.transform.SetParent(r, false);
        portalLightGo.transform.localPosition = new Vector3(0f, 1.6f, 0.15f);
        portalLightGo.transform.localEulerAngles = new Vector3(15f, 0f, 0f);
        var portalLight = portalLightGo.AddComponent<Light>();
        portalLight.type = LightType.Spot;
        portalLight.color = new Color(0.35f, 1f, 0.5f);
        portalLight.intensity = 8f;
        portalLight.range = 10f;
        portalLight.spotAngle = 150f;
        portalLight.innerSpotAngle = 60f;

        var door = r.gameObject.AddComponent<ProximityDoor>();
        var doorSo = new SerializedObject(door);
        doorSo.FindProperty("hinge").objectReferenceValue = hinge;
        SerializedProperty reveal = doorSo.FindProperty("revealLights");
        reveal.arraySize = 1;
        reveal.GetArrayElementAtIndex(0).objectReferenceValue = portalLight;
        doorSo.ApplyModifiedPropertiesWithoutUndo();

        
        var triggerGo = new GameObject("Exit Trigger");
        triggerGo.transform.SetParent(r, false);
        var trigger = triggerGo.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 1.1f, 0.4f);
        trigger.size = new Vector3(0.95f, 2.2f, 0.8f);
        triggerGo.AddComponent<ExitDoor>();
        return r.gameObject;
    }

    
    static Transform BuildExitSign(Transform parent, Vector3 position)
    {
        Transform sign = Group(parent, "Exit Sign", position);
        Box(sign, new Vector3(0f, 0f, 0.05f), new Vector3(0.7f, 0.28f, 0.08f), signHousing);
        Box(sign, new Vector3(0f, 0.17f, 0.03f), new Vector3(0.3f, 0.06f, 0.05f), signHousing);

        const float z = 0.093f, depth = 0.008f, stroke = 0.028f, h = 0.17f, w = 0.1f;
        
        void Bar(float x, float y, float bw, float bh, float angle = 0f) =>
            Box(sign, new Vector3(-x, y, z), new Vector3(bw, bh, depth), exitGreen, new Vector3(0f, 0f, -angle));

        
        float ex = -0.195f;
        Bar(ex - w / 2f + stroke / 2f, 0f, stroke, h);
        Bar(ex, h / 2f - stroke / 2f, w, stroke);
        Bar(ex - 0.008f, 0f, w - 0.016f, stroke);
        Bar(ex, -h / 2f + stroke / 2f, w, stroke);
        
        float diag = Mathf.Sqrt(w * w + h * h) - 0.02f;
        float angle = Mathf.Atan2(w, h) * Mathf.Rad2Deg;
        Bar(-0.065f, 0f, stroke, diag, angle);
        Bar(-0.065f, 0f, stroke, diag, -angle);
        
        Bar(0.04f, 0f, stroke, h);
        
        Bar(0.145f, h / 2f - stroke / 2f, w, stroke);
        Bar(0.145f, -stroke / 2f, stroke, h - stroke);
        
        foreach (int side in new[] { -1, 1 })
        {
            float cx = side * 0.3f;
            Bar(cx, 0.017f, 0.012f, 0.045f, side * 40f);
            Bar(cx, -0.017f, 0.012f, 0.045f, -side * 40f);
        }

        
        var lightGo = new GameObject("Sign Light");
        lightGo.transform.SetParent(sign, false);
        lightGo.transform.localPosition = new Vector3(0f, -0.2f, 0.3f);
        lightGo.transform.localEulerAngles = new Vector3(40f, 0f, 0f);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Spot;
        light.color = new Color(0.2f, 1f, 0.35f);
        light.intensity = 4f;
        light.range = 8f;
        light.spotAngle = 140f;
        light.innerSpotAngle = 60f;

        
        var flicker = sign.gameObject.AddComponent<FixtureFlicker>();
        var flickerSo = new SerializedObject(flicker);
        flickerSo.FindProperty("steadyTime").vector2Value = new Vector2(2f, 6f);
        flickerSo.FindProperty("burstTime").vector2Value = new Vector2(0.1f, 0.5f);
        flickerSo.FindProperty("dimLevel").floatValue = 0.2f;
        flickerSo.ApplyModifiedPropertiesWithoutUndo();
        return sign;
    }
}
