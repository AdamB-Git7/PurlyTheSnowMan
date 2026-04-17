using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Score")]
    // Text element that displays the live score during gameplay.
    public Text scoreText;

    [Header("HUD Buttons")]
    // Top-level pause button visible during play.
    public Button pauseButton;

    // Top-level restart button visible during play.
    public Button restartButton;

    [Header("Pause Panel")]
    // Pause overlay panel shown when the player pauses the game.
    public GameObject pausePanel;

    // Button that resumes the paused session.
    public Button resumeButton;

    // Restart button inside the pause panel.
    public Button restartInPauseButton;

    [Header("Game Over Panel")]
    // Panel shown after the player dies.
    public GameObject gameOverPanel;

    // Text element that shows the final score on the game-over panel.
    public Text finalScoreText;

    // These buttons are created at runtime the first time the game-over panel opens.
    private Button restartOnGameOverButton;
    private Button mainMenuOnGameOverButton;

    // Built-in font used for the runtime-created button labels.
    private Font runtimeFont;

    void Start()
    {
        // Load Unity's built-in legacy font for the runtime-generated button text.
        runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Attach all button callbacks that exist in the scene.
        BindButton(pauseButton, OnPauseClicked);
        BindButton(restartButton, OnRestartClicked);
        BindButton(resumeButton, OnResumeClicked);
        BindButton(restartInPauseButton, OnRestartClicked);

        // Start with overlays hidden and the resume button disabled.
        SetPausePanelVisible(false);
        SetActive(gameOverPanel, false);
        SetInteractable(resumeButton, false);

        // Subscribe to game events so the HUD stays in sync with gameplay state.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScoreDisplay;
            GameManager.Instance.OnGameOver += ShowGameOver;
            UpdateScoreDisplay(GameManager.Instance.CurrentScore);
        }
        else
        {
            Debug.LogWarning("HUDController: GameManager.Instance is null in Start.");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe safely when this HUD is destroyed to avoid stale listeners.
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
        GameManager.Instance.OnGameOver -= ShowGameOver;
    }

    private void UpdateScoreDisplay(int score)
    {
        // Refresh the visible score text if the reference exists.
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score:D2}";
        }
    }

    private void OnPauseClicked()
    {
        // Pause gameplay through the central manager.
        GameManager.Instance?.PauseGame();

        // Show pause UI and enable the resume button.
        SetPausePanelVisible(true);
        SetInteractable(resumeButton, true);
    }

    private void OnResumeClicked()
    {
        // Resume gameplay through the central manager.
        GameManager.Instance?.ResumeGame();

        // Hide pause UI and disable the resume button until the next pause.
        SetPausePanelVisible(false);
        SetInteractable(resumeButton, false);
    }

    private void OnRestartClicked()
    {
        // Restart the active game scene.
        GameManager.Instance?.RestartGame();
    }

    private void OnMainMenuClicked()
    {
        // Return to the menu scene.
        GameManager.Instance?.GoToMainMenu();
    }

    private void SetPausePanelVisible(bool visible)
    {
        // Show or hide the pause panel if it exists.
        SetActive(pausePanel, visible);
    }

    private void ShowGameOver()
    {
        // Hide the pause UI if it was open when the game ended.
        SetPausePanelVisible(false);

        // Nothing more to do if the scene has no game-over panel configured.
        if (gameOverPanel == null)
        {
            return;
        }

        // Show the panel before laying out its runtime content.
        gameOverPanel.SetActive(true);
        ConfigureGameOverLayout();

        // Show the final score captured by the manager.
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {GameManager.Instance?.CurrentScore ?? 0}";
        }

        // Create the restart and main menu buttons if they do not exist yet.
        EnsureGameOverButtons();
    }

    private void EnsureGameOverButtons()
    {
        // Guard against missing panel references.
        if (gameOverPanel == null)
        {
            return;
        }

        if (restartOnGameOverButton == null)
        {
            // Create the restart button once and keep the reference.
            restartOnGameOverButton = CreateGameOverButton("Restart", -95f);
            if (restartOnGameOverButton != null)
            {
                restartOnGameOverButton.onClick.AddListener(OnRestartClicked);
            }
        }

        if (mainMenuOnGameOverButton == null)
        {
            // Create the main menu button once and keep the reference.
            mainMenuOnGameOverButton = CreateGameOverButton("Main Menu", -165f);
            if (mainMenuOnGameOverButton != null)
            {
                mainMenuOnGameOverButton.onClick.AddListener(OnMainMenuClicked);
            }
        }
    }

    private void ConfigureGameOverLayout()
    {
        // Resize the panel to make room for the score text and two buttons.
        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.sizeDelta = new Vector2(500f, 300f);
        }

        // Stop here if there is no score text to position.
        if (finalScoreText == null)
        {
            return;
        }

        // Position the score text near the top-center of the panel.
        RectTransform scoreRect = finalScoreText.GetComponent<RectTransform>();
        if (scoreRect != null)
        {
            scoreRect.anchorMin = new Vector2(0.5f, 1f);
            scoreRect.anchorMax = new Vector2(0.5f, 1f);
            scoreRect.pivot = new Vector2(0.5f, 1f);
            scoreRect.anchoredPosition = new Vector2(0f, -22f);
            scoreRect.sizeDelta = new Vector2(360f, 50f);
        }

        // Apply text styling so the score reads clearly.
        finalScoreText.alignment = TextAnchor.MiddleCenter;
        finalScoreText.fontSize = 36;
    }

    private Button CreateGameOverButton(string label, float topOffset)
    {
        // Create a new child object under the game-over panel.
        GameObject buttonObject = new GameObject(label + " Button");
        buttonObject.transform.SetParent(gameOverPanel.transform, false);

        // Position the button relative to the top-center of the panel.
        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = new Vector2(0f, topOffset);
        rectTransform.sizeDelta = new Vector2(220f, 55f);

        // Add a visible background to the button.
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.92f, 0.96f, 1f, 0.96f);

        // Add Unity's clickable button component.
        Button button = buttonObject.AddComponent<Button>();

        // Keep runtime-created buttons above earlier panel children.
        buttonObject.transform.SetAsLastSibling();

        // Disable directional navigation because these buttons are mouse-driven.
        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;

        // Create a child text object that fills the whole button area.
        GameObject textObject = new GameObject("Label");
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Add the Text component used for the button caption.
        textObject.AddComponent<CanvasRenderer>();
        Text text = textObject.AddComponent<Text>();
        text.font = runtimeFont;
        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 26;
        text.fontStyle = FontStyle.Bold;
        text.color = new Color(0.1f, 0.18f, 0.28f, 1f);

        // Return the finished button so click handlers can be attached.
        return button;
    }

    private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        // Skip missing references gracefully instead of throwing null errors.
        if (button != null)
        {
            button.onClick.AddListener(action);
        }
    }

    private static void SetActive(GameObject target, bool isActive)
    {
        // Unity UI objects are often optional in small projects, so guard the reference.
        if (target != null)
        {
            target.SetActive(isActive);
        }
    }

    private static void SetInteractable(Button button, bool isInteractable)
    {
        // Avoid repeating the same null-check pattern at each call site.
        if (button != null)
        {
            button.interactable = isInteractable;
        }
    }
}
