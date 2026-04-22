// AppShell.xaml.cs
using TravelGuide.Services;
using TravelGuide.Views;

namespace TravelGuide
{
    public partial class AppShell : Shell
    {
        private static LocalizationService L => LocalizationService.Instance;

        // Dùng `new` để tránh warning CS0108 vì Shell.Current đã tồn tại
        public new static AppShell? Current { get; private set; }

        public AppShell()
        {
            InitializeComponent();
            Current = this;

            Routing.RegisterRoute(nameof(POIDetailPage), typeof(POIDetailPage));

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

        /// <summary>
        /// Được gọi từ MainActivity.HandleIntent() khi nhận deep-link
        /// travelguide://poi/{poiId} — dù app đang chạy hay vừa khởi động.
        /// </summary>
        public static async Task HandleDeepLinkAsync(string poiId)
        {
            if (string.IsNullOrWhiteSpace(poiId)) return;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                // Đợi Shell hoàn toàn sẵn sàng (quan trọng khi app cold-start từ QR)
                var timeout = 0;
                while (Shell.Current == null && timeout < 20)
                {
                    await Task.Delay(100);
                    timeout++;
                }

                if (Shell.Current == null) return;

                // Nếu đang ở login/language page thì vào main trước
                var currentRoute = Shell.Current.CurrentState?.Location?.ToString() ?? "";
                if (!currentRoute.Contains("main"))
                {
                    await Shell.Current.GoToAsync("//main/home");
                    await Task.Delay(200);
                }

                await Shell.Current.GoToAsync(
                    $"{nameof(POIDetailPage)}?PoiId={poiId}");
            });
        }
    }
}