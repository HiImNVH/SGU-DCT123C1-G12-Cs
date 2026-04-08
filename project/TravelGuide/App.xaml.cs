namespace TravelGuide;

using TravelGuide.Services;

public partial class App : Application
{
    private static AppShell _shell;

    public App()
    {
        InitializeComponent();
        // 🔥 Load ngôn ngữ trước
      /*  LocalizationService.Instance.SetLanguage(
            Preferences.Get("preferred_language", "vi")
        );*/
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        _shell ??= new AppShell();
        return new Window(_shell);
    }
}