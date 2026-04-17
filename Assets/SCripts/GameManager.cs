using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game manager. Handles score tracking, Purly's death, pause/resume,
/// saving scores to a local file, and exposes game state to other systems.
/// Singleton pattern: one instance persists while the game scene is active.
/// </summary>
public class GameManager : MonoBehaviour
{
    // PlayerPrefs keys used to keep pause/resume data alive across scene reloads.
    private const string SavedScoreKey = "SavedGame.Score";
    private const string HasSavedGameKey = "SavedGame.HasSave";

    // Global reference used by UI and gameplay scripts.
    public static GameManager Instance { get; private set; }

    // True only while the player is actively playing.
    public bool IsPlaying { get; private set; }

    // The score for the current run.
    public int CurrentScore { get; private set; }

    // UI listens to this when it needs to refresh the on-screen score.
    public event Action<int> OnScoreChanged;

    // Other systems listen to this to react to the player dying.
    public event Action OnGameOver;

    // The player name is set in the menu scene before the game starts.
    private static string playerName = "Player";

    // The menu sets this to tell the next game scene load to resume saved score.
    public static bool ResumingSavedGame;

    // These static fields preserve a paused score across scene loads.
    private static int savedScore;
    private static bool hasSavedGame;

    public static void SetPlayerName(string name)
    {
        // Fall back to "Player" if the user leaves the field blank.
        playerName = string.IsNullOrWhiteSpace(name) ? "Player" : name.Trim();
    }

    // The menu uses this to pre-fill the name field with the latest value.
    public static string GetPlayerName()
    {
        return playerName;
    }

    void Awake()
    {
        // Keep a single manager instance in the active scene.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        // Start or resume the session as soon as the game scene loads.
        StartGame();
    }

    /// <summary>Begins or restarts a game session.</summary>
    public void StartGame()
    {
        // Rehydrate saved-game state in case Unity recreated static fields.
        LoadSavedGameState();

        // Resume the stored score only when the menu explicitly asked for it.
        CurrentScore = ResumingSavedGame ? savedScore : 0;

        // Consume the resume flag so future new games start from zero.
        ResumingSavedGame = false;

        // Mark gameplay as active again and unpause time.
        IsPlaying = true;
        Time.timeScale = 1f;

        // Immediately refresh any score UI.
        OnScoreChanged?.Invoke(CurrentScore);
    }

    /// <summary>Adds points to the current score and fires the score-changed event.</summary>
    public void AddScore(int points)
    {
        // Ignore score changes while the game is paused or over.
        if (!IsPlaying)
        {
            return;
        }

        // Increase the score by the requested amount.
        CurrentScore += points;

        // Notify any listeners that the displayed score should update.
        OnScoreChanged?.Invoke(CurrentScore);
    }

    /// <summary>Called when a snowball hits Purly. Ends the session and saves the score.</summary>
    public void OnPurlyDied()
    {
        // Prevent duplicate game-over handling.
        if (!IsPlaying)
        {
            return;
        }

        // Stop accepting gameplay actions and freeze the scene immediately.
        IsPlaying = false;
        Time.timeScale = 0f;

        // Persist this run to the score file.
        SaveScore();

        // Tell UI and other systems that the run has ended.
        OnGameOver?.Invoke();
    }

    /// <summary>Pauses the game and saves current score so it can be resumed.</summary>
    public void PauseGame()
    {
        // Ignore pauses if the game is already paused or over.
        if (!IsPlaying)
        {
            return;
        }

        // Mark gameplay inactive and freeze time.
        IsPlaying = false;
        Time.timeScale = 0f;

        // Remember the score so the menu's "Saved Game" option can restore it.
        savedScore = CurrentScore;
        hasSavedGame = true;

        // Persist the paused run so the resume button still works after scene reloads.
        SaveSavedGameState();
    }

    /// <summary>Resumes a paused game.</summary>
    public void ResumeGame()
    {
        // Reactivate gameplay and unfreeze time.
        IsPlaying = true;
        Time.timeScale = 1f;
    }

    /// <summary>Returns true if there is a saved (paused) game to resume.</summary>
    public static bool HasSavedGame()
    {
        // Rehydrate from PlayerPrefs when static state has not been initialized yet.
        if (!hasSavedGame)
        {
            hasSavedGame = PlayerPrefs.GetInt(HasSavedGameKey, 0) == 1;
        }

        return hasSavedGame;
    }

    /// <summary>Restores the saved score when resuming via Save Game button.</summary>
    public void RestoreSavedScore()
    {
        // Rehydrate saved-game state in case Unity recreated static fields.
        LoadSavedGameState();

        // Restore the saved score and refresh the HUD.
        CurrentScore = savedScore;
        OnScoreChanged?.Invoke(CurrentScore);
    }

    /// <summary>Saves the current score (with player name) to the local scores file.</summary>
    public void SaveScore()
    {
        // Delegate file writing to the shared score helper.
        ScoreUtility.SaveScore(playerName, CurrentScore);
    }

    /// <summary>Loads all score entries from the local file and returns the top scores.</summary>
    public List<ScoreEntry> LoadScores()
    {
        // Reuse the shared score parsing logic used by the menu screens.
        return ScoreUtility.LoadTopScores();
    }

    /// <summary>Reloads the active game scene to restart.</summary>
    public void RestartGame()
    {
        // Ensure gameplay resumes when the scene reloads.
        Time.timeScale = 1f;

        // Reload the current scene by build index.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Loads the landing/main menu scene (build index 0).</summary>
    public void GoToMainMenu()
    {
        // Ensure the menu is not loaded in a paused state.
        Time.timeScale = 1f;

        // Load the first scene in the build settings.
        SceneManager.LoadScene(0);
    }

    private static void SaveSavedGameState()
    {
        // Store whether a paused run exists.
        PlayerPrefs.SetInt(HasSavedGameKey, hasSavedGame ? 1 : 0);

        // Store the paused score that should be restored on resume.
        PlayerPrefs.SetInt(SavedScoreKey, savedScore);

        // Flush PlayerPrefs immediately so scene changes do not lose the data.
        PlayerPrefs.Save();
    }

    private static void LoadSavedGameState()
    {
        // Read the last known paused-game flag.
        hasSavedGame = PlayerPrefs.GetInt(HasSavedGameKey, hasSavedGame ? 1 : 0) == 1;

        // Read the last saved score, falling back to the current static value if present.
        savedScore = PlayerPrefs.GetInt(SavedScoreKey, savedScore);
    }
}

[Serializable]
public class ScoreEntry
{
    // The display name saved for the score row.
    public string playerName;

    // The number of points earned in that run.
    public int score;
}
