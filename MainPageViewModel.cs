using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace LevelUp;

// mini viewmodel for a single quest row
// the XAML binds directly to these properties so we dont need code-behind logic for each quest
public class QuestViewModel : INotifyPropertyChanged
{
    private bool _isDone;

    public string Name        { get; init; } = "";
    public string Description { get; init; } = "";

    public bool IsDone
    {
        get => _isDone;
        set
        {
            if (_isDone == value) return;
            _isDone = value;
            // notify all the display properties that depend on this flag
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusIcon));
            OnPropertyChanged(nameof(StatusIconColor));
            OnPropertyChanged(nameof(ButtonText));
            OnPropertyChanged(nameof(ButtonEnabled));
            OnPropertyChanged(nameof(ButtonColor));
        }
    }

    // computed display properties - the XAML just binds to these directly
    public string StatusIcon      => IsDone ? "\u2713" : "\u25cb";
    public Color  StatusIconColor => IsDone ? Color.FromArgb("#00FF88") : Color.FromArgb("#778899");
    public string ButtonText      => IsDone ? "DONE" : "COMPLETE";
    public bool   ButtonEnabled   => !IsDone;
    public Color  ButtonColor     => IsDone ? Color.FromArgb("#1A2A1A") : Color.FromArgb("#3D5AFE");

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// main viewmodel for the dashboard page
// holds all the state and commands - the page just handles hardware stuff and dialogs
public class MainPageViewModel : INotifyPropertyChanged
{
    // rank data - same as before just moved here so the VM owns it
    private readonly string[] rankNames      = { "E", "D", "C", "B", "A", "S" };
    private readonly string[] rankTitles     = { "Iron Hunter", "Bronze Hunter", "Silver Hunter", "Gold Hunter", "Platinum Hunter", "Shadow Monarch" };
    private readonly string[] rankColours    = { "#778899", "#5588BB", "#3D5AFE", "#7B61FF", "#9966FF", "#FFD700" };
    private readonly int[]    rankThresholds = { 0, 200, 400, 700, 1000, 1500 };

    private readonly string[] rankUpMessages =
    {
        "You have risen. The hunt begins.",
        "Your power grows. The shadows stir.",
        "The system acknowledges your strength.",
        "Formidable. Few reach this rank.",
        "Elite. The gates tremble before you.",
        "You have become the Shadow Monarch. Arise."
    };

    // track current rank internally so we can detect changes
    private int    _rankIndex = 0;
    private string _rank      = "E";

    // events raised when something happens that needs a UI response (dialog, sound, animation)
    // the page subscribes to these - keeps UI stuff out of the viewmodel
    public event Func<string, string, string, Task>? RankUpOccurred;
    public event Func<string, int, bool, Task>?      QuestCompleted;
    public event Func<Task>?                          DecayOccurred;
    public event Func<Task>?                          RankMaintained;
    public event Func<string, Task>?                  ShakeQuoteRequested;

    // the 4 daily quests as their own mini-viewmodels
    public QuestViewModel StepsQuest   { get; } = new() { Name = "Walk 10,000 Steps", Description = "Daily step goal" };
    public QuestViewModel PushupsQuest { get; } = new() { Name = "Do 50 Push-ups",    Description = "Upper body strength" };
    public QuestViewModel SitupsQuest  { get; } = new() { Name = "Do 50 Sit-ups",     Description = "Core strength" };
    public QuestViewModel SquatsQuest  { get; } = new() { Name = "Do 100 Squats",     Description = "Lower body strength" };

    // commands bound to the buttons in XAML
    public ICommand CompleteStepsQuestCommand   { get; }
    public ICommand CompletePushupsQuestCommand { get; }
    public ICommand CompleteSitupsQuestCommand  { get; }
    public ICommand CompleteSquatsQuestCommand  { get; }
    public ICommand ResetQuestsCommand          { get; }
    public ICommand SimulateDecayCommand        { get; }
    public ICommand NavigateToWorkoutLogCommand { get; }
    public ICommand NavigateToHelpCommand       { get; }
    public ICommand TestNotificationCommand     { get; }
    public ICommand TestShakeQuoteCommand       { get; }

