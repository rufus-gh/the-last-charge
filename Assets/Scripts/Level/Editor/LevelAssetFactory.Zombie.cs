using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;






public static partial class LevelAssetFactory
{
    public const string ZombieModelDir = "Assets/Resources/Zombies";
    
    public const string ZombieModelResource = "Zombies/ZombieModel";

    public static string ZombieModelPath(int variant) => $"{ZombieModelDir}/ZombieModel{variant}.prefab";

    static void BuildZombieModels()
    {
        ZombieModels.Configure();
        for (int i = 1; i <= ZombieModels.Variants; i++)
        {
            int variant = i;
            SavePrefab(() => ZombieVisual(variant, ZombieModels.GameControllerPath, $"ZombieModel{variant}", true), ZombieModelPath(variant), PrefabKind.Fixture);
        }
    }

    
    
    public static void RebuildZombiePrefabs()
    {
        EnsureAssets();
        overwrite = true;
        try
        {
            ZombieModels.Configure();
            for (int i = 1; i <= ZombieModels.Variants; i++)
            {
                int variant = i;
                SavePrefab(() => ZombieVisual(variant, ZombieModels.GameControllerPath, $"ZombieModel{variant}", true), ZombieModelPath(variant), PrefabKind.Fixture);
                SavePrefab(() => CrowdZombie(variant), CrowdZombiePath(variant), PrefabKind.Fixture);
            }
            SavePrefab(SpecimenTank, SpecimenTankPath, PrefabKind.Prop, AddTankGlow);
        }
        finally
        {
            overwrite = false;
        }
        AssetDatabase.SaveAssets();
    }

    public static Material ZombieSkin(int variant) =>
        TexturedMat($"Zombie/Skin{variant}", ZombieModels.AlbedoPath(variant), ZombieModels.NormalPath(variant), null, 0.12f);

    
    
    static GameObject ZombieVisual(int variant, string controllerPath, string name, bool castShadows)
    {
        Material skin = ZombieSkin(variant);
        GameObject zombie = Model(ZombieModels.ModelPath, name);
        foreach (SkinnedMeshRenderer r in zombie.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            r.sharedMaterial = skin;
            r.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            r.updateWhenOffscreen = false;
        }

        var animator = zombie.GetComponent<Animator>();
        if (animator == null) animator = zombie.AddComponent<Animator>();
        if (animator.avatar == null)
            animator.avatar = AssetDatabase.LoadAllAssetsAtPath(ZombieModels.ModelPath).OfType<Avatar>().FirstOrDefault();
        animator.runtimeAnimatorController = Load<RuntimeAnimatorController>(controllerPath);
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        return zombie;
    }
}
