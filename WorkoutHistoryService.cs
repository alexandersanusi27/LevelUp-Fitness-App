using System.Text.Json;

namespace LevelUp;

// represents one exercise entry as it gets saved to disk
// separate from the page's ExerciseEntry class so the file format stays stable
public class SavedExercise
{
    public string Name   { get; set; } = "";
    public double Weight { get; set; }
    public int    Reps   { get; set; }
    public bool   Done   { get; set; }

    public override string ToString() => $"{Name} \u2014 {Weight}kg x {Reps}";
}

// one workout session = a date + the exercises logged that day
public class WorkoutSession
{
    public string             Date      { get; set; } = "";
    public List<SavedExercise> Exercises { get; set; } = new();
}

// handles reading and writing workout history to a JSON file in the app data folder
// the file persists between app restarts and is private to the app (not visible to the user)
public static class WorkoutHistoryService
{
    // path to the history file inside the app's private data directory
    private static readonly string FilePath =
        Path.Combine(FileSystem.AppDataDirectory, "workout_history.json");

    // in-memory copy of all sessions - loaded once at startup
    private static List<WorkoutSession> _sessions = new();

    // reads the history file from disk - call this once when the page opens
    public static void Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return;
            string json = File.ReadAllText(FilePath);
            _sessions = JsonSerializer.Deserialize<List<WorkoutSession>>(json) ?? new();
        }
        catch { _sessions = new(); } // if the file is corrupt just start fresh
    }

    // saves the current list of exercises as todays session
    // if a session for today already exists it replaces it (safe to call after every add)
    public static void SaveToday(IEnumerable<SavedExercise> exercises)
    {
        string today   = DateTime.Today.ToString("yyyy-MM-dd");
        var    session = _sessions.FirstOrDefault(s => s.Date == today);

        if (session != null)
            session.Exercises = exercises.ToList();
        else
            _sessions.Add(new WorkoutSession { Date = today, Exercises = exercises.ToList() });

        WriteFile();
    }

    // returns todays saved exercises so the page can restore them after an app restart
    public static List<SavedExercise> GetToday()
    {
        string today = DateTime.Today.ToString("yyyy-MM-dd");
        return _sessions.FirstOrDefault(s => s.Date == today)?.Exercises ?? new();
    }

    // returns past sessions (not today) sorted newest first, capped at 7 days
    public static List<WorkoutSession> GetHistory()
    {
        string today = DateTime.Today.ToString("yyyy-MM-dd");
        return _sessions
            .Where(s => s.Date != today && s.Exercises.Count > 0)
            .OrderByDescending(s => s.Date)
            .Take(7)
            .ToList();
    }

    // writes the full session list to disk as formatted JSON
    private static void WriteFile()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(FilePath, JsonSerializer.Serialize(_sessions, options));
        }
        catch { } // silent fail - history is nice to have, not critical
    }
}
