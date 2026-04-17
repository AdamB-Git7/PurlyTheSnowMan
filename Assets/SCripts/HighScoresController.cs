using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Drives the dedicated High Scores scene. Loads the top 5 scores from the local
/// scores file, displays them in descending order with player names, and provides
/// a Back button to return to the main menu.
/// </summary>
public class HighScoresController : MonoBehaviour
{
    [Header("UI References")]
    // Text element that shows the ranked score list.
    public Text highScoresText;

    // Button used to return to the main menu scene.
    public Button backButton;

    [Header("Scene")]
    [Tooltip("Build index of the main menu scene.")]
    // Scene index for the menu.
    public int mainMenuSceneBuildIndex = 0;

    void Start()
    {
        // Wire the Back button once when the scene starts.
        backButton.onClick.AddListener(OnBackClicked);

        // Fill the screen with the latest saved scores.
        DisplayHighScores();
    }

    /// <summary>Reads and displays the top 5 scores in descending order.</summary>
    private void DisplayHighScores()
    {
        // Load, sort, trim, and format scores using the shared helper.
        List<ScoreEntry> entries = ScoreUtility.LoadTopScores();
        highScoresText.text = ScoreUtility.BuildHighScoreDisplayText(entries);
    }

    private void OnBackClicked()
    {
        // Return to the configured main menu scene.
        SceneManager.LoadScene(mainMenuSceneBuildIndex);
    }
}
