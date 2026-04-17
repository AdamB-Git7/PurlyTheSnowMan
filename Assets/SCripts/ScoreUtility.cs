using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Centralizes score-file behavior so menu and gameplay code do not have to
/// duplicate the same file parsing, formatting, and sorting logic.
/// </summary>
public static class ScoreUtility
{
    // Keep the score filename in one place so every script uses the same file.
    private const string ScoreFileName = "scores.txt";

    // Only the top 5 scores are shown in the UI.
    private const int MaxScoresToKeep = 5;

    /// <summary>
    /// Builds the full path to the persistent score file.
    /// Unity stores this in a safe per-application data folder.
    /// </summary>
    public static string GetScoreFilePath()
    {
        return Path.Combine(Application.persistentDataPath, ScoreFileName);
    }

    /// <summary>
    /// Appends one score entry to the score file.
    /// </summary>
    public static void SaveScore(string playerName, int score)
    {
        // Include the timestamp so saved lines remain readable if inspected manually.
        string entry = $"{playerName},{score},{DateTime.Now:yyyy-MM-dd HH:mm}";

        // Append instead of overwrite so all historical scores remain available.
        File.AppendAllText(GetScoreFilePath(), entry + Environment.NewLine);

        // Log the final destination to help during debugging.
        Debug.Log($"Score saved: {entry} -> {GetScoreFilePath()}");
    }

    /// <summary>
    /// Loads, sorts, and trims score entries from the local file.
    /// </summary>
    public static List<ScoreEntry> LoadTopScores()
    {
        string filePath = GetScoreFilePath();
        var entries = new List<ScoreEntry>();

        // If the file does not exist yet, there are simply no scores to show.
        if (!File.Exists(filePath))
        {
            return entries;
        }

        foreach (string line in File.ReadAllLines(filePath))
        {
            // Ignore blank lines so malformed spacing does not break parsing.
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            // The game stores data as: playerName,score,timestamp
            string[] parts = line.Split(',');

            // Only accept rows where the score column parses correctly.
            if (parts.Length >= 2 && int.TryParse(parts[1], out int score))
            {
                entries.Add(new ScoreEntry
                {
                    playerName = parts[0],
                    score = score
                });
            }
        }

        // Highest score should appear first.
        entries.Sort((left, right) => right.score.CompareTo(left.score));

        // Only keep the top N entries used by the UI.
        if (entries.Count > MaxScoresToKeep)
        {
            entries = entries.GetRange(0, MaxScoresToKeep);
        }

        return entries;
    }

    /// <summary>
    /// Converts score entries into the menu/high-score screen display format.
    /// </summary>
    public static string BuildHighScoreDisplayText(List<ScoreEntry> entries)
    {
        // Provide a friendly empty state before any games have been played.
        if (entries == null || entries.Count == 0)
        {
            return "No scores yet.\nPlay a game to set your first record!";
        }

        var builder = new StringBuilder();

        // Add a simple title line above the ranked list.
        builder.AppendLine("=== High Scores ===");
        builder.AppendLine();

        for (int index = 0; index < entries.Count; index++)
        {
            ScoreEntry entry = entries[index];
            builder.AppendLine($"{index + 1}.  {entry.playerName}  -  {entry.score} pts");
        }

        return builder.ToString();
    }
}
