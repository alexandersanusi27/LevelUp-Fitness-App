namespace LevelUp;

public partial class WorkoutLogPage : ContentPage
{
    private class ExerciseEntry
    {
        public string Name   { get; set; } = "";
        public double Weight { get; set; }
        public int    Reps   { get; set; }
        public bool   Done   { get; set; } = false;
        public override string ToString() => $"{Name} \u2014 {Weight}kg x {Reps}";
    }

    private readonly List<ExerciseEntry> exercises = new();
    private int xpEarnedOnThisPage = 0;

    private static void HapticClick()
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }
    }

    private static void Vibrate(int milliseconds)
    {
        try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(milliseconds)); } catch { }
    }

    public WorkoutLogPage()
    {
        InitializeComponent();
    }

    // validates the exercise form inputs and shows errors if anything is wrong
    private async void OnAddExerciseClicked(object? sender, EventArgs e)
    {
        string name       = ExerciseNameEntry.Text?.Trim() ?? "";
        string weightText = WeightEntry.Text?.Trim()       ?? "";
        string repsText   = RepsEntry.Text?.Trim()         ?? "";

        if (string.IsNullOrEmpty(name))
        {
            await DisplayAlertAsync("[ INPUT ERROR ]", "Please enter an exercise name.", "OK");
            return;
        }

        if (!double.TryParse(weightText, out double weight) || weight < 0)
        {
            await DisplayAlertAsync("[ INPUT ERROR ]", "Please enter a valid weight (kg).", "OK");
            return;
        }

        if (!int.TryParse(repsText, out int reps) || reps <= 0)
        {
            await DisplayAlertAsync("[ INPUT ERROR ]", "Please enter a valid rep count.", "OK");
            return;
        }
    }
}
