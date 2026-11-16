using System.Text.Json;

namespace Tetris;

/// <summary>
/// Immutable data transfer record persisted to disk for tracking the best run.
/// </summary>
public record HighScoreRecord(int HighScore, int MaxLines, DateTime AchievedAt);

/// <summary>
/// Handles loading and saving high-score data in JSON format next to the executable.
/// </summary>
public class ScoreManager
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public HighScoreRecord BestRecord { get; private set; }

    public ScoreManager()
    {
        _filePath = Path.Combine(AppContext.BaseDirectory, "highscores.json");
        BestRecord = Load();
    }

    /// <summary>
    /// Updates and persists the high score if the current run beats the saved record.
    /// </summary>
    public bool TryUpdateHighScore(int score, int lines)
    {
        if (score <= BestRecord.HighScore)
            return false;

        BestRecord = new HighScoreRecord(score, lines, DateTime.Now);
        Save(BestRecord);
        return true;
    }

    private HighScoreRecord Load()
    {
        try
        {
            if (!File.Exists(_filePath))
                return new HighScoreRecord(0, 0, DateTime.Now);

            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<HighScoreRecord>(json)
                   ?? new HighScoreRecord(0, 0, DateTime.Now);
        }
        catch
        {
            // Fallback to a clean record if the JSON file is corrupted or unreadable
            return new HighScoreRecord(0, 0, DateTime.Now);
        }
    }

    private void Save(HighScoreRecord record)
    {
        try
        {
            string json = JsonSerializer.Serialize(record, JsonOptions);
            File.WriteAllText(_filePath, json);
        }
        catch
        {
            // Ignore disk write errors (e.g., read-only directory) so gameplay is never interrupted
        }
    }
}