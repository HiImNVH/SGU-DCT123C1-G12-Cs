using Microsoft.EntityFrameworkCore;
using Serilog;
using TravelGuide.API.Configurations;
using TravelGuide.API.Data;
using TravelGuide.API.Middleware;
using TravelGuide.API.Repositories;
using TravelGuide.API.Services;

// ─────────────────────────────────────────────
// SERILOG - cau hinh logging truoc khi khoi dong
// ─────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

Log.Information("[info] - Bat dau khoi dong Travel Guide App API");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5171); // 🔥 cho phép điện thoại truy cập
    });
    // ─────────────────────────────────────────────
    // SERILOG tich hop vao ASP.NET Core
    // ─────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
    });

    // ─────────────────────────────────────────────
    // DATABASE - EF Core + SQL Server
    // ─────────────────────────────────────────────
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseSqlServer(connStr);
        Log.Information("[info] - Da cau hinh SQL Server DbContext");
    });

    // ─────────────────────────────────────────────
    // AUTHENTICATION - JWT Bearer
    // ─────────────────────────────────────────────
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddAuthorization();

    // ─────────────────────────────────────────────
    // DEPENDENCY INJECTION - Repositories & Services
    // ─────────────────────────────────────────────

    // Repositories
    builder.Services.AddScoped<IPOIRepository, POIRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();

    // Services
    builder.Services.AddScoped<IPOIService, POIService>();
    builder.Services.AddScoped<IQRService, QRService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IUserPreferenceService, UserPreferenceService>();
    builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
    builder.Services.AddScoped<IDeviceService, DeviceService>();

    // ─────────────────────────────────────────────
    // MVC + API
    // ─────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerWithJwt();

    // ─────────────────────────────────────────────
    // CORS - cho phep MAUI va Blazor goi API
    // ─────────────────────────────────────────────
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });


    // ─────────────────────────────────────────────
    // BUILD APP
    // ─────────────────────────────────────────────
    var app = builder.Build();
    // ─────────────────────────────────────────────
    // seed data
    // ─────────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var seedLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        await DatabaseSeeder.SeedAsync(db, seedLogger);
    }

    // ─────────────────────────────────────────────
    // MIDDLEWARE PIPELINE
    // ─────────────────────────────────────────────

    // Xu ly loi toan cuc - phai dung dau tien
    app.UseMiddleware<ErrorHandlingMiddleware>();

    // Log request/response
    app.UseMiddleware<RequestLoggingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Travel Guide API v1");
            options.RoutePrefix = string.Empty; // Swagger tai root URL
        });
        Log.Information("[info] - Swagger UI da bat: http://localhost:PORT/");
    }

    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("[info] - Travel Guide App API da san sang");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "[error] - API khoi dong that bai: {Message}", ex.Message);
}
finally
{
    Log.CloseAndFlush();
}