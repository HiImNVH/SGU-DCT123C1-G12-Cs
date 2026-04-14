namespace TravelGuide.Models.DTOs
{
    // ── POI ────────────────────────────────────────────────────────

    /// <summary>GET /api/poi/{id}?lang=vi</summary>
    public class POIDetailDto
    {
        public Guid    Id        { get; set; }
        public string  Name      { get; set; } = "";
        public string  Category  { get; set; } = "";
        public string? ImageUrl  { get; set; }
        public double  Latitude  { get; set; }
        public double  Longitude { get; set; }
        public POIContentDto? Content { get; set; }
    }

    /// <summary>GET /api/poi - danh sách</summary>
    public class POISummaryDto
    {
        public Guid    Id        { get; set; }
        public string  Name      { get; set; } = "";
        public string  Category  { get; set; } = "";
        public string? ImageUrl  { get; set; }
        public double  Latitude  { get; set; }
        public double  Longitude { get; set; }
        public bool    IsActive  { get; set; }
    }

    /// <summary>Nội dung đa ngôn ngữ trong POIDetailDto</summary>
    public class POIContentDto
    {
        public string  LanguageCode   { get; set; } = "vi";
        public string  NarrationText  { get; set; } = "";
        public string? AudioUrl       { get; set; }
    }

    // ── Auth ───────────────────────────────────────────────────────

    /// <summary>POST /api/auth/login - request body</summary>
    public class LoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
    /// <summary>POST /api/auth/register - request body</summary>

    public class RegisterRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string? PreferredLanguage { get; set; } = "vi";
    }
    /// <summary>POST /api/auth/login - response từ API (TokenResult)</summary>
    public class TokenResult
    {
        public string   Token             { get; set; } = "";
        public DateTime ExpiresAt         { get; set; }
        public string   PreferredLanguage { get; set; } = "vi";
        public string   Role              { get; set; } = "User";
    }

    /// <summary>Internal response dùng trong app</summary>
    public class AuthResponse
    {
        public bool    Success      { get; set; }
        public string? Token        { get; set; }
        public string? ErrorMessage { get; set; }
        public User?   User         { get; set; }
    }

    // ── Language ───────────────────────────────────────────────────

    /// <summary>PUT /api/user/preference/language - request body</summary>
    public class UpdateLanguageRequest
    {
        public string LanguageCode { get; set; } = "vi";
    }
}
