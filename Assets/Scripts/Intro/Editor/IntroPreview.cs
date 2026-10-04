using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;



public static class IntroPreview
{
    public static void Capture()
    {
        IntroSceneBuilder.Build();
        EditorSceneManager.OpenScene("Assets/Scenes/Intro.unity", OpenSceneMode.Single);

        var heli = Object.FindAnyObjectByType<HelicopterRig>();
        var hatch = Object.FindAnyObjectByType<RoofHatch>();
        Camera cam = null;
        foreach (Camera c in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            if (c.name == "Cutscene Camera") cam = c;
        Transform tower = GameObject.Find("Tower Look").transform;
        Vector3 hatchLook = hatch.transform.position + Vector3.up * 0.6f;
        Vector3 eye = GameObject.Find("Landing Point").transform.position + Vector3.up * 0.6f;

        SpawnCrowds();

        foreach (float t in new[] { 0.2f, 0.5f, 0.8f })
        {
            heli.SetProgress(1f - Mathf.Pow(1f - t, 2.2f));
            Transform door = heli.DoorMount;
            Vector3 street = door.position + door.right * 16f + door.up * -28f + door.forward * 28f;
            Vector3 target = Vector3.Lerp(street, tower.position, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.55f, 1f, t)));
            Shot(cam, door.position, target, $"fly{t:0.0}");
        }

        heli.SetProgress(1f);
        Shot(cam, heli.DoorMount.position, hatchLook, "hover");
        Shot(cam, Vector3.Lerp(heli.DoorMount.position, eye, 0.55f) + Vector3.up * 1.2f, hatchLook, "leap");
        Shot(cam, eye, hatchLook, "landed");
        Shot(cam, hatchLook + new Vector3(4f, 1.2f, 0f), hatchLook, "hatch");
        Debug.Log("Preview frames written to /tmp/lpcheck");
    }

    
    static void SpawnCrowds()
    {
        foreach (ZombieCrowd crowd in Object.FindObjectsByType<ZombieCrowd>(FindObjectsSortMode.None))
        {
            var so = new SerializedObject(crowd);
            var prefab = (GameObject)so.FindProperty("zombiePrefab").objectReferenceValue;
            int count = so.FindProperty("count").intValue;
            float radius = so.FindProperty("radius").floatValue;
            var rng = new System.Random(7);
            for (int i = 0; i < count; i++)
            {
                float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
                float distance = Mathf.Sqrt((float)rng.NextDouble()) * radius;
                var local = new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.transform.SetParent(crowd.transform, false);
                go.transform.localPosition = local;
                go.transform.localRotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
            }
        }
    }

    static void Shot(Camera cam, Vector3 position, Vector3 target, string name)
    {
        cam.enabled = true;
        cam.transform.SetPositionAndRotation(position, Quaternion.LookRotation(target - position));
        var rt = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32) { antiAliasing = 2 };
        cam.targetTexture = rt;
        cam.Render();
        var shot = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        shot.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        shot.Apply();
        RenderTexture.active = null;
        cam.targetTexture = null;
        System.IO.File.WriteAllBytes($"/tmp/lpcheck/{name}.png", shot.EncodeToPNG());
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(shot);
    }
}
