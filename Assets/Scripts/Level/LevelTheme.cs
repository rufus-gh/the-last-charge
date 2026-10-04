using System.Collections.Generic;
using UnityEngine;




[CreateAssetMenu(menuName = "Losing Power/Level Theme", fileName = "LevelTheme")]
public class LevelTheme : ScriptableObject
{
    [Tooltip("Shown on the between-level screen, e.g. \"Furnace Level (B4)\". Empty = the asset name.")]
    public string displayName;

    [Header("Surfaces")]
    public Material wallMaterial;
    public Material floorMaterial;
    public Material ceilingMaterial;
    public float wallHeight = 3f;

    [Header("Rooms")]
    [Tooltip("Open rooms carved into the maze")]
    public int roomCount = 4;
    public Vector2Int roomSizeRange = new Vector2Int(2, 3);
    [Tooltip("Size in cells of an open room at the spawn (0 = none), e.g. a trading floor")]
    public Vector2Int startRoomSize;
    public RoomFurnishing startRoomFurnishing;
    [Tooltip("Furnishing for carved rooms, used in order (MRI room, boardroom, stacks...). Rooms past the end of the list get wall props.")]
    public List<RoomFurnishing> featureRooms = new List<RoomFurnishing>();

    [Header("Props")]
    [Tooltip("Furniture against corridor walls. Prefab front (+Z) faces away from the wall.")]
    public List<PropEntry> wallProps = new List<PropEntry>();
    [Tooltip("Furniture against walls inside rooms (patient room beds...). Empty = use wallProps.")]
    public List<PropEntry> roomWallProps = new List<PropEntry>();
    [Tooltip("Small loose items on the floor, no colliders")]
    public List<PropEntry> clutter = new List<PropEntry>();
    [Range(0f, 1f)] public float wallPropChance = 0.65f;
    public int maxWallPropsPerCell = 2;
    public int maxClutterPerCell = 3;
    [Tooltip("Hung from the ceiling along corridors (overhead pipes). Prefab runs along local X, origin at the ceiling.")]
    public List<PropEntry> ceilingProps = new List<PropEntry>();
    [Range(0f, 1f)] public float ceilingPropChance;
    [Tooltip("Placed in ceiling corners where two walls meet (CobwebPlacer). Origin on the corner point at the ceiling, +Z out of the corner.")]
    public List<GameObject> cobwebs = new List<GameObject>();
    [Range(0f, 1f)] public float cobwebChance;
    [Tooltip("Chance that a wall line gets service pipes running along it under the ceiling (PipeRunPlacer)")]
    [Range(0f, 1f)] public float pipeChance;
    public List<PipeRun> pipeRuns = new List<PipeRun>();

    [Header("Windows (outer walls only)")]
    [Range(0f, 1f)] public float windowChance;
    [Tooltip("Window prefabs: origin at the bottom middle of the opening, front (+Z) facing into the building")]
    public List<PropEntry> windows = new List<PropEntry>();
    public float windowSill = 0.9f;
    [Tooltip("Wall left above the window opening")]
    public float windowHeader = 0.4f;
    [Tooltip("Fraction of the cell width the opening takes")]
    [Range(0.3f, 0.9f)] public float windowWidth = 0.7f;

    [Header("Outside (only matters with windows)")]
    [Tooltip("Sky, sun and a city skyline around the building")]
    public bool outdoors;
    public Material skybox;
    public Color sunColor = new Color(1f, 0.95f, 0.85f);
    public float sunIntensity = 1.6f;
    public Vector3 sunEuler = new Vector3(35f, 40f, 0f);
    [Tooltip("How far below this floor the street is")]
    public float cityElevation = 60f;
    public Material cityFacadeMaterial;
    public Material cityGroundMaterial;

    [Header("Lights")]
    public GameObject lightFixture;
    [Tooltip("Swapped onto the glowing parts of dead fixtures")]
    public Material deadFixtureMaterial;
    [Range(0f, 1f)] public float lightChance = 0.25f;
    [Range(0f, 1f)] public float flickerChance = 0.3f;
    [Range(0f, 1f)] public float deadChance = 0.2f;

    [Header("Atmosphere")]
    public Color ambientColor = new Color(0.035f, 0.06f, 0.06f);
    public Color fogColor = new Color(0.015f, 0.04f, 0.04f);
    [Range(0f, 0.2f)] public float fogDensity = 0.06f;

    [Header("Exit")]
    public GameObject exitPrefab;
    [Tooltip("Replaces the exit door: a goal in the middle of a room (the cure). Picking it up wins the game.")]
    public GameObject goalPrefab;
    [Tooltip("Guards the goal")]
    public GameObject bossPrefab;
    [Tooltip("Size in cells of the boss arena the route ends in (far corner from the spawn). 0 = put the goal in an ordinary room.")]
    public Vector2Int goalRoomSize;
}



[System.Serializable]
public class PipeRun
{
    public GameObject segment;
    public GameObject end;
}

[System.Serializable]
public class PropEntry
{
    public GameObject prefab;
    public float weight = 1f;
    [Range(0f, 1f)] public float tipOverChance;
}

public enum RoomLayout
{
    
    Center,
    
    Grid,
}

[System.Serializable]
public class RoomFurnishing
{
    public GameObject prefab;
    public RoomLayout layout = RoomLayout.Center;
    [Tooltip("Grid only: distance between prop centres (x along the room, y across)")]
    public Vector2 spacing = new Vector2(3f, 3f);
    [Tooltip("Grid only: clear walkway kept around the room edge")]
    public float edgeMargin = 1.4f;
}
