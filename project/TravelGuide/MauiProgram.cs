using Microsoft.Extensions.Logging;
using TravelGuide.Services;

namespace TravelGuide
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Đăng ký Services
            builder.Services.AddSingleton<IAuthService, ApiAuthService>();
            builder.Services.AddSingleton<INarrationService, NarrationService>();
            builder.Services.AddSingleton<ILocationService, LocationService>();

            // Đăng ký Pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<PlaceListPage>();
            builder.Services.AddTransient<PlaceDetailPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<LanguageSelectionPage>();
            builder.Services.AddTransient<Views.MapPage>();
	    builder.Services.AddSingleton<IPOIService, ApiPOIService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}