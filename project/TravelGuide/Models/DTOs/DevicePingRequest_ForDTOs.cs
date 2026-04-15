// Thêm vào TravelGuide/Models/DTOs/DTOs.cs

// ── Device Tracking ────────────────────────────────────────────────

/// <summary>POST /api/device/ping</summary>
public class DevicePingRequest
{
    public string DeviceId     { get; set; } = string.Empty;
    public string Platform     { get; set; } = string.Empty;
    public string OsVersion    { get; set; } = string.Empty;
    public string AppVersion   { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = "vi";
    public string? Username    { get; set; }
}
