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

    // runs when the user taps "Add Exercise"
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

        bool isFirstExercise = exercises.Count == 0;

        HapticClick();
        exercises.Add(new ExerciseEntry { Name = name, Weight = weight, Reps = reps });
        await SoundService.PlayAsync("exercise_add.wav");

        // first exercise of the day triggers the bonus workout quest
        if (isFirstExercise && !AppState.WorkoutQuestDone)
        {
            AppState.WorkoutQuestDone = true;
            AppState.DailyQuestsCompleted = Math.Min(AppState.DailyQuestsCompleted + 1, AppState.MaxDailyQuests);
            AppState.CurrentXP += AppState.XPPerQuest;
            xpEarnedOnThisPage += AppState.XPPerQuest;
            AppState.Save();
            Vibrate(200);
            await DisplayAlertAsync("[ QUEST COMPLETE ]", $"Workout quest complete! +{AppState.XPPerQuest} XP awarded.", "OK");
        }

        SaveCurrentSession();

        ExerciseNameEntry.Text = "";
        WeightEntry.Text       = "";
        RepsEntry.Text         = "";

        RefreshExerciseList();
        UpdateStats();
    }

    private void SaveCurrentSession()
    {
        var toSave = exercises.Select(e => new SavedExercise
        {
            Name   = e.Name,
            Weight = e.Weight,
            Reps   = e.Reps,
            Done   = e.Done
        });
        WorkoutHistoryService.SaveToday(toSave);
    }

    private void RefreshExerciseList()
    {
        ExerciseListContainer.Children.Clear();
        EmptyListLabel.IsVisible = exercises.Count == 0;

        for (int i = 0; i < exercises.Count; i++)
        {
            var entry = exercises[i];

            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                ColumnSpacing   = 10,
                BackgroundColor = entry.Done ? Color.FromArgb("#0D2B0D") : Color.FromArgb("#111120"),
                Padding         = new Thickness(10, 8)
            };

            var checkbox = new CheckBox
            {
                IsChecked = entry.Done,
                Color     = Color.FromArgb("#9966FF")
            };

            checkbox.CheckedChanged += (s, ev) =>
            {
                HapticClick();
                entry.Done = ev.Value;
                SaveCurrentSession();
                RefreshExerciseList();
                UpdateStats();
            };

            var label = new Label
            {
                Text            = entry.ToString(),
                TextColor       = entry.Done ? Color.FromArgb("#448844") : Colors.White,
                FontSize        = 14,
                TextDecorations = entry.Done ? TextDecorations.Strikethrough : TextDecorations.None,
                VerticalTextAlignment = TextAlignment.Center
            };

            row.Add(checkbox, 0, 0);
            row.Add(label,    1, 0);
            ExerciseListContainer.Children.Add(row);
        }
    }

    private void UpdateStats()
    {
        int total     = exercises.Count;
        int completed = exercises.Count(ex => ex.Done);
        TotalLoggedLabel.Text    = total.ToString();
        TotalCompletedLabel.Text = completed.ToString();
        XPEarnedLabel.Text       = $"{xpEarnedOnThisPage} XP";
    }
}
