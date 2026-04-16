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
