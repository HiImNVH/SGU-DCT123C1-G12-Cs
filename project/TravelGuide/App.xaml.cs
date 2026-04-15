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

        public App(DeviceTrackingService tracking)
{
    InitializeComponent();
    LocalizationService.Instance.SetLanguage(Preferences.Get("preferred_language", "vi"));
 
    // Gửi ping mỗi khi app mở (fire-and-forget)
    tracking.PingAsync();
}

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
