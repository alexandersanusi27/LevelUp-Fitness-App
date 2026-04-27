namespace LevelUp;

public partial class WorkoutLogPage : ContentPage
{
    // simple model to hold one exercise entry for the current session
    private class ExerciseEntry
    {
        public string Name   { get; set; } = "";
        public double Weight { get; set; }
        public int    Reps   { get; set; }
        public bool   Done   { get; set; } = false;

        public override string ToString() => $"{Name} \u2014 {Weight}kg x {Reps}";
    }

    // exercises logged in the current session
    private readonly List<ExerciseEntry> exercises = new();

    // tracks XP earned just on this page visit, shown in the stats bar
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

    // runs every time the page is navigated to
    // loads history from disk and restores any exercises already logged today
    protected override void OnAppearing()
    {
        base.OnAppearing();

        WorkoutHistoryService.Load();

        // restore todays exercises if the user left and came back (or restarted the app)
        if (exercises.Count == 0)
        {
            foreach (var saved in WorkoutHistoryService.GetToday())
            {
                exercises.Add(new ExerciseEntry
                {
                    Name   = saved.Name,
                    Weight = saved.Weight,
                    Reps   = saved.Reps,
                    Done   = saved.Done
                });
            }

            // if we restored exercises, the workout quest was already counted before
            // so mark it done to prevent awarding XP again
            if (exercises.Count > 0)
                AppState.WorkoutQuestDone = true;
        }

        RefreshExerciseList();
        RefreshHistory();
        UpdateStats();
        _ = FetchLocationAsync();
    }

    // fetches the device's current GPS coordinates and displays them
    // uses GetLastKnownLocationAsync first (fast, cached) then falls back to a live fix
    private async Task FetchLocationAsync()
    {
        try
        {
            var location = await Geolocation.Default.GetLastKnownLocationAsync();
            if (location == null)
                location = await Geolocation.Default.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.Low, TimeSpan.FromSeconds(5)));

            if (location != null)
            {
                // format as cardinal coordinates, e.g. 53.4808°N, 2.2426°W
                string lat = $"{Math.Abs(location.Latitude):F4}°{(location.Latitude >= 0 ? "N" : "S")}";
                string lon = $"{Math.Abs(location.Longitude):F4}°{(location.Longitude >= 0 ? "E" : "W")}";
                LocationLabel.Text = $"{lat}, {lon}";
                LocationPanel.IsVisible = true;
            }
        }
        catch { /* location unavailable or permission denied - panel stays hidden */ }
    }

    // runs when the user taps "Add Exercise"
    private async void OnAddExerciseClicked(object? sender, EventArgs e)
    {
        string name       = ExerciseNameEntry.Text?.Trim() ?? "";
        string weightText = WeightEntry.Text?.Trim()       ?? "";
        string repsText   = RepsEntry.Text?.Trim()         ?? "";

        if (string.IsNullOrEmpty(name))
        {
            await DisplayAlert("[ INPUT ERROR ]", "Please enter an exercise name.", "OK");
            return;
        }

        if (!double.TryParse(weightText, out double weight) || weight < 0)
        {
            await DisplayAlert("[ INPUT ERROR ]", "Please enter a valid weight (kg).", "OK");
            return;
        }

        if (!int.TryParse(repsText, out int reps) || reps <= 0)
        {
            await DisplayAlert("[ INPUT ERROR ]", "Please enter a valid rep count.", "OK");
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
            await DisplayAlert("[ QUEST COMPLETE ]", $"Workout quest complete! +{AppState.XPPerQuest} XP awarded.", "OK");
        }

        // save the updated session to disk
        SaveCurrentSession();

        ExerciseNameEntry.Text = "";
        WeightEntry.Text       = "";
        RepsEntry.Text         = "";

        RefreshExerciseList();
        UpdateStats();
    }

    // converts the current exercise list to SavedExercise objects and hands them to the service
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

    // rebuilds the exercise list UI from scratch
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

    // rebuilds the past sessions panel from the history service
    private void RefreshHistory()
    {
        var history = WorkoutHistoryService.GetHistory();

        HistoryPanel.IsVisible = history.Count > 0;
        HistoryContainer.Children.Clear();

        foreach (var session in history)
        {
            string displayDate = DateTime.TryParse(session.Date, out var dt)
                ? dt.ToString("dd MMM yyyy")
                : session.Date;

            HistoryContainer.Children.Add(new Label
            {
                Text           = displayDate,
                TextColor      = Color.FromArgb("#9966FF"),
                FontSize       = 13,
                FontAttributes = FontAttributes.Bold
            });

            foreach (var ex in session.Exercises)
            {
                HistoryContainer.Children.Add(new Label
                {
                    Text      = "  " + ex.ToString() + (ex.Done ? "  \u2713" : ""),
                    TextColor = ex.Done ? Color.FromArgb("#448844") : Color.FromArgb("#AAAAAA"),
                    FontSize  = 12
                });
            }

            HistoryContainer.Children.Add(new BoxView
            {
                HeightRequest   = 1,
                BackgroundColor = Color.FromArgb("#1A1A2E"),
                Margin          = new Thickness(0, 4)
            });
        }
    }

    // updates the stats bar at the top
    private void UpdateStats()
    {
        int total     = exercises.Count;
        int completed = exercises.Count(ex => ex.Done);
        TotalLoggedLabel.Text    = total.ToString();
        TotalCompletedLabel.Text = completed.ToString();
        XPEarnedLabel.Text       = $"{xpEarnedOnThisPage} XP";
    }
}
