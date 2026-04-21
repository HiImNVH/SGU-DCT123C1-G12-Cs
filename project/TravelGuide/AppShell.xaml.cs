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

            Routing.RegisterRoute(nameof(POIDetailPage), typeof(POIDetailPage));

            // Cập nhật tab titles ngay khi khởi động
            Dispatcher.DispatchAsync(UpdateTabTitles);

            // Lắng nghe đổi ngôn ngữ → cập nhật tab titles ngay lập tức
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
                            "home" => L["Tab_Home"],
                            "scan" => L["Tab_Scan"],
                            "map" => L["Tab_Map"],
                            "profile" => L["Tab_Profile"],
                            _ => tab.Title
                        };
                    }
                }
            }
        }
    }
}
