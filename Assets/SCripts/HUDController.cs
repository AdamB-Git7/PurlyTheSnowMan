using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Score")]
    public Text scoreText;

    [Header("HUD Buttons")]
    public Button pauseButton;
    public Button restartButton;

    [Header("Pause Panel")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button restartInPauseButton;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public Text finalScoreText;

    private Button restartOnGameOverButton;
    private Button mainMenuOnGameOverButton;
    private Font runtimeFont;

    void Start()
    {
        runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (pauseButton != null) pauseButton.onClick.AddListener(OnPauseClicked);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
        if (restartInPauseButton != null) restartInPauseButton.onClick.AddListener(OnRestartClicked);

        SetPausePanelVisible(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (resumeButton != null) resumeButton.interactable = false;

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
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
        GameManager.Instance.OnGameOver -= ShowGameOver;
    }

    private void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score:D2}";
    }

    private void OnPauseClicked()
    {
        GameManager.Instance?.PauseGame();
        SetPausePanelVisible(true);
        if (resumeButton != null) resumeButton.interactable = true;
    }

    private void OnResumeClicked()
    {
        GameManager.Instance?.ResumeGame();
        SetPausePanelVisible(false);
        if (resumeButton != null) resumeButton.interactable = false;
    }

    private void OnRestartClicked()
    {
        GameManager.Instance?.RestartGame();
    }

    private void OnMainMenuClicked()
    {
        GameManager.Instance?.GoToMainMenu();
    }

    private void SetPausePanelVisible(bool visible)
    {
        if (pausePanel != null) pausePanel.SetActive(visible);
    }

    private void ShowGameOver()
    {
        SetPausePanelVisible(false);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            ConfigureGameOverLayout();
            if (finalScoreText != null)
                finalScoreText.text = $"Final Score: {GameManager.Instance?.CurrentScore ?? 0}";

            EnsureGameOverButtons();
        }
    }

    private void EnsureGameOverButtons()
    {
        if (gameOverPanel == null) return;

        if (restartOnGameOverButton == null)
        {
            restartOnGameOverButton = CreateGameOverButton("Restart", -95f);
            if (restartOnGameOverButton != null)
                restartOnGameOverButton.onClick.AddListener(OnRestartClicked);
        }

        if (mainMenuOnGameOverButton == null)
        {
            mainMenuOnGameOverButton = CreateGameOverButton("Main Menu", -165f);
            if (mainMenuOnGameOverButton != null)
                mainMenuOnGameOverButton.onClick.AddListener(OnMainMenuClicked);
        }
    }

    private void ConfigureGameOverLayout()
    {
        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        if (panelRect != null)
            panelRect.sizeDelta = new Vector2(500f, 300f);

        if (finalScoreText == null) return;

        RectTransform scoreRect = finalScoreText.GetComponent<RectTransform>();
        if (scoreRect != null)
        {
            scoreRect.anchorMin = new Vector2(0.5f, 1f);
            scoreRect.anchorMax = new Vector2(0.5f, 1f);
            scoreRect.pivot = new Vector2(0.5f, 1f);
            scoreRect.anchoredPosition = new Vector2(0f, -22f);
            scoreRect.sizeDelta = new Vector2(360f, 50f);
        }

        finalScoreText.alignment = TextAnchor.MiddleCenter;
        finalScoreText.fontSize = 36;
    }

    private Button CreateGameOverButton(string label, float topOffset)
    {
        GameObject buttonObject = new GameObject(label + " Button");
        buttonObject.transform.SetParent(gameOverPanel.transform, false);

        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = new Vector2(0f, topOffset);
        rectTransform.sizeDelta = new Vector2(220f, 55f);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.92f, 0.96f, 1f, 0.96f);

        Button button = buttonObject.AddComponent<Button>();
        buttonObject.transform.SetAsLastSibling();
        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;

        GameObject textObject = new GameObject("Label");
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        textObject.AddComponent<CanvasRenderer>();
        Text text = textObject.AddComponent<Text>();
        text.font = runtimeFont;
        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 26;
        text.fontStyle = FontStyle.Bold;
        text.color = new Color(0.1f, 0.18f, 0.28f, 1f);

        return button;
    }
}
