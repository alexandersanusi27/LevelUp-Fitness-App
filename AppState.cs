namespace LevelUp;

// global state holder - all pages read and write here instead of passing data around manually
public static class AppState
{
    public static int  CurrentXP            { get; set; } = 0;
    public static int  DailyQuestsCompleted { get; set; } = 0;
    public static int  MaxDailyQuests       { get; set; } = 4;
    public static int  XPPerQuest           { get; set; } = 50;
    public static bool WorkoutQuestDone     { get; set; } = false;
    public static bool StepsQuestDone       { get; set; } = false;
    public static bool PushupsQuestDone     { get; set; } = false;
    public static bool SitupsQuestDone      { get; set; } = false;
    public static bool SquatsQuestDone      { get; set; } = false;
    public static bool TTSEnabled           { get; set; } = true;

    // writes current state to device storage so it survives app restarts
    public static void Save()
    {
        Preferences.Default.Set("xp",              CurrentXP);
        Preferences.Default.Set("ttsEnabled",      TTSEnabled);
        Preferences.Default.Set("questsCompleted", DailyQuestsCompleted);
        Preferences.Default.Set("workoutDone",     WorkoutQuestDone);
        Preferences.Default.Set("stepsDone",       StepsQuestDone);
        Preferences.Default.Set("pushupsDone",     PushupsQuestDone);
        Preferences.Default.Set("situpsDone",      SitupsQuestDone);
        Preferences.Default.Set("squatsDone",      SquatsQuestDone);
        // store today's date so we can detect when midnight has passed
        Preferences.Default.Set("lastQuestDate",   DateTime.Today.ToString("yyyy-MM-dd"));
    }
}
