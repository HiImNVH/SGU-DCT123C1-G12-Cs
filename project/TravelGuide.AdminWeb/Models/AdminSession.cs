namespace TravelGuide.AdminWeb.Models;

/// <summary>
/// Luu thong tin session admin sau khi dang nhap.
/// Khong persist - mat khi refresh page (dung cho POC).
/// </summary>
public class AdminSession
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string PreferredLanguage { get; set; } = "vi";
    public DateTime ExpiresAt { get; set; }

    public bool IsLoggedIn => !string.IsNullOrEmpty(Token) && ExpiresAt > DateTime.UtcNow;
}
