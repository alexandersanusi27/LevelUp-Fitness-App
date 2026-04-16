using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace LevelUp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public const int ScreenCaptureRequestCode = 1000;
    public static Action<int, Intent?>? ProjectionResultCallback;

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        if (requestCode == ScreenCaptureRequestCode)
        {
            ProjectionResultCallback?.Invoke((int)resultCode, data);
            ProjectionResultCallback = null;
        }
    }
}
