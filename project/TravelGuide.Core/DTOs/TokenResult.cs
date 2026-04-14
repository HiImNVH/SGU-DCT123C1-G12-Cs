namespace TravelGuide.Core.DTOs;

/// <summary>
/// Ket qua tra ve sau khi dang nhap thanh cong.
/// Dung cho: POST /api/auth/login (response)
/// </summary>
public class TokenResult
{
    /// <summary>JWT token string</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Thoi diem JWT het han</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Ngon ngu ua thich hien tai cua user</summary>
    public string PreferredLanguage { get; set; } = "vi";

    /// <summary>Role cua user: "User" hoac "Admin"</summary>
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// Request body cho dang nhap.
/// Dung cho: POST /api/auth/login (request)
/// </summary>
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Request body cap nhat ngon ngu ua thich.
/// Dung cho: PUT /api/user/preference/language (request)
/// </summary>
public class UpdateLanguageRequest
{
    /// <summary>Ma ngon ngu theo BCP-47: vi, en, zh, ja, ko, fr</summary>
    public string LanguageCode { get; set; } = string.Empty;
}

/// <summary>
/// POST /api/auth/register - request body
/// </summary>
public class RegisterRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string? PreferredLanguage { get; set; } = "vi";
}
