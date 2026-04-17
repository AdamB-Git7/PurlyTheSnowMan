using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the in-game HUD: live score display and the pause/resume/restart buttons.
/// At game start: Resume is inactive, PausePanel is hidden.
/// While paused: PausePanel is shown (with Resume + Restart inside).
/// On game over: GameOverPanel is shown.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Score")]
    public Text scoreText;

    [Header("HUD Buttons (always visible)")]
    public Button pauseButton;
    public Button restartButton;

    [Header("Pause Panel (shown while paused)")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button restartInPauseButton;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public Text finalScoreText;

    void Start()
    {
        // Wire buttons first — these never depend on GameManager
        if (pauseButton   != null) pauseButton.onClick.AddListener(OnPauseClicked);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (resumeButton  != null) resumeButton.onClick.AddListener(OnResumeClicked);
        if (restartInPauseButton != null) restartInPauseButton.onClick.AddListener(OnRestartClicked);

        // Initial panel states
        SetPausePanelVisible(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (resumeButton  != null) resumeButton.interactable = false;

        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScoreDisplay;
            GameManager.Instance.OnGameOver     += ShowGameOver;
            UpdateScoreDisplay(GameManager.Instance.CurrentScore);
        }
        else
        {
            Debug.LogWarning("HUDController: GameManager.Instance is null in Start. Score events will not work.");
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
        GameManager.Instance.OnGameOver     -= ShowGameOver;
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

    private void SetPausePanelVisible(bool visible)
    {
        if (pausePanel != null) pausePanel.SetActive(visible);
    }

    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText != null)
                finalScoreText.text = $"Final Score: {GameManager.Instance?.CurrentScore}";
        }
    }
}
