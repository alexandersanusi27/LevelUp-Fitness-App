using Microsoft.Extensions.DependencyInjection;

namespace LevelUp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		AppState.Load(); // restore XP and quest state; auto-resets quests if it's a new day

		// schedule the 8pm daily reminder - fire and forget, no need to await here
		_ = NotificationService.ScheduleDailyReminderAsync();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}