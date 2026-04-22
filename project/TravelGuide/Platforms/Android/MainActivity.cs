using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace TravelGuide
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges =
            ConfigChanges.ScreenSize | ConfigChanges.Orientation |
            ConfigChanges.UiMode | ConfigChanges.ScreenLayout |
            ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]

    // ── Deep-link: travelguide://poi/{poiId} ────────────────────────
    // Khi người dùng scan QR (hoặc nhấn link), Android kiểm tra IntentFilter này.
    // Nếu app đã cài → mở thẳng MainActivity và gọi HandleIntent().
    // Nếu chưa cài  → Android không match, browser/camera app fallback về URL HTTP
    //                 (tức là trang qr-redirect.html trên GitHub Pages).
    [IntentFilter(
        new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "travelguide",
        DataHost = "poi")]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Xử lý intent khi app khởi động từ QR
            HandleIntent(Intent);
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            // Xử lý intent khi app đang chạy và nhận deep-link mới
            HandleIntent(intent);
        }

        private static void HandleIntent(Intent? intent)
        {
            if (intent?.Action != Intent.ActionView) return;

            var uri = intent.Data;
            if (uri?.Scheme != "travelguide" || uri.Host != "poi") return;

            var poiId = uri.LastPathSegment;
            if (string.IsNullOrWhiteSpace(poiId)) return;

            // Delegate sang AppShell — xử lý navigation sau khi Shell sẵn sàng
            _ = AppShell.HandleDeepLinkAsync(poiId);
        }
    }
}