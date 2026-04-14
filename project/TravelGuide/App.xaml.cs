// App.xaml.cs
using TravelGuide.Services;

namespace TravelGuide
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            // Load ngôn ngữ đã lưu trước khi render UI
            LocalizationService.Instance.SetLanguage(
                Preferences.Get("preferred_language", "vi"));
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
