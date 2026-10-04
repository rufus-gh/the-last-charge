using UnityEditor;
using UnityEngine;





public static class ZombieInstaller
{
    const string ZombiePath = "Assets/Prefabs/Zombie.prefab";
    const string BossPath = "Assets/Prefabs/BossZombie.prefab";

    [MenuItem("Losing Power/Install Zombie Model")]
    public static void Install()
    {
        
        LevelAssetFactory.RebuildZombiePrefabs();
        InstallRegularZombie();
        InstallBoss();
        AssetDatabase.SaveAssets();
        Debug.Log("Zombie model installed on the regular zombie and the boss.");
    }

    static void InstallRegularZombie()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(ZombiePath);
        try
        {
            
            foreach (string old in new[] { "Body", "Face", "Model" })
            {
                Transform child = root.transform.Find(old);
                if (child != null) Object.DestroyImmediate(child.gameObject);
            }

            
            
            AssetDatabase.ImportAsset(LevelAssetFactory.ZombieModelPath(1), ImportAssetOptions.ForceUpdate);
            var visual = AssetDatabase.LoadAssetAtPath<GameObject>(LevelAssetFactory.ZombieModelPath(1));
            var model = (GameObject)PrefabUtility.InstantiatePrefab(visual, root.transform);
            model.GetComponent<Animator>().runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ZombieModels.GameControllerPath);
            model.name = "Model";
            
            model.transform.localPosition = new Vector3(0f, -1f, 0f);
            model.transform.localRotation = Quaternion.identity;
            foreach (Transform t in model.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = root.layer;

            var skins = new Object[ZombieModels.Variants];
            for (int i = 0; i < skins.Length; i++) skins[i] = LevelAssetFactory.ZombieSkin(i + 1);
            LevelAssetFactory.SetFields(root.GetComponent<Zombie>(),
                ("animator", model.GetComponent<Animator>()),
                ("skins", skins),
                ("hitFlashColor", new Color(2.2f, 2.2f, 2.2f)),
                ("attackHitDelay", 0.45f),
                ("deathLinger", 4.5f));

            
            Transform bar = root.transform.Find("Health Bar");
            if (bar != null) bar.localPosition = new Vector3(0f, 1.45f, 0f);

            PrefabUtility.SaveAsPrefabAsset(root, ZombiePath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static void InstallBoss()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(BossPath);
        try
        {
            LevelAssetFactory.ApplyBossVisual(root);
            PrefabUtility.SaveAsPrefabAsset(root, BossPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
