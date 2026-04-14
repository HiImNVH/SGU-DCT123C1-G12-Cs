// AppShell.xaml.cs
using TravelGuide.Services;
using TravelGuide.Views;

namespace TravelGuide
{
    public partial class AppShell : Shell
    {
        private static LocalizationService L => LocalizationService.Instance;
        public static AppShell? Current { get; private set; }

        public AppShell()
        {
            InitializeComponent();
            Current = this;

            // Đăng ký các route điều hướng push (không phải tab)
            Routing.RegisterRoute(nameof(POIDetailPage), typeof(POIDetailPage));

            // Cập nhật tab title theo ngôn ngữ
            Dispatcher.DispatchAsync(UpdateTabTitles);
            L.PropertyChanged += (_, _) =>
                MainThread.BeginInvokeOnMainThread(UpdateTabTitles);
        }

        public void UpdateTabTitles()
        {
            foreach (var item in Items)
            {
                if (item is not TabBar tabBar) continue;
                foreach (var tab in tabBar.Items)
                {
                    if (tab.Items.FirstOrDefault() is ShellContent content)
                    {
                        tab.Title = content.Route switch
                        {
                            "home"    => "Trang chủ",
                            "scan"    => "Quét QR",
                            "map"     => "Bản đồ",
                            "profile" => "Hồ sơ",
                            _         => tab.Title
                        };
                    }
                }
            }
        }
    }
}
