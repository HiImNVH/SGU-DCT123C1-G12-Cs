// MauiProgram.cs
using CommunityToolkit.Maui;
using ZXing.Net.Maui.Controls;
using TravelGuide.Repositories;
using TravelGuide.Services;
using TravelGuide.ViewModels;
using TravelGuide.Views;

namespace TravelGuide
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                // ZXing QR Scanner
                .UseBarcodeReader()
                // CommunityToolkit (UI helpers + InvertedBoolConverter)
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            var s = builder.Services;

            // ── Repositories ──────────────────────────────
            s.AddSingleton<LocalCacheRepository>();

            // ── Services ──────────────────────────────────
            s.AddSingleton<AuthService>();
            s.AddSingleton<CacheService>();
            s.AddSingleton<POIDataService>();
            s.AddSingleton<QRScannerService>();
            s.AddSingleton<TTSPlayerService>();
            s.AddSingleton<ProximityService>();
            // ── ViewModels ────────────────────────────────
            /*
             * FIX #2a: ScanViewModel đổi sang Singleton
             * Shell tự cache ScanPage instance (do ContentTemplate DataTemplate),
             * nên ViewModel cũng phải là Singleton để tránh tạo instance mới
             * mỗi lần DI resolve trong khi page vẫn giữ instance cũ.
             */
            s.AddSingleton<ScanViewModel>();

            s.AddSingleton<DeviceTrackingService>();

            // ── Views ─────────────────────────────────────
            // Auth (Transient OK vì không phải tab Shell)
            s.AddTransient<LoginPage>();
            s.AddTransient<LanguageSelectionPage>();
            s.AddTransient<RegisterPage>();
            /*
             * FIX #2b: ScanPage đổi sang Singleton
             * Shell dùng ContentTemplate="{DataTemplate}" → Shell tự cache page,
             * KHÔNG tạo instance mới khi chuyển tab.
             * AddTransient gây xung đột: DI tạo instance mới nhưng Shell dùng cái cũ
             * → camera state bị dirty, OnAppearing không chạy đúng lifecycle.
             */
            s.AddSingleton<ScanPage>();

            // Các tab khác giữ Transient vì không có camera state
            s.AddTransient<HomePage>();
            s.AddTransient<MapPage>();
            s.AddTransient<ProfilePage>();

            // Push pages (Transient OK vì navigate bình thường)
            s.AddTransient<POIDetailPage>();

            return builder.Build();
        }
    }
}
