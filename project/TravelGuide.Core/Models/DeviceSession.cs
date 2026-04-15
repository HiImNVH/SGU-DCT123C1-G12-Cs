// TravelGuide.Core/Models/DeviceSession.cs
namespace TravelGuide.Core.Models;

/// <summary>
/// Theo dõi phiên hoạt động của thiết bị.
/// Mỗi lần app gọi API lần đầu (hoặc ping) sẽ upsert bản ghi này.
/// </summary>
public class DeviceSession
{
    /// <summary>Khoá chính — DeviceId do app tạo (Guid) và lưu local</summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>Platform: Android, iOS, Windows...</summary>
    public string Platform { get; set; } = string.Empty;

    /// <summary>Phiên bản hệ điều hành</summary>
    public string OsVersion { get; set; } = string.Empty;

    /// <summary>Phiên bản app</summary>
    public string AppVersion { get; set; } = string.Empty;

    /// <summary>Ngôn ngữ đang dùng</summary>
    public string LanguageCode { get; set; } = "vi";

    /// <summary>Lần cuối thiết bị hoạt động (UTC)</summary>
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

    /// <summary>Lần đầu thiết bị đăng ký (UTC)</summary>
    public DateTime FirstSeenAt { get; set; } = DateTime.UtcNow;

    /// <summary>Số lần scan QR trên thiết bị này</summary>
    public int ScanCount { get; set; } = 0;

    /// <summary>Tổng số lần mở app (ping)</summary>
    public int SessionCount { get; set; } = 0;

    /// <summary>Username nếu đã đăng nhập, null nếu Guest</summary>
    public string? Username { get; set; }
}
