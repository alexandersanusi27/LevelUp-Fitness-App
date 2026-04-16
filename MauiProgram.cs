using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.Maui.Audio;

namespace LevelUp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.AddAudio()            // registers Plugin.Maui.Audio so we can play sound effects
			.UseLocalNotification() // registers Plugin.LocalNotification for daily reminders
			.ConfigureFonts(fonts =>
			{
				// just the default MAUI fonts, keeping these in
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		// only logs in debug mode so it doesnt spam in release builds
		builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton<MainPage>();

		var app = builder.Build();

		// pass the audio manager into SoundService so it can play sounds anywhere
		SoundService.Init(app.Services.GetRequiredService<IAudioManager>());
		return app;
	}
}
