using MudBlazor;
using MudBlazor.Services;
using Serilog;
using TravelGuide.AdminWeb.Components;
using TravelGuide.AdminWeb.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

Log.Information("[info] - Bat dau khoi dong Travel Guide Admin Web");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ─────────────────────────────────────
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
    });

    // ── Blazor Server ────────────────────────────────
    builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        // Tang gioi han message len 10MB
        options.MaximumReceiveMessageSize = 10 * 1024 * 1024;
    });

    // ── MudBlazor ────────────────────────────────────
    builder.Services.AddMudServices(config =>
    {
        config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
        config.SnackbarConfiguration.ShowCloseIcon = true;
        config.SnackbarConfiguration.VisibleStateDuration = 3000;
    });

    // ── HttpClient goi API ───────────────────────────
    var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7208";

    // Trong Development: bo qua SSL certificate validation
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddHttpClient("TravelGuideAPI", client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30); // Tăng timeout cho QR generation
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
    }
    else
    {
        builder.Services.AddHttpClient("TravelGuideAPI", client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });
    }

    // ── DI Services ──────────────────────────────────

    // AdminSession la Singleton vi Blazor Server dung shared state per-circuit
    builder.Services.AddSingleton<TravelGuide.AdminWeb.Models.AdminSession>();

    // Services la Scoped (moi Blazor circuit 1 instance)
    builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();
    builder.Services.AddScoped<IPOIManagementService, POIManagementService>();
    builder.Services.AddScoped<IPOIContentManagementService, POIContentManagementService>();
    builder.Services.AddScoped<IQRManagementService, QRManagementService>();
    builder.Services.AddScoped<IDeviceManagementService, DeviceManagementService>();
    builder.Services.AddScoped<ITranslationService, TranslationService>();

    // ── Build ─────────────────────────────────────────
    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseStaticFiles();
    app.UseAntiforgery();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    Log.Information("[info] - Travel Guide Admin Web da san sang");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "[error] - Admin Web khoi dong that bai: {Message}", ex.Message);
}
finally
{
    Log.CloseAndFlush();
}