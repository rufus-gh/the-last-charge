using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;





public static class VaultPlacement
{
    [MenuItem("Losing Power/Move Vault To Back Wall")]
    public static void Apply()
    {
        var maze = Object.FindAnyObjectByType<MazeGenerator>();
        var vault = Object.FindAnyObjectByType<CureBriefcase>();
        if (maze == null || vault == null || maze.GoalRoom.width <= 0)
        {
            Debug.LogWarning("Open the lab level (a maze with a cure vault and a boss arena) first.");
            return;
        }

        if (!maze.TryGetGoalBackPlacement(out Vector3 position, out Vector3 facing)) return;
        Undo.RecordObject(vault.transform, "Move vault to back wall");
        vault.transform.SetPositionAndRotation(new Vector3(position.x, vault.transform.position.y, position.z), Quaternion.LookRotation(facing));
        EditorUtility.SetDirty(vault.transform);

        MazeLevelBuilder.BakeNavMesh(maze);
        EditorSceneManager.MarkSceneDirty(maze.gameObject.scene);
        Debug.Log("Vault moved to " + vault.transform.position + " and the NavMesh re-baked. Save the scene.");
    }
}
