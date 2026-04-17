using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Drives the dedicated High Scores scene. Loads the top 5 scores from the local
/// scores file, displays them in descending order with player names, and provides
/// a Back button to return to the main menu.
/// </summary>
public class HighScoresController : MonoBehaviour
{
    [Header("UI References")]
    public Text highScoresText;
    public Button backButton;

    [Header("Scene")]
    [Tooltip("Build index of the main menu scene.")]
    public int mainMenuSceneBuildIndex = 0;

    private static readonly string ScoreFileName = "scores.txt";

    void Start()
    {
        backButton.onClick.AddListener(OnBackClicked);
        DisplayHighScores();
    }

    /// <summary>Reads and displays the top 5 scores in descending order.</summary>
    private void DisplayHighScores()
    {
        string filePath = Path.Combine(Application.persistentDataPath, ScoreFileName);
        List<ScoreEntry> entries = LoadScores(filePath);

        if (entries.Count == 0)
        {
            highScoresText.text = "No scores yet.\nPlay a game to set your first record!";
            return;
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== High Scores ===\n");
        for (int i = 0; i < entries.Count; i++)
            sb.AppendLine($"{i + 1}.  {entries[i].playerName}  —  {entries[i].score} pts");

        highScoresText.text = sb.ToString();
    }

    private void OnBackClicked()
    {
        SceneManager.LoadScene(mainMenuSceneBuildIndex);
    }

    /// <summary>Loads all score entries from the file and returns the top 5 sorted descending.</summary>
    private List<ScoreEntry> LoadScores(string filePath)
    {
        var entries = new List<ScoreEntry>();
        if (!File.Exists(filePath)) return entries;

        foreach (string line in File.ReadAllLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] parts = line.Split(',');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int score))
                entries.Add(new ScoreEntry { playerName = parts[0], score = score });
        }

        entries.Sort((a, b) => b.score.CompareTo(a.score));
        if (entries.Count > 5) entries = entries.GetRange(0, 5);
        return entries;
    }
}
