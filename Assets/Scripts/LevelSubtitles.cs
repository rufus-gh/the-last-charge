using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;






public class LevelSubtitles : MonoBehaviour
{
    const string FirstLevel = "Level_01";
    const string IntroScene = "Intro";

    static readonly Dictionary<string, string[]> Lines = new Dictionary<string, string[]>
    {
        [FirstLevel] = new[]
        {
            "The trading floor. Main power was long gone. All you have is the flashlight. Press F to turn it on.",
            "The owners of Spectre Tower had sealed every floor into a maze. It kept the dead from reaching anyone in a straight line.",
            "Your flashlight is losing power. The battery drains while it's on.",
            "Find batteries to charge it back up. When the light dies, you can't see them coming.",
        },
        ["Level_02"] = new[] { "The hospital. Where the first patients were brought in.", "It was supposed to be the safe place." },
        ["Level_03"] = new[] { "The server room.", "Somewhere in those racks, the broadcast is still running on whatever power is left." },
        ["Level_04"] = new[] { "The archive.", "The deeper you go, the less power the tower has left." },
        ["Level_05"] = new[] { "The executive floor.", "The people who signed off on all of this had been the first to run." },
        ["Level_06"] = new[] { "The service level. No lights at all.", "Down here, the dark isn't empty." },
        ["Level_07"] = new[] { "The laboratory. The cure is here.", "So is the thing that is guarding it." },
    };

    public static readonly string[] BossLines = { "Patient Zero. The first one.", "Bigger than the others, and slower to fall." };

    const float StartDelay = 1.2f;
    const float LineTime = 4.2f;
    const float Gap = 0.35f;
    const float Fade = 0.4f;

    static string previousScene;
    static LevelSubtitles current;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        
        previousScene = null;
        current = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void FirstScene()
    {
        if (previousScene == null) OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Single) return;
        string from = previousScene;
        previousScene = scene.name;
        if (!Lines.TryGetValue(scene.name, out string[] lines)) return;
        if (scene.name == FirstLevel && from != null && from != IntroScene) return;
        Show(lines, StartDelay);
    }

    
    public static void Show(string[] lines, float delay = 0f)
    {
        if (current != null) Destroy(current.gameObject);
        current = new GameObject("Level Subtitles").AddComponent<LevelSubtitles>();
        current.lines = lines;
        current.delay = delay;
    }

    string[] lines;
    float delay;
    Text label;

    void Start()
    {
        Canvas canvas = RuntimeUI.Canvas("Subtitles Canvas", 50);
        canvas.transform.SetParent(transform, false);
        label = RuntimeUI.Text(canvas.transform, "Subtitle", "", 30, Color.white,
            new Vector2(0.5f, 0f), new Vector2(0f, 190f), new Vector2(1300f, 90f));
        label.GetComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.9f);
        SetAlpha(0f);
        StartCoroutine(Play());
    }

    
    IEnumerator Play()
    {
        yield return new WaitForSeconds(delay);
        foreach (string line in lines)
        {
            label.text = line;
            for (float t = 0f; t < LineTime; t += Time.deltaTime)
            {
                SetAlpha(Mathf.Min(1f, t / Fade, (LineTime - t) / Fade));
                yield return null;
            }
            SetAlpha(0f);
            yield return new WaitForSeconds(Gap);
        }
        Destroy(gameObject);
    }

    void SetAlpha(float a)
    {
        Color c = label.color;
        c.a = Mathf.Clamp01(a);
        label.color = c;
    }
}
