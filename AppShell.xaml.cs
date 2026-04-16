namespace LevelUp;

// the shell is MAUIs navigation container - think of it like a nav stack manager
// pages navigate by name using Shell.Current.GoToAsync(nameof(PageName))
public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        // register the pages that can be navigated to by name
        // without these lines GoToAsync would throw an exception
        Routing.RegisterRoute(nameof(WorkoutLogPage), typeof(WorkoutLogPage));
        Routing.RegisterRoute(nameof(HelpPage), typeof(HelpPage));
    }
}
