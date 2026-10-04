using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;







public static class ZombieModels
{
    public const string Dir = "Assets/Models/Zombie";
    public const string ModelPath = Dir + "/Zombie.fbx";
    public const string CrowdControllerPath = Dir + "/CrowdZombie.controller";
    public const string GameControllerPath = Dir + "/Zombie.controller";
    public const int Variants = 3;

    static readonly string[] CrowdClips = { "idle1", "idle2", "walk", "run", "roar" };
    static readonly string[] LoopingClips = { "idle1", "idle2", "walk", "run" };

    public static string ClipPath(string name) => $"{Dir}/Animations/zombie@{name}.fbx";
    public static string AlbedoPath(int variant) => $"{Dir}/Textures/Zombie{variant}_Albedo.png";
    public static string NormalPath(int variant) => $"{Dir}/Textures/Zombie{variant}_Normal.png";

    public static AnimationClip Clip(string name) =>
        AssetDatabase.LoadAllAssetsAtPath(ClipPath(name)).OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview__"));

    public static void Configure()
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (string path in AssetDatabase.FindAssets("t:Texture2D", new[] { Dir }).Select(AssetDatabase.GUIDToAssetPath))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                bool changed = false;
                if (path.EndsWith("_Normal.png") && importer.textureType != TextureImporterType.NormalMap)
                {
                    importer.textureType = TextureImporterType.NormalMap;
                    changed = true;
                }
                if (importer.maxTextureSize != 2048)
                {
                    importer.maxTextureSize = 2048;
                    changed = true;
                }
                if (changed) importer.SaveAndReimport();
            }

            foreach (string path in AssetDatabase.FindAssets("t:Model", new[] { Dir }).Select(AssetDatabase.GUIDToAssetPath))
            {
                var importer = (ModelImporter)AssetImporter.GetAtPath(path);
                bool isClip = path.Contains("/Animations/");
                bool changed = false;
                if (importer.animationType != ModelImporterAnimationType.Generic)
                {
                    importer.animationType = ModelImporterAnimationType.Generic;
                    changed = true;
                }
                if (importer.importAnimation != isClip)
                {
                    importer.importAnimation = isClip;
                    changed = true;
                }
                if (importer.materialImportMode != ModelImporterMaterialImportMode.None)
                {
                    importer.materialImportMode = ModelImporterMaterialImportMode.None;
                    changed = true;
                }
                if (importer.importCameras || importer.importLights)
                {
                    importer.importCameras = false;
                    importer.importLights = false;
                    changed = true;
                }
                if (isClip && ClipLoopsWrong(importer, LoopingClips.Contains(ClipName(path))))
                {
                    SetLoop(importer, LoopingClips.Contains(ClipName(path)));
                    changed = true;
                }
                if (changed) importer.SaveAndReimport();
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        AssetDatabase.Refresh();
        BuildCrowdController();
        BuildGameController();
    }

    static string ClipName(string path) => System.IO.Path.GetFileNameWithoutExtension(path).Replace("zombie@", "");

    static bool ClipLoopsWrong(ModelImporter importer, bool loop) =>
        importer.clipAnimations.Length == 0 || importer.clipAnimations.Any(c => c.loopTime != loop);

    static void SetLoop(ModelImporter importer, bool loop)
    {
        ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
        foreach (ModelImporterClipAnimation clip in clips)
        {
            clip.loopTime = loop;
            clip.loopPose = loop;
            
            clip.lockRootRotation = true;
            clip.lockRootHeightY = true;
            clip.lockRootPositionXZ = true;
            clip.keepOriginalOrientation = true;
            clip.keepOriginalPositionY = true;
            clip.keepOriginalPositionXZ = true;
        }
        importer.clipAnimations = clips;
    }

    static void BuildCrowdController()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CrowdControllerPath);
        if (controller != null && controller.layers[0].stateMachine.states.Length == CrowdClips.Length) return;
        if (controller != null) AssetDatabase.DeleteAsset(CrowdControllerPath);

        controller = AnimatorController.CreateAnimatorControllerAtPath(CrowdControllerPath);
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        foreach (string name in CrowdClips)
        {
            AnimationClip clip = Clip(name);
            if (clip == null) continue;
            AnimatorState state = machine.AddState(name);
            state.motion = clip;
            if (name == "walk") machine.defaultState = state;
        }
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
    }

    static void BuildGameController()
    {
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(GameControllerPath);
        if (existing != null && existing.layers[0].stateMachine.states.Length >= 7 && existing.layers[0].stateMachine.states.Any(q => q.state.name == "Attack2" && Mathf.Approximately(q.state.speed, 2.5f))) return;
        if (existing != null) AssetDatabase.DeleteAsset(GameControllerPath);

        var controller = AnimatorController.CreateAnimatorControllerAtPath(GameControllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        foreach (string trigger in new[] { "Attack1", "Attack2", "Hit", "Roar", "Death1", "Death2" })
            controller.AddParameter(trigger, AnimatorControllerParameterType.Trigger);
        AnimatorStateMachine machine = controller.layers[0].stateMachine;

        
        AnimatorState locomotion = controller.CreateBlendTreeInController("Locomotion", out BlendTree tree);
        tree.blendType = BlendTreeType.Simple1D;
        tree.blendParameter = "Speed";
        tree.useAutomaticThresholds = false;
        tree.AddChild(Clip("idle1"), 0f);
        tree.AddChild(Clip("walk"), 1.2f);
        tree.AddChild(Clip("run"), 3.2f);
        machine.defaultState = locomotion;

        
        
        
        AddAction(machine, locomotion, "Attack1", "atack1", "Attack1", 0.12f, 2f, 0.9f);
        AddAction(machine, locomotion, "Attack2", "atack2", "Attack2", 0.12f, 2.5f, 0.45f);
        AddAction(machine, locomotion, "Hit", "gethit", "Hit", 0.08f, 3f, 0.85f);
        AddAction(machine, locomotion, "Roar", "roar", "Roar", 0.15f, 1.3f, 0.32f);
        AddAction(machine, locomotion, "Death1", "death1", "Death1", 0.1f, 1.8f, -1f);
        AddAction(machine, locomotion, "Death2", "death2", "Death2", 0.1f, 1.6f, -1f);

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
    }

    
    static void AddAction(AnimatorStateMachine machine, AnimatorState back, string stateName, string clip, string trigger, float blend, float speed, float exitTime)
    {
        AnimatorState state = machine.AddState(stateName);
        state.motion = Clip(clip);
        state.speed = speed;

        AnimatorStateTransition enter = machine.AddAnyStateTransition(state);
        enter.AddCondition(AnimatorConditionMode.If, 0f, trigger);
        enter.hasExitTime = false;
        enter.duration = blend;
        enter.canTransitionToSelf = false;

        if (exitTime < 0f) return;
        AnimatorStateTransition leave = state.AddTransition(back);
        leave.hasExitTime = true;
        leave.exitTime = exitTime;
        leave.duration = 0.15f;
    }
}
