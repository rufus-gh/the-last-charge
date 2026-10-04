using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Video;







public class CureBriefcase : MonoBehaviour
{
    [SerializeField] GameObject bossPrefab;
    [Tooltip("Where the boss stands guard, relative to the pedestal")]
    [SerializeField] Vector3 bossOffset = new Vector3(0f, 0f, 3f);
    [Tooltip("Lifts up into the ceiling when the case unlocks")]
    [SerializeField] Transform glassCase;
    [SerializeField] float caseLift = 2.4f;
    [SerializeField] Transform briefcase;
    [Tooltip("Escape ladder to the surface; the finale sends the player here once the cure is taken")]
    [SerializeField] Transform escapeLadder;
    [Tooltip("The city-explosion render played at the climax. Leave empty for a placeholder card.")]
    [SerializeField] VideoClip explosionVideo;
    [SerializeField] Light[] alarmLights;
    [SerializeField] Color unlockedColor = new Color(0.3f, 1f, 0.45f);
    [SerializeField] float promptDistance = 4.5f;
    [Tooltip("Walk this close to the pedestal to take the cure once it's unlocked")]
    [SerializeField] float takeDistance = 1.9f;

    public bool Unlocked { get; private set; }

    BossZombie boss;
    Transform player;
    Text prompt;
    Vector3 caseStart;
    Vector3 briefcaseStart;
    float lift;
    bool taken;

    void Start()
    {
        var controller = FindAnyObjectByType<CharacterController>();
        if (controller != null) player = controller.transform;
        if (glassCase != null) caseStart = glassCase.localPosition;
        if (briefcase != null) briefcaseStart = briefcase.localPosition;

        Canvas canvas = RuntimeUI.Canvas("Cure Prompt", 4);
        prompt = RuntimeUI.Text(canvas.transform, "Prompt", "", 30, new Color(0.85f, 1f, 0.75f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -160f), new Vector2(1200f, 50f));

        SpawnBoss();
    }

    void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Unlock();
            return;
        }
        Vector3 spot = transform.TransformPoint(bossOffset);
        if (NavMesh.SamplePosition(spot, out NavMeshHit hit, 3f, NavMesh.AllAreas)) spot = hit.position;
        GameObject go = Instantiate(bossPrefab, spot, Quaternion.LookRotation(transform.forward));
        boss = go.GetComponent<BossZombie>();
        if (boss == null)
        {
            Unlock();
            return;
        }
        boss.Health.Died += Unlock;
        
        var maze = FindAnyObjectByType<MazeGenerator>();
        if (maze != null && maze.GoalRoom.width > 0)
            boss.SetArena(maze.RoomCenter(maze.GoalRoom), new Vector3(maze.GoalRoom.width, 1f, maze.GoalRoom.height) * maze.CellSize);
        BossHealthBar.Create(boss);
    }

    void Unlock()
    {
        if (Unlocked) return;
        Unlocked = true;
        foreach (Light l in alarmLights)
            if (l != null) l.color = unlockedColor;
    }

    void Update()
    {
        if (Unlocked && lift < 1f)
        {
            lift = Mathf.MoveTowards(lift, 1f, Time.deltaTime / 1.5f);
            if (glassCase != null) glassCase.localPosition = caseStart + Vector3.up * caseLift * Mathf.SmoothStep(0f, 1f, lift);
        }
        
        if (Unlocked && briefcase != null)
        {
            briefcase.localPosition = briefcaseStart + Vector3.up * (0.12f * lift + Mathf.Sin(Time.time * 2f) * 0.03f * lift);
            briefcase.Rotate(0f, 40f * lift * Time.deltaTime, 0f, Space.World);
        }

        if (taken || player == null) return;
        Vector3 offset = player.position - transform.position;
        offset.y = 0f;
        float distance = offset.magnitude;

        if (Unlocked && distance < takeDistance)
        {
            Take();
            return;
        }
        prompt.text = distance > promptDistance ? ""
            : Unlocked ? "The vault is open. Take the cure!"
            : "The cure is locked in the vault. Kill Patient Zero to unlock it.";
    }

    void Take()
    {
        taken = true;
        prompt.text = "";
        if (briefcase != null) briefcase.gameObject.SetActive(false);
        FinaleDirector.Begin(escapeLadder, explosionVideo);
    }
}
