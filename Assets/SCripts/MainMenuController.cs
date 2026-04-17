using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Drives the landing/main-menu scene. Handles Enter Name, New Game, Saved Game,
/// High Scores (shown as an overlay panel in this scene), and Exit.
/// High scores are listed in descending order with player names, top 5.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Player Name")]
    // Field where the player types their display name.
    public InputField playerNameInput;

    [Header("Main Buttons")]
    // Starts a fresh game.
    public Button newGameButton;

    // Resumes a paused game if one exists.
    public Button savedGameButton;

    // Opens the high-score overlay.
    public Button highScoresButton;

    // Closes the application.
    public Button exitButton;

    [Header("Panels")]
    // Panel that contains the main menu buttons.
    public GameObject buttonPanel;

    // Overlay used to show the score list in this same scene.
    public GameObject highScoresPanel;

    // Text element inside the high-score overlay.
    public Text highScoresText;

    // Button that closes the score overlay.
    public Button closeHighScoresButton;

    [Header("Scene")]
    [Tooltip("Build index of the game scene.")]
    // Scene index for the gameplay scene.
    public int gameSceneBuildIndex = 1;

    void Start()
    {
        // Show the main menu panel and hide the score overlay on startup.
        SetActive(highScoresPanel, false);
        SetActive(buttonPanel, true);

        // Hook up all menu button actions.
        newGameButton.onClick.AddListener(OnNewGame);
        savedGameButton.onClick.AddListener(OnSavedGame);
        highScoresButton.onClick.AddListener(OnHighScores);
        exitButton.onClick.AddListener(OnExit);

        if (closeHighScoresButton != null)
        {
            closeHighScoresButton.onClick.AddListener(OnCloseHighScores);
        }

        // Restore the last-used player name into the input field.
        if (playerNameInput != null)
        {
            playerNameInput.text = GameManager.GetPlayerName();
        }

        // Reflect whether a paused run is actually available to resume.
        if (savedGameButton != null)
        {
            savedGameButton.interactable = GameManager.HasSavedGame();
        }
    }

    private void OnNewGame()
    {
        // Store the current name before loading the game scene.
        ApplyPlayerName();

        // Start the game scene fresh.
        SceneManager.LoadScene(gameSceneBuildIndex);
    }

    private void OnSavedGame()
    {
        // Only continue if a paused game was actually stored.
        if (!GameManager.HasSavedGame())
        {
            return;
        }

        // Keep the current name and tell the game scene to resume the saved score.
        ApplyPlayerName();
        GameManager.ResumingSavedGame = true;

        // Load the game scene in resume mode.
        SceneManager.LoadScene(gameSceneBuildIndex);
    }

    /// <summary>Hides the main buttons and shows the High Scores overlay panel.</summary>
    private void OnHighScores()
    {
        // If the overlay was not assigned, there is nothing to show.
        if (highScoresPanel == null)
        {
            return;
        }

        // Load the latest score entries and convert them into display text.
        List<ScoreEntry> entries = ScoreUtility.LoadTopScores();
        if (highScoresText != null)
        {
            highScoresText.text = ScoreUtility.BuildHighScoreDisplayText(entries);
        }

        // Swap panels so the overlay becomes visible.
        SetActive(buttonPanel, false);
        highScoresPanel.SetActive(true);
    }

    private void OnCloseHighScores()
    {
        // Hide the overlay and restore the regular menu buttons.
        SetActive(highScoresPanel, false);
        SetActive(buttonPanel, true);
    }

    private void OnExit()
    {
        // Quit the built game.
        Application.Quit();

#if UNITY_EDITOR
        // Also stop Play Mode when running inside the Unity Editor.
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void ApplyPlayerName()
    {
        // Save the current input field value back into the shared manager.
        if (playerNameInput != null)
        {
            GameManager.SetPlayerName(playerNameInput.text);
        }
    }

    private static void SetActive(GameObject target, bool isActive)
    {
        // Small helper to reduce repeated null checks when toggling panels.
        if (target != null)
        {
            target.SetActive(isActive);
        }
    }
}
