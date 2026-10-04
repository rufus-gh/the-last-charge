using UnityEngine;



[RequireComponent(typeof(MazeGenerator))]
public class CeilingLightPlacer : MonoBehaviour
{
    [Tooltip("So the first thing the player sees isn't pitch black")]
    [SerializeField] bool alwaysLightStart = true;

    public void Place(MazeGenerator maze)
    {
        Transform parent = maze.ResetGeneratedChild("Lights");
        LevelTheme theme = maze.Theme;
        
        if (theme == null || theme.lightFixture == null || theme.lightChance <= 0f) return;

        var rng = new System.Random(maze.Seed + 2);
        for (int x = 0; x < maze.GridSize.x; x++)
        for (int z = 0; z < maze.GridSize.y; z++)
        {
            var cell = new Vector2Int(x, z);
            bool forced = alwaysLightStart && cell == maze.StartCell;
            if (!forced && rng.NextDouble() >= theme.lightChance) continue;

            GameObject fixture = MazeGenerator.SpawnPrefab(theme.lightFixture, parent);
            
            bool openNorthSouth = !maze.HasWall(cell, 0) || !maze.HasWall(cell, 2);
            bool openEastWest = !maze.HasWall(cell, 1) || !maze.HasWall(cell, 3);
            Quaternion along = Quaternion.LookRotation(maze.DirToWorld(openNorthSouth && !openEastWest ? 1 : 0));
            fixture.transform.SetPositionAndRotation(maze.CellCenter(cell) + Vector3.up * maze.WallHeight, along);

            if (!forced && rng.NextDouble() < theme.deadChance) MakeDead(fixture, theme.deadFixtureMaterial);
            else if (rng.NextDouble() < theme.flickerChance)
            {
                var flicker = fixture.GetComponent<FixtureFlicker>();
                if (flicker == null) flicker = fixture.AddComponent<FixtureFlicker>();
                flicker.enabled = true;
            }
        }
    }

    static void MakeDead(GameObject fixture, Material deadMaterial)
    {
        foreach (Light l in fixture.GetComponentsInChildren<Light>()) l.enabled = false;
        if (deadMaterial == null) return;
        foreach (Renderer r in fixture.GetComponentsInChildren<Renderer>())
            if (r.sharedMaterial != null && r.sharedMaterial.IsKeywordEnabled("_EMISSION")) r.sharedMaterial = deadMaterial;
    }
}
