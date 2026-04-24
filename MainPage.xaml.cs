namespace LevelUp;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _vm;

    // shake detection - stays in code-behind because its hardware/sensor stuff
    private const double ShakeThreshold = 2.5;
    private DateTime _lastShakeTime = DateTime.MinValue;
    private bool _shakeAlertOpen = false;

    public MainPage()
    {
        InitializeComponent();

        // create the viewmodel and set it as the binding context for the XAML
        _vm = new MainPageViewModel();
        BindingContext = _vm;

        // subscribe to viewmodel events so we can handle dialogs, sounds, and animations here
        _vm.RankUpOccurred      += OnRankUpAsync;
        _vm.QuestCompleted      += OnQuestCompletedAsync;
        _vm.DecayOccurred       += OnDecayOccurredAsync;
        _vm.RankMaintained      += OnRankMaintainedAsync;
        _vm.ShakeQuoteRequested += OnShakeQuoteAsync;
    }

    // sync the UI every time we come back to this page (e.g. after logging a workout)
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.SyncFromAppState();

        // start shake detection only if not already running
        if (Accelerometer.Default.IsSupported && !Accelerometer.Default.IsMonitoring)
        {
            Accelerometer.Default.ReadingChanged += OnAccelerometerReadingChanged;
            Accelerometer.Default.Start(SensorSpeed.Game);
        }
    }

    // stop the accelerometer when leaving to save battery
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (Accelerometer.Default.IsMonitoring)
        {
            Accelerometer.Default.Stop();
            Accelerometer.Default.ReadingChanged -= OnAccelerometerReadingChanged;
        }
    }

    // ── ViewModel Event Handlers ─────────────────────────────────────────────

    private async Task OnRankUpAsync(string rank, string title, string message)
    {
        Vibrate(600);
        await SoundService.PlayAsync("rank_up.wav");
        await FlashRankUpEffect();
        if (AppState.TTSEnabled) _ = TextToSpeech.Default.SpeakAsync($"Rank up! You have advanced to Rank {rank}: {title}. {message}");
        await DisplayAlert("[ RANK UP ]", $"You have advanced to Rank {rank}: {title}\n\n{message}", "ARISE");
    }

    private async Task OnQuestCompletedAsync(string questName, int xp, bool allDone)
    {
        await SoundService.PlayAsync("quest_complete.wav");
        if (allDone)
        {
            HapticLong();
            if (AppState.TTSEnabled) _ = TextToSpeech.Default.SpeakAsync("All daily quests complete! Outstanding work, Hunter.");
            await DisplayAlert("[ QUEST COMPLETE ]", "All 4 daily quests finished! The system is pleased.", "OK");
        }
        else
        {
            Vibrate(200);
            if (AppState.TTSEnabled) _ = TextToSpeech.Default.SpeakAsync($"Quest complete! {questName}. {xp} XP awarded.");
            await DisplayAlert("[ QUEST COMPLETE ]", $"{questName} complete! +{xp} XP awarded.", "OK");
        }
    }

    private async Task OnDecayOccurredAsync()
    {
        Vibrate(800);
        await SoundService.PlayAsync("rank_decay.wav");
        await DisplayAlert("[ RANK DECAY ]", "Maintenance failed. The system has revoked your title.\nYou have fallen to Rank A: Platinum Hunter.", "Understood");
    }

    private async Task OnRankMaintainedAsync()
    {
        HapticLong();
        await DisplayAlert("[ RANK MAINTAINED ]", "Maintenance confirmed. Your status as Shadow Monarch endures.", "Arise");
    }

    // ── Shake Detection ──────────────────────────────────────────────────────

    private void OnAccelerometerReadingChanged(object? sender, AccelerometerChangedEventArgs e)
    {
        var a = e.Reading.Acceleration;
        double magnitude = Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);

        if (magnitude < ShakeThreshold) return;
        if (_shakeAlertOpen) return;
        if ((DateTime.UtcNow - _lastShakeTime).TotalSeconds < 2) return;

        _lastShakeTime = DateTime.UtcNow;
        MainThread.BeginInvokeOnMainThread(ShowRandomQuote);
    }

    private async void ShowRandomQuote()
    {
        if (_shakeAlertOpen) return;
        _shakeAlertOpen = true;
        try
        {
            Vibrate(150);
            string quote = MainPageViewModel.GetRandomQuote();
            if (AppState.TTSEnabled) _ = TextToSpeech.Default.SpeakAsync(quote);
            await DisplayAlert("[ SYSTEM MESSAGE ]", $"\"{quote}\"", "Understood");
        }
        finally { _shakeAlertOpen = false; }
    }

    // handles the demo button quote request from the viewmodel
    private async Task OnShakeQuoteAsync(string quote)
    {
        if (_shakeAlertOpen) return;
        _shakeAlertOpen = true;
        try
        {
            if (AppState.TTSEnabled) _ = TextToSpeech.Default.SpeakAsync(quote);
            await DisplayAlert("[ SYSTEM MESSAGE ]", $"\"{quote}\"", "Understood");
        }
        finally { _shakeAlertOpen = false; }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    // badge flashes 3 times on rank up
    private async Task FlashRankUpEffect()
    {
        for (int i = 0; i < 3; i++)
        {
            await RankBadgeOuter.FadeTo(0.2, 200);
            await RankBadgeOuter.FadeTo(1.0, 200);
        }
    }

    private static void HapticClick()
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }
    }

    private static void HapticLong()
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
    }

    private static void Vibrate(int milliseconds)
    {
        try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(milliseconds)); } catch { }
    }
}
