namespace TravelGuide.Core.Enums;

/// <summary>
/// Phan quyen nguoi dung trong he thong.
/// Guest: khong co record trong DB, xac dinh khi khong co JWT token.
/// </summary>
public enum UserRole
{
    User,
    Admin
}
