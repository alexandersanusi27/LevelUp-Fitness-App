namespace LevelUp;

// the help / hunters guide page - just static content explaining the rank system and quests
public partial class HelpPage : ContentPage
{
    public HelpPage()
    {
        InitializeComponent();
    }

    // goes back to the previous page (the dashboard) when the back button is tapped
    private async void OnBackClicked(object? sender, EventArgs e)
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }
        await Shell.Current.GoToAsync("..");
    }
}
