namespace TravelGuide.Admin.Models
{
    public class POIDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int Radius { get; set; } = 50;
        public string ImageUrl { get; set; }
        public string Category { get; set; }
        public bool IsActive { get; set; }
        public List<POIContentDto> Contents { get; set; } = new();
        public double AverageRating { get; set; } = 0;
        public int TotalRatings { get; set; } = 0;
    }

    public class POIContentDto
    {
        public Guid Id { get; set; }
        public string LanguageCode { get; set; }
        public string Title { get; set; }
        public string NarrationText { get; set; }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty; // ✅ Thêm
        public string Role { get; set; } = string.Empty;
        public string PreferredLanguage { get; set; } = "vi";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class CreatePOIRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int Radius { get; set; } = 50;
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        
        // ⚠️ Quan trọng: Phải dùng POIContentDto, không phải CreatePOIContentRequest
        public List<POIContentDto> Contents { get; set; } = new();
    }
}