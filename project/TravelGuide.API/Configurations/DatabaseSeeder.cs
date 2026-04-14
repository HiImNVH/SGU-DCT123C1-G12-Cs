using TravelGuide.API.Data;
using TravelGuide.Core.Models;
using TravelGuide.Core.Enums;

namespace TravelGuide.API.Configurations;

/// <summary>
/// Seed du lieu mac dinh: tao admin account neu chua co
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger)
    {
        logger.LogInformation("[info] - Bat dau seed du lieu mac dinh");

        // Tao admin mac dinh neu chua co
        if (!db.Users.Any(u => u.Username == "admin"))
        {
            var admin = new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                PreferredLanguage = "vi",
                Role = UserRole.Admin
            };
            db.Users.Add(admin);
            logger.LogInformation("[info] - Da tao admin mac dinh (username=admin, password=Admin@123)");
        }

        // Tao du lieu POI mau de test
        if (!db.POIs.Any())
        {
            var samplePoi = new POI
            {
                Name = "Gian hang gom su Bat Trang",
                Category = "Thu cong my nghe",
                ImageUrl = null,
                Latitude = 20.9747,
                Longitude = 105.9231,
                IsActive = true
            };
            db.POIs.Add(samplePoi);
            await db.SaveChangesAsync();

            db.POIContents.Add(new POIContent
            {
                POIId = samplePoi.Id,
                LanguageCode = "vi",
                NarrationText = "Chao mung ban den voi gian hang gom su Bat Trang. Day la mot trong nhung lang nghe gom su noi tieng nhat Viet Nam, voi lich su hon 700 nam.",
                AudioUrl = null
            });

            db.POIContents.Add(new POIContent
            {
                POIId = samplePoi.Id,
                LanguageCode = "en",
                NarrationText = "Welcome to the Bat Trang ceramic shop. This is one of Vietnam's most famous pottery villages, with over 700 years of history.",
                AudioUrl = null
            });

            logger.LogInformation("[info] - Da tao POI mau id={Id}", samplePoi.Id);
        }

        await db.SaveChangesAsync();
        logger.LogInformation("[info] - Seed du lieu hoan thanh");
    }
}