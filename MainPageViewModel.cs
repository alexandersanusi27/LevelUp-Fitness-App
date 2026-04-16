using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace LevelUp;

// mini viewmodel for a single quest row
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
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusIcon));
            OnPropertyChanged(nameof(StatusIconColor));
            OnPropertyChanged(nameof(ButtonText));
            OnPropertyChanged(nameof(ButtonEnabled));
            OnPropertyChanged(nameof(ButtonColor));
        }
    }

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
public class MainPageViewModel : INotifyPropertyChanged
{
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

    private int    _rankIndex = 0;
    private string _rank      = "E";

    // events raised when something happens that needs a UI response
    public event Func<string, string, string, Task>? RankUpOccurred;
    public event Func<string, int, bool, Task>?      QuestCompleted;
    public event Func<Task>?                          DecayOccurred;
    public event Func<Task>?                          RankMaintained;

    public QuestViewModel StepsQuest   { get; } = new() { Name = "Walk 10,000 Steps", Description = "Daily step goal" };
    public QuestViewModel PushupsQuest { get; } = new() { Name = "Do 50 Push-ups",    Description = "Upper body strength" };
    public QuestViewModel SitupsQuest  { get; } = new() { Name = "Do 50 Sit-ups",     Description = "Core strength" };
    public QuestViewModel SquatsQuest  { get; } = new() { Name = "Do 100 Squats",     Description = "Lower body strength" };

    public ICommand CompleteStepsQuestCommand   { get; }
    public ICommand CompletePushupsQuestCommand { get; }
    public ICommand CompleteSitupsQuestCommand  { get; }
    public ICommand CompleteSquatsQuestCommand  { get; }
    public ICommand ResetQuestsCommand          { get; }
    public ICommand SimulateDecayCommand        { get; }
    public ICommand NavigateToWorkoutLogCommand { get; }
    public ICommand NavigateToHelpCommand       { get; }
    public ICommand TestNotificationCommand     { get; }

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
    }

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
    }

    private void NotifyQuestProperties()
    {
        OnPropertyChanged(nameof(QuestCountText));
        OnPropertyChanged(nameof(QuestProgress));
        OnPropertyChanged(nameof(QuestStatusText));
    }

    private static void HapticClick()
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
