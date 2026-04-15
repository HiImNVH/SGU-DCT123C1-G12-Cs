// TravelGuide.Core/DTOs/DeviceDtos.cs
namespace TravelGuide.Core.DTOs;

/// <summary>POST /api/device/ping — App gửi lên mỗi khi mở</summary>
public class DevicePingRequest
{
    public string DeviceId    { get; set; } = string.Empty;
    public string Platform    { get; set; } = string.Empty;
    public string OsVersion   { get; set; } = string.Empty;
    public string AppVersion  { get; set; } = string.Empty;
    public string LanguageCode{ get; set; } = "vi";
    public string? Username   { get; set; }
}

/// <summary>POST /api/device/scan — App gửi mỗi khi scan QR thành công</summary>
public class DeviceScanRequest
{
    public string DeviceId { get; set; } = string.Empty;
}

/// <summary>GET /api/admin/devices/stats — Tổng hợp cho dashboard</summary>
public class DeviceStatsDto
{
    public int TotalDevices      { get; set; }
    public int ActiveToday       { get; set; }  // LastSeenAt trong 24h
    public int ActiveThisWeek    { get; set; }  // LastSeenAt trong 7 ngày
    public int ActiveThisMonth   { get; set; }  // LastSeenAt trong 30 ngày
    public int LoggedInDevices   { get; set; }  // Username != null
    public int GuestDevices      { get; set; }  // Username == null
    public int TotalScans        { get; set; }
    public List<LanguageStatDto> ByLanguage  { get; set; } = new();
    public List<PlatformStatDto> ByPlatform  { get; set; } = new();
    public List<DeviceListItemDto> RecentDevices { get; set; } = new();
}

public class LanguageStatDto
{
    public string LanguageCode { get; set; } = string.Empty;
    public int    Count        { get; set; }
}

public class PlatformStatDto
{
    public string Platform { get; set; } = string.Empty;
    public int    Count    { get; set; }
}

public class DeviceListItemDto
{
    public string   DeviceId     { get; set; } = string.Empty;
    public string   Platform     { get; set; } = string.Empty;
    public string   OsVersion    { get; set; } = string.Empty;
    public string   AppVersion   { get; set; } = string.Empty;
    public string   LanguageCode { get; set; } = string.Empty;
    public string?  Username     { get; set; }
    public DateTime LastSeenAt   { get; set; }
    public DateTime FirstSeenAt  { get; set; }
    public int      ScanCount    { get; set; }
    public int      SessionCount { get; set; }
    public bool     IsOnline     { get; set; }  // LastSeenAt < 5 phút
}
