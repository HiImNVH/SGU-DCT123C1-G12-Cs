namespace TravelGuide;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
    private void OnLoginClicked(object sender, EventArgs e)
    {
        Application.Current.Windows[0].Page = new AppShell();
    }
}