using TravelGuide.Services;
using TravelGuide.Views;

namespace TravelGuide;

public partial class AppShell : Shell
{
    private static LocalizationService L => LocalizationService.Instance;

    public static AppShell Current { get; private set; }

    public AppShell()
    {
        InitializeComponent();
        Current = this;

        Routing.RegisterRoute(nameof(MapPage), typeof(MapPage));
        Routing.RegisterRoute(nameof(PlaceDetailPage), typeof(PlaceDetailPage));

        // 🔥 Update title ngay khi load
        Dispatcher.DispatchAsync(UpdateTabTitles);

        // 🔥 Khi đổi ngôn ngữ
        L.PropertyChanged += (_, _) =>
            MainThread.BeginInvokeOnMainThread(UpdateTabTitles);

        // 🔥 Khi navigate
        Navigated += (_, _) =>
            MainThread.BeginInvokeOnMainThread(UpdateTabTitles);
    }

    public void UpdateTabTitles()
    {
        foreach (var item in Items)
        {
            if (item is not TabBar tabBar) continue;

            foreach (var tab in tabBar.Items)
            {
                // 🔥 LẤY ĐÚNG ROUTE TỪ ShellContent
                if (tab.Items.FirstOrDefault() is ShellContent content)
                {
                    tab.Title = content.Route switch
                    {
                        "home" => L["Tab_Home"],
                        "places" => L["Tab_Places"],
                        "profile" => L["Tab_Profile"],
                        _ => tab.Title
                    };
                }
            }
        }
    }
}