    public MainPageViewModel()
    {
        CompleteStepsQuestCommand   = new Command(async () => await CompleteQuestAsync(StepsQuest));
        CompletePushupsQuestCommand = new Command(async () => await CompleteQuestAsync(PushupsQuest));
        CompleteSitupsQuestCommand  = new Command(async () => await CompleteQuestAsync(SitupsQuest));
        CompleteSquatsQuestCommand  = new Command(async () => await CompleteQuestAsync(SquatsQuest));
        ResetQuestsCommand          = new Command(ResetQuests);
        SimulateDecayCommand        = new Command(async () => await SimulateDecayAsync());
        NavigateToWorkoutLogCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(WorkoutLogPage)));
        NavigateToHelpCommand       = new Command(async () => await Shell.Current.GoToAsync(nameof(HelpPage)));
        TestNotificationCommand     = new Command(async () => await NotificationService.SendTestNotificationAsync());
        TestShakeQuoteCommand       = new Command(TriggerShakeQuote);
    }

    // ── Bindable Properties ──────────────────────────────────────────────────

    public int CurrentXP
    {
        get => AppState.CurrentXP;
        set
        {
            AppState.CurrentXP = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(XPLabelText));
            OnPropertyChanged(nameof(XPToNextText));
            OnPropertyChanged(nameof(XPProgress));
        }
    }

    public string RankLetter => _rank;
    public string RankTitle  => rankTitles[_rankIndex];
    public Color  RankColour => Color.FromArgb(rankColours[_rankIndex]);

    public string XPLabelText  => $"Total XP: {CurrentXP}";
    public string XPToNextText
    {
        get
        {
            int next = _rankIndex < rankThresholds.Length - 1 ? rankThresholds[_rankIndex + 1] : -1;
            return next == -1 ? "MAX RANK \u2014 Shadow Monarch" : $"XP to next rank: {next - CurrentXP}";
        }
    }

    public double XPProgress
    {
        get
        {
            int next = _rankIndex < rankThresholds.Length - 1 ? rankThresholds[_rankIndex + 1] : -1;
            if (next == -1) return 1.0;
            int into  = CurrentXP - rankThresholds[_rankIndex];
            int range = next - rankThresholds[_rankIndex];
            return Math.Clamp((double)into / range, 0, 1);
        }
    }

    public string QuestCountText  => $"Daily Quests Completed: {AppState.DailyQuestsCompleted} / {AppState.MaxDailyQuests}";
    public double QuestProgress   => (double)AppState.DailyQuestsCompleted / AppState.MaxDailyQuests;
    public string QuestStatusText => AppState.DailyQuestsCompleted >= AppState.MaxDailyQuests
        ? "All daily quests complete! Outstanding work, Hunter."
        : "Complete quests to earn XP and rank up.";

    public bool TTSEnabled
    {
        get => AppState.TTSEnabled;
        set { AppState.TTSEnabled = value; OnPropertyChanged(); AppState.Save(); }
    }

    public bool SRankBannerVisible   => _rank == "S";
    public bool SimulateDecayVisible => _rank == "S";

    // accessibility descriptions read aloud by screen readers (WCAG 1.1.1)
    public string RankBadgeAccessibilityText     => $"Current rank: {_rank}, {rankTitles[_rankIndex]}";
    public string XPProgressAccessibilityText    => $"XP progress: {Math.Round(XPProgress * 100)}% through current rank";
    public string QuestProgressAccessibilityText => $"Quest progress: {AppState.DailyQuestsCompleted} of {AppState.MaxDailyQuests} quests completed";

    // ── Internal Logic ────────────────────────────────────────────────────────

    private void UpdateRank()
    {
        for (int i = rankThresholds.Length - 1; i >= 0; i--)
        {
            if (CurrentXP >= rankThresholds[i])
            {
                _rankIndex = i;
                _rank = rankNames[i];
                break;
            }
        }
        OnPropertyChanged(nameof(RankLetter));
        OnPropertyChanged(nameof(RankTitle));
        OnPropertyChanged(nameof(RankColour));
        OnPropertyChanged(nameof(SRankBannerVisible));
        OnPropertyChanged(nameof(SimulateDecayVisible));
        OnPropertyChanged(nameof(RankBadgeAccessibilityText));
    }

    public async Task AddXPAsync(int amount)
    {
        string previousRank = _rank;
        CurrentXP += amount;
        UpdateRank();

        if (_rank != previousRank && RankUpOccurred != null)
            await RankUpOccurred.Invoke(_rank, rankTitles[_rankIndex], rankUpMessages[_rankIndex]);
    }

    private async Task CompleteQuestAsync(QuestViewModel quest)
    {
        if (quest.IsDone) return;
        if (AppState.DailyQuestsCompleted >= AppState.MaxDailyQuests) return;

        HapticClick();
        quest.IsDone = true;
        AppState.DailyQuestsCompleted++;

        if (quest == StepsQuest)   AppState.StepsQuestDone   = true;
        if (quest == PushupsQuest) AppState.PushupsQuestDone = true;
        if (quest == SitupsQuest)  AppState.SitupsQuestDone  = true;
        if (quest == SquatsQuest)  AppState.SquatsQuestDone  = true;

        await AddXPAsync(AppState.XPPerQuest);
        NotifyQuestProperties();
        AppState.Save();

        bool allDone = AppState.DailyQuestsCompleted >= AppState.MaxDailyQuests;
        if (allDone) NotificationService.CancelDailyReminder();

        if (QuestCompleted != null)
            await QuestCompleted.Invoke(quest.Name, AppState.XPPerQuest, allDone);
    }

    public void ResetQuests()
    {
        HapticClick();
        AppState.DailyQuestsCompleted = 0;
        AppState.WorkoutQuestDone     = false;
        AppState.StepsQuestDone       = false;
        AppState.PushupsQuestDone     = false;
        AppState.SitupsQuestDone      = false;
        AppState.SquatsQuestDone      = false;

        StepsQuest.IsDone   = false;
        PushupsQuest.IsDone = false;
        SitupsQuest.IsDone  = false;
        SquatsQuest.IsDone  = false;

        NotifyQuestProperties();
        AppState.Save();
    }

    private async Task SimulateDecayAsync()
    {
        if (AppState.DailyQuestsCompleted < 2)
        {
            CurrentXP = 1000;
            UpdateRank();
            AppState.Save();
            if (DecayOccurred != null) await DecayOccurred.Invoke();
        }
        else
        {
            if (RankMaintained != null) await RankMaintained.Invoke();
        }
    }

    private void TriggerShakeQuote()
    {
        if (ShakeQuoteRequested != null)
            _ = ShakeQuoteRequested.Invoke(GetRandomQuote());
    }

    public void SyncFromAppState()
    {
        UpdateRank();
        OnPropertyChanged(nameof(CurrentXP));
        OnPropertyChanged(nameof(XPLabelText));
        OnPropertyChanged(nameof(XPToNextText));
        OnPropertyChanged(nameof(XPProgress));
        NotifyQuestProperties();

        StepsQuest.IsDone   = AppState.StepsQuestDone;
        PushupsQuest.IsDone = AppState.PushupsQuestDone;
        SitupsQuest.IsDone  = AppState.SitupsQuestDone;
        SquatsQuest.IsDone  = AppState.SquatsQuestDone;

        // if overnight decay was applied, fire the event so the page shows the alert
        if (AppState.DecayAppliedOnLoad)
        {
            AppState.DecayAppliedOnLoad = false;
            if (DecayOccurred != null)
                _ = DecayOccurred.Invoke();
        }
    }

    // the 15 motivational quotes shown on shake
    private static readonly string[] SystemQuotes =
    {
        "The system watches. Every step forward is recorded.",
        "Pain is the price of strength. Pay it without complaint.",
        "You are not yet at your limit. Push further.",
        "The weak grow stronger. The strong grow unstoppable.",
        "Even the mightiest hunters began at Rank E.",
        "Rest is not failure. Recovery is part of the grind.",
        "Your body remembers every rep. So does the system.",
        "A single step taken daily outweighs a sprint taken once.",
        "The dungeon does not care about your excuses.",
        "Arise. Your potential has not yet been realised.",
        "Shadows follow the strong. Keep moving.",
        "There is no shortcut to Rank S. Only persistence.",
        "The system has chosen you. Do not waste the opportunity.",
        "Fatigue is temporary. Rank is permanent.",
        "Every hunter who quit also had reasons. Rise anyway."
    };

    public static string GetRandomQuote() => SystemQuotes[Random.Shared.Next(SystemQuotes.Length)];

    private void NotifyQuestProperties()
    {
        OnPropertyChanged(nameof(QuestCountText));
        OnPropertyChanged(nameof(QuestProgress));
        OnPropertyChanged(nameof(QuestStatusText));
        OnPropertyChanged(nameof(QuestProgressAccessibilityText));
    }

    private static void HapticClick()
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
