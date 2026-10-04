using System.Linq;
using UnityEditor;
using UnityEngine;



public static class IntroModels
{
    public const string Dir = "Assets/Models/Intro";

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
                if (!importer.importCameras && !importer.importLights) continue;
                importer.importCameras = false;
                importer.importLights = false;
                importer.SaveAndReimport();
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        AssetDatabase.Refresh();
    }
}
