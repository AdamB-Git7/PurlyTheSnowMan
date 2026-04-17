using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Drives the landing/main-menu scene. Handles Enter Name, New Game, Saved Game,
/// High Scores (shown as an overlay panel in this scene), and Exit.
/// High scores are listed in descending order with player names, top 5.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Player Name")]
    public InputField playerNameInput;

    [Header("Main Buttons")]
    public Button newGameButton;
    public Button savedGameButton;
    public Button highScoresButton;
    public Button exitButton;

    [Header("Panels")]
    public GameObject buttonPanel;
    public GameObject highScoresPanel;
    public Text highScoresText;
    public Button closeHighScoresButton;

    [Header("Scene")]
    [Tooltip("Build index of the game scene.")]
    public int gameSceneBuildIndex = 1;

    private static readonly string ScoreFileName = "scores.txt";

    void Start()
    {
        if (highScoresPanel != null) highScoresPanel.SetActive(false);
        if (buttonPanel != null) buttonPanel.SetActive(true);

        newGameButton.onClick.AddListener(OnNewGame);
        savedGameButton.onClick.AddListener(OnSavedGame);
        highScoresButton.onClick.AddListener(OnHighScores);
        exitButton.onClick.AddListener(OnExit);

        if (closeHighScoresButton != null)
            closeHighScoresButton.onClick.AddListener(OnCloseHighScores);

        if (playerNameInput != null)
            playerNameInput.text = GameManager.GetPlayerName();
    }

    private void OnNewGame()
    {
        ApplyPlayerName();
        SceneManager.LoadScene(gameSceneBuildIndex);
    }

    private void OnSavedGame()
    {
        if (!GameManager.HasSavedGame()) return;
        ApplyPlayerName();
        GameManager.ResumingSavedGame = true;
        SceneManager.LoadScene(gameSceneBuildIndex);
    }

    /// <summary>Hides the main buttons and shows the High Scores overlay panel.</summary>
    private void OnHighScores()
    {
        if (highScoresPanel == null) return;

        string filePath = Path.Combine(Application.persistentDataPath, ScoreFileName);
        List<ScoreEntry> entries = LoadScores(filePath);

        if (entries.Count == 0)
        {
            highScoresText.text = "No scores yet.\nPlay a game to set your first record!";
        }
        else
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== High Scores ===\n");
            for (int i = 0; i < entries.Count; i++)
                sb.AppendLine($"{i + 1}.  {entries[i].playerName}  —  {entries[i].score} pts");
            highScoresText.text = sb.ToString();
        }

        if (buttonPanel != null) buttonPanel.SetActive(false);
        highScoresPanel.SetActive(true);
    }

    private void OnCloseHighScores()
    {
        if (highScoresPanel != null) highScoresPanel.SetActive(false);
        if (buttonPanel != null) buttonPanel.SetActive(true);
    }

    private void OnExit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void ApplyPlayerName()
    {
        if (playerNameInput != null)
            GameManager.SetPlayerName(playerNameInput.text);
    }

    /// <summary>Reads score entries from the local file, sorted descending, top 5.</summary>
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
