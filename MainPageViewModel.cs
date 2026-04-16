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

    private int    _rankIndex = 0;
    private string _rank      = "E";

    public QuestViewModel StepsQuest   { get; } = new() { Name = "Walk 10,000 Steps", Description = "Daily step goal" };
    public QuestViewModel PushupsQuest { get; } = new() { Name = "Do 50 Push-ups",    Description = "Upper body strength" };
    public QuestViewModel SitupsQuest  { get; } = new() { Name = "Do 50 Sit-ups",     Description = "Core strength" };
    public QuestViewModel SquatsQuest  { get; } = new() { Name = "Do 100 Squats",     Description = "Lower body strength" };

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
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
