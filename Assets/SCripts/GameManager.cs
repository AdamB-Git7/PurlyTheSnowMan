using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game manager. Handles score tracking, Purly's death, pause/resume,
/// saving scores to a local file, and exposes game state to other systems.
/// Singleton pattern — one instance persists while the game scene is active.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsPlaying { get; private set; } = false;

    public int CurrentScore { get; private set; } = 0;

    public event Action<int> OnScoreChanged;
    public event Action OnGameOver;

    private static readonly string ScoreFileName = "scores.txt";
    private string ScoreFilePath => Path.Combine(Application.persistentDataPath, ScoreFileName);

    // Player name set from the landing page
    private static string playerName = "Player";

    public static void SetPlayerName(string name)
    {
        playerName = string.IsNullOrWhiteSpace(name) ? "Player" : name.Trim();
    }

    public static string GetPlayerName() => playerName;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        StartGame();
    }

    // Flag set by MainMenuController to signal a save-game resume
    public static bool ResumingSavedGame = false;

    /// <summary>Begins or restarts a game session.</summary>
    public void StartGame()
    {
        CurrentScore = ResumingSavedGame ? savedScore : 0;
        ResumingSavedGame = false;
        IsPlaying = true;
        Time.timeScale = 1f;
        OnScoreChanged?.Invoke(CurrentScore);
    }

    /// <summary>Adds points to the current score and fires the score-changed event.</summary>
    public void AddScore(int points)
    {
        if (!IsPlaying) return;
        CurrentScore += points;
        OnScoreChanged?.Invoke(CurrentScore);
    }

    /// <summary>Called when a snowball hits Purly. Ends the session and saves the score.</summary>
    public void OnPurlyDied()
    {
        if (!IsPlaying) return;
        IsPlaying = false;
        Time.timeScale = 0f;
        SaveScore();
        OnGameOver?.Invoke();
    }

    // Persists the last paused score so "Save Game" can resume it
    private static int savedScore = 0;
    private static bool hasSavedGame = false;

    /// <summary>Pauses the game and saves current score so it can be resumed.</summary>
    public void PauseGame()
    {
        if (!IsPlaying) return;
        IsPlaying = false;
        Time.timeScale = 0f;
        savedScore = CurrentScore;
        hasSavedGame = true;
    }

    /// <summary>Resumes a paused game.</summary>
    public void ResumeGame()
    {
        IsPlaying = true;
        Time.timeScale = 1f;
    }

    /// <summary>Returns true if there is a saved (paused) game to resume.</summary>
    public static bool HasSavedGame() => hasSavedGame;

    /// <summary>Restores the saved score when resuming via Save Game button.</summary>
    public void RestoreSavedScore()
    {
        CurrentScore = savedScore;
        OnScoreChanged?.Invoke(CurrentScore);
    }

    /// <summary>Saves the current score (with player name) to the local scores file.</summary>
    public void SaveScore()
    {
        string entry = $"{playerName},{CurrentScore},{DateTime.Now:yyyy-MM-dd HH:mm}";
        File.AppendAllText(ScoreFilePath, entry + Environment.NewLine);
        Debug.Log($"Score saved: {entry} → {ScoreFilePath}");
    }

    /// <summary>Loads all score entries from the local file. Returns newest-first list.</summary>
    public List<ScoreEntry> LoadScores()
    {
        var entries = new List<ScoreEntry>();
        if (!File.Exists(ScoreFilePath)) return entries;

        string[] lines = File.ReadAllLines(ScoreFilePath);
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] parts = line.Split(',');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int score))
            {
                entries.Add(new ScoreEntry { playerName = parts[0], score = score });
            }
        }

        // Sort descending by score
        entries.Sort((a, b) => b.score.CompareTo(a.score));

        // Keep last 5
        if (entries.Count > 5) entries = entries.GetRange(0, 5);
        return entries;
    }

    /// <summary>Reloads the active game scene to restart.</summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Loads the landing/main menu scene (build index 0).</summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}

[Serializable]
public class ScoreEntry
{
    public string playerName;
    public int score;
}
