namespace LevelUp;

// basically this is like a global variable holder for the whole app
// instead of passing data between pages manually, everything just reads/writes here
// not the most elegant solution but it works and its simple lol
public static class AppState
{
    // how much XP the user has in total - starts at 0 obviously
    public static int CurrentXP { get; set; } = 0;

    // tracks how many of the 4 daily quests theyve done today
    public static int DailyQuestsCompleted { get; set; } = 0;

    // there are 4 quests per day, hardcoded for now
    public static int MaxDailyQuests { get; set; } = 4;

    // each quest gives 50 XP, easy to change later if needed
    public static int XPPerQuest { get; set; } = 50;

    // this one is for the workout log page - first exercise logged counts as a quest
    public static bool WorkoutQuestDone { get; set; } = false;

    // individual flags for each quest so we know which ones are ticked off
    // once these are true the button on the dashboard gets disabled
    public static bool StepsQuestDone   { get; set; } = false;
    public static bool PushupsQuestDone { get; set; } = false;
    public static bool SitupsQuestDone  { get; set; } = false;
    public static bool SquatsQuestDone  { get; set; } = false;

    // lets the user turn off the voice announcements if theyre in public lol
    public static bool TTSEnabled { get; set; } = true;

    // set to true by Load() when overnight decay is applied, so the UI can show the alert
    public static bool DecayAppliedOnLoad { get; set; } = false;

    // ── Persistence ──────────────────────────────────────────────────────────

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
        // store todays date so we can detect when midnight has passed
        Preferences.Default.Set("lastQuestDate",   DateTime.Today.ToString("yyyy-MM-dd"));
    }

    // reads state from device storage on startup
    // if the stored date is from a previous day, quest flags are automatically reset
    public static void Load()
    {
        CurrentXP  = Preferences.Default.Get("xp",         0);
        TTSEnabled = Preferences.Default.Get("ttsEnabled",  true);

        string savedDate = Preferences.Default.Get("lastQuestDate", "");
        string today     = DateTime.Today.ToString("yyyy-MM-dd");

        if (savedDate == today)
        {
            // same day - restore quest progress exactly as it was
            DailyQuestsCompleted = Preferences.Default.Get("questsCompleted", 0);
            WorkoutQuestDone     = Preferences.Default.Get("workoutDone",     false);
            StepsQuestDone       = Preferences.Default.Get("stepsDone",       false);
            PushupsQuestDone     = Preferences.Default.Get("pushupsDone",     false);
            SitupsQuestDone      = Preferences.Default.Get("situpsDone",      false);
            SquatsQuestDone      = Preferences.Default.Get("squatsDone",      false);
        }
        else
        {
            // check if S-rank decay should apply before resetting yesterdays quest count
            int yesterdayQuests = Preferences.Default.Get("questsCompleted", 0);
            if (CurrentXP >= 1500 && yesterdayQuests < 2)
            {
                CurrentXP = 1000;
                DecayAppliedOnLoad = true;
            }

            // new day - wipe quest flags but keep XP (rank is preserved)
            DailyQuestsCompleted = 0;
            WorkoutQuestDone     = false;
            StepsQuestDone       = false;
            PushupsQuestDone     = false;
            SitupsQuestDone      = false;
            SquatsQuestDone      = false;
            // save immediately so the new date is recorded
            Save();
        }
    }
}
