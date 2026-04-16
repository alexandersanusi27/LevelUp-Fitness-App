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
        _vm = new MainPageViewModel();
        BindingContext = _vm;
    }

    // sync the UI every time we come back to this page
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

    // fires constantly from the sensor - calculates movement magnitude
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
            await DisplayAlertAsync("[ SYSTEM MESSAGE ]", $"\"{quote}\"", "Understood");
        }
        finally { _shakeAlertOpen = false; }
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
