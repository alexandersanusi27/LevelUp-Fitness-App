using Plugin.LocalNotification;

namespace LevelUp;

// handles scheduling the daily quest reminder notification
// uses Plugin.LocalNotification which works on Android and iOS
public static class NotificationService
{
    // fixed ID for the daily reminder - using a constant means the old notification
    // gets replaced each time instead of stacking up duplicates
    private const int DailyReminderId = 1001;

    // schedules a notification every day at 8pm to remind the user to do their quests
    // safe to call every time the app starts - it just updates the existing schedule
    public static async Task ScheduleDailyReminderAsync()
    {
        // if 8pm today has already passed, schedule for 8pm tomorrow instead
        DateTime notifyTime = DateTime.Today.AddHours(20);
        if (DateTime.Now >= notifyTime)
            notifyTime = notifyTime.AddDays(1);

        var notification = new NotificationRequest
        {
            NotificationId = DailyReminderId,
            Title          = "[ SYSTEM NOTIFICATION ]",
            Description    = "Your daily quests await, Hunter. Complete them before midnight.",
            BadgeNumber    = 1,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = notifyTime,
                RepeatType = NotificationRepeat.Daily // fires again every 24 hours after that
            }
        };

        await LocalNotificationCenter.Current.Show(notification);
    }

    // call this when the user finishes all 4 quests for the day
    // cancels the reminder so they dont get nagged after already finishing
    public static void CancelDailyReminder()
    {
        LocalNotificationCenter.Current.Cancel(DailyReminderId);
    }

    // fires a one-off notification in 5 seconds - only used for demo purposes
    // gives you enough time to press the home button and show it arriving
    public static async Task SendTestNotificationAsync()
    {
        var notification = new NotificationRequest
        {
            NotificationId = 9999,
            Title          = "[ SYSTEM NOTIFICATION ]",
            Description    = "Your daily quests await, Hunter. Complete them before midnight.",
            BadgeNumber    = 1,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = DateTime.Now.AddSeconds(5),
                RepeatType = NotificationRepeat.No // one-off, doesnt repeat
            }
        };

        await LocalNotificationCenter.Current.Show(notification);
    }
}
