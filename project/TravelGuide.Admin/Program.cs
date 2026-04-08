using TravelGuide.Admin.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// HttpClient cho Admin API
builder.Services.AddHttpClient("AdminApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5042");
});

// HttpClient cho Translation API (MyMemory - miễn phí, không cần API key)
builder.Services.AddHttpClient("MyMemory", client =>
{
    client.BaseAddress = new Uri("https://api.mymemory.translated.net/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Đăng ký services
builder.Services.AddScoped<IAdminApiService, AdminApiService>();
builder.Services.AddScoped<ITranslationService, TranslationService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<TravelGuide.Admin.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();