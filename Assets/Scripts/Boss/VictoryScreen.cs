using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;



public class VictoryScreen : MonoBehaviour
{
    [SerializeField] string mainMenuScene = "MainMenu";
    [SerializeField] float fadeInTime = 1.2f;

    CanvasGroup group;
    float shownAt;
    bool leaving;

    public static void Show()
    {
        if (FindAnyObjectByType<VictoryScreen>() != null) return;
        new GameObject("Victory Screen").AddComponent<VictoryScreen>();
    }

    void Awake()
    {
        float runTime = Time.time - RunState.StartedAt;
        shownAt = Time.unscaledTime;

        
        Time.timeScale = 0f;
        foreach (MonoBehaviour b in new MonoBehaviour[] { FindAnyObjectByType<FPSController>(), FindAnyObjectByType<WeaponManager>() })
            if (b != null) b.enabled = false;
        var health = FindAnyObjectByType<PlayerHealth>();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Canvas canvas = RuntimeUI.Canvas("Victory Canvas", 20);
        canvas.transform.SetParent(transform, false);
        group = canvas.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;

        var center = new Vector2(0.5f, 0.5f);
        RuntimeUI.Stretch(canvas.transform, "Backdrop").gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(0.01f, 0.05f, 0.03f, 0.9f);
        RuntimeUI.Text(canvas.transform, "Title", "YOU FOUND THE CURE", 110, new Color(0.35f, 1f, 0.45f), center, new Vector2(0f, 230f), new Vector2(1600f, 140f));
        RuntimeUI.Text(canvas.transform, "Subtitle", "Patient Zero is dead and the briefcase is yours. Humanity gets a second chance.", 30,
            new Color(0.8f, 0.85f, 0.8f), center, new Vector2(0f, 135f), new Vector2(1500f, 50f));

        int minutes = Mathf.FloorToInt(runTime / 60f), seconds = Mathf.FloorToInt(runTime % 60f);
        string stats = $"Run time  {minutes}:{seconds:00}\n" +
                       $"Zombies killed  {RunState.Kills}\n" +
                       $"Tokens left over  {RunState.Tokens}\n" +
                       $"Health left  {(health != null ? Mathf.CeilToInt(health.CurrentHealth) : 0)}";
        RuntimeUI.Text(canvas.transform, "Stats", stats, 32, new Color(1f, 0.85f, 0.5f), center, new Vector2(0f, -10f), new Vector2(1000f, 200f));
        RuntimeUI.Button(canvas.transform, "Main Menu Button", "MAIN MENU", new Vector2(0f, -200f), new Vector2(320f, 64f), GoToMainMenu);
        RuntimeUI.Text(canvas.transform, "Hint", "Press Space to return to the main menu", 22, new Color(0.6f, 0.65f, 0.6f), center, new Vector2(0f, -270f), new Vector2(900f, 34f));

        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
    }

    void Update()
    {
        group.alpha = Mathf.Clamp01((Time.unscaledTime - shownAt) / fadeInTime);
        Keyboard keyboard = Keyboard.current;
        if (group.alpha >= 1f && keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame))
            GoToMainMenu();
    }

    void GoToMainMenu()
    {
        if (leaving) return;
        leaving = true;
        
        RunState.Reset();
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }
}
