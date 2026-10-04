using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;




public class DeathScreen : MonoBehaviour
{
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] CanvasGroup group;
    [SerializeField] GameObject buttonsRoot;
    [Tooltip("Primary action: restart the run from floor 1")]
    [SerializeField] Button respawnButton;
    [SerializeField] Button mainMenuButton;
    [Tooltip("Shows the time left before the primary action fires")]
    [SerializeField] Text countdownLabel;
    [Tooltip("Player scripts switched off while dead (movement, gun...)")]
    [SerializeField] Behaviour[] disableWhileDead;
    [SerializeField] string mainMenuScene = "MainMenu";
    [SerializeField] string firstLevelScene = "Level_01";

    [Header("Timing")]
    [SerializeField] float fadeInTime = 1f;
    [SerializeField] float buttonsDelay = 1.2f;
    [Tooltip("Auto-triggers the primary action this long after dying")]
    [SerializeField] float autoDelay = 4f;

    bool showing;
    bool leaving;
    float diedAt;

    void Awake()
    {
        respawnButton.onClick.AddListener(Continue);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        SetLabel(respawnButton, "RESPAWN");
        Hide();
    }

    void OnEnable()
    {
        if (playerHealth != null) playerHealth.Died += Show;
    }

    void OnDisable()
    {
        if (playerHealth != null) playerHealth.Died -= Show;
    }

    void Update()
    {
        if (!showing) return;

        float t = Time.unscaledTime - diedAt;
        group.alpha = Mathf.Clamp01(t / fadeInTime);

        if (t >= buttonsDelay && !buttonsRoot.activeSelf)
        {
            buttonsRoot.SetActive(true);
            group.interactable = true;
        }

        float left = autoDelay - t;
        if (countdownLabel != null)
        {
            countdownLabel.text = left > 0f ? $"Respawning at the start in {Mathf.CeilToInt(left)}..." : "";
        }
        if (left <= 0f) Continue();
    }

    void Show()
    {
        showing = true;
        diedAt = Time.unscaledTime;
        group.alpha = 0f;
        group.blocksRaycasts = true;
        group.interactable = false;
        buttonsRoot.SetActive(false);
        if (countdownLabel != null) countdownLabel.text = "";

        foreach (Behaviour b in disableWhileDead) if (b != null) b.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Hide()
    {
        showing = false;
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
        buttonsRoot.SetActive(false);
    }

    
    void Continue()
    {
        if (leaving) return;
        leaving = true;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        RunState.Reset();
        SceneManager.LoadScene(firstLevelScene);
    }

    void GoToMainMenu()
    {
        if (leaving) return;
        leaving = true;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        
        RunState.Reset();
        SceneManager.LoadScene(mainMenuScene);
    }

    static void SetLabel(Button button, string text)
    {
        if (button == null) return;
        Text label = button.GetComponentInChildren<Text>();
        if (label != null) label.text = text;
    }
}
