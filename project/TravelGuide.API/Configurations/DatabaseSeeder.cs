using TravelGuide.API.Data;
using TravelGuide.Core.Models;
using TravelGuide.Core.Enums;

namespace TravelGuide.API.Configurations;

/// <summary>
/// Seed du lieu mac dinh: tao admin account neu chua co
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db, IConfiguration configuration, ILogger logger)
    {
        logger.LogInformation("[info] - Bat dau seed du lieu mac dinh");

        // Chi tao admin khi duoc bat ro rang. Thong tin dang nhap phai den tu
        // User Secrets hoac bien moi truong, khong duoc commit vao source code.
        if (configuration.GetValue<bool>("SeedAdmin:Enabled"))
        {
            var username = configuration["SeedAdmin:Username"];
            var password = configuration["SeedAdmin:Password"];

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "SeedAdmin da bat nhung thieu SeedAdmin:Username hoac SeedAdmin:Password.");
            }

            if (!db.Users.Any(u => u.Username == username))
            {
                var admin = new User
                {
                    Username = username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    PreferredLanguage = "vi",
                    Role = UserRole.Admin
                };
                db.Users.Add(admin);
                logger.LogInformation("[info] - Da tao tai khoan admin demo username={Username}", username);
            }
        }

        // Tao 5 POI mau tai TP.HCM khi database chua co du lieu.
        // Anh duoc phat hanh tu Wikimedia Commons; xem docs/IMAGE_CREDITS.md.
        if (!db.POIs.Any())
        {
            var samplePois = new[]
            {
                new
                {
                    Name = "Dinh Độc Lập",
                    Category = "Di tích lịch sử",
                    ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Independence_Palace.jpg?width=960",
                    Latitude = 10.777571,
                    Longitude = 106.695837,
                    Vi = "Dinh Độc Lập là một công trình lịch sử tiêu biểu tại trung tâm Thành phố Hồ Chí Minh. Công trình gắn liền với nhiều dấu mốc quan trọng của lịch sử Việt Nam hiện đại.",
                    En = "Independence Palace is a major historical landmark in central Ho Chi Minh City. The building is associated with important milestones in modern Vietnamese history."
                },
                new
                {
                    Name = "Chợ Bến Thành",
                    Category = "Chợ truyền thống",
                    ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Ben_Thanh_Market.jpg?width=960",
                    Latitude = 10.772075,
                    Longitude = 106.698278,
                    Vi = "Chợ Bến Thành là một trong những biểu tượng quen thuộc của Thành phố Hồ Chí Minh. Nơi đây tập trung nhiều mặt hàng, món ăn và sản phẩm thủ công đặc trưng của thành phố.",
                    En = "Ben Thanh Market is one of Ho Chi Minh City's best-known landmarks. It offers local food, handicrafts and a wide range of traditional market goods."
                },
                new
                {
                    Name = "Bưu điện Trung tâm Sài Gòn",
                    Category = "Kiến trúc",
                    ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Clock_and_exterior_of_Saigon_Central_Post_Office.JPG?width=960",
                    Latitude = 10.779928,
                    Longitude = 106.699893,
                    Vi = "Bưu điện Trung tâm Sài Gòn nổi bật với mặt tiền màu vàng, đồng hồ lớn và không gian kiến trúc cổ điển. Công trình nằm cạnh Nhà thờ Đức Bà ở khu vực trung tâm Quận 1.",
                    En = "Saigon Central Post Office is known for its yellow facade, large clock and classical interior. It stands beside Notre-Dame Cathedral in District 1."
                },
                new
                {
                    Name = "Nhà thờ Đức Bà Sài Gòn",
                    Category = "Kiến trúc tôn giáo",
                    ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/20190923_Notre-Dame_Cathedral_Basilica_of_Saigon-5.jpg?width=960",
                    Latitude = 10.779785,
                    Longitude = 106.699018,
                    Vi = "Nhà thờ Đức Bà Sài Gòn là công trình kiến trúc tôn giáo nổi bật tại trung tâm Quận 1. Hai tháp chuông và mặt ngoài bằng gạch đỏ tạo nên hình ảnh đặc trưng của công trình.",
                    En = "Notre-Dame Cathedral Basilica of Saigon is a prominent religious landmark in District 1. Its twin bell towers and red-brick exterior form its distinctive appearance."
                },
                new
                {
                    Name = "Landmark 81",
                    Category = "Kiến trúc hiện đại",
                    ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Landmark_81.png?width=960",
                    Latitude = 10.794917,
                    Longitude = 106.721833,
                    Vi = "Landmark 81 là tòa nhà cao tầng nổi bật bên sông Sài Gòn, thuộc khu đô thị Vinhomes Central Park. Công trình là một điểm nhấn của đường chân trời Thành phố Hồ Chí Minh.",
                    En = "Landmark 81 is a prominent skyscraper by the Saigon River in Vinhomes Central Park. It is a defining feature of Ho Chi Minh City's modern skyline."
                }
            };

            foreach (var sample in samplePois)
            {
                var poi = new POI
                {
                    Name = sample.Name,
                    Category = sample.Category,
                    ImageUrl = sample.ImageUrl,
                    Latitude = sample.Latitude,
                    Longitude = sample.Longitude,
                    IsActive = true
                };

                db.POIs.Add(poi);
                db.POIContents.AddRange(
                    new POIContent
                    {
                        POIId = poi.Id,
                        LanguageCode = "vi",
                        NarrationText = sample.Vi,
                        AudioUrl = null
                    },
                    new POIContent
                    {
                        POIId = poi.Id,
                        LanguageCode = "en",
                        NarrationText = sample.En,
                        AudioUrl = null
                    });

                logger.LogInformation("[info] - Da tao POI mau {Name}, id={Id}", poi.Name, poi.Id);
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("[info] - Seed du lieu hoan thanh");
    }
}
