using TravelGuide.Core.Enums;

namespace TravelGuide.Core.Models;

/// <summary>
/// Nguoi dung da dang ky trong he thong.
/// Pham vi: SQL Server (server) — KHONG luu tren SQLite local.
///
/// Luu y: Guest (khach chua dang nhap) KHONG co record trong DB.
/// Guest duoc xac dinh khi request khong mang JWT token.
/// </summary>
public class User
{
    /// <summary>Khoa chinh</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Ten dang nhap — unique</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Mat khau da bam bang BCrypt</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Ngon ngu ua thich, mac dinh = "vi"</summary>
    public string PreferredLanguage { get; set; } = "vi";

    /// <summary>Phan quyen: User hoac Admin</summary>
    public UserRole Role { get; set; } = UserRole.User;
}
