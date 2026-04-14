namespace TravelGuide.Core.Models;

/// <summary>
/// Noi dung da ngon ngu cua mot POI.
/// Pham vi: SQL Server (server) + SQLite (local cache tren MAUI).
///
/// Rang buoc quan trong:
///   - Moi POI chi co MOT ban ghi cho moi LanguageCode (unique index: POIId + LanguageCode).
///   - BAT BUOC phai co ban ghi LanguageCode = "vi" lam fallback.
/// </summary>
public class POIContent
{
    /// <summary>Khoa chinh</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Khoa ngoai -> POI</summary>
    public Guid POIId { get; set; }

    /// <summary>Ma ngon ngu theo chuan BCP-47 (vi, en, zh, ja, ko, fr...)</summary>
    public string LanguageCode { get; set; } = string.Empty;

    /// <summary>Van ban thuyet minh — dung cho TTS neu khong co audio</summary>
    public string NarrationText { get; set; } = string.Empty;

    /// <summary>URL file audio da thu san (MP3/WAV) — co the null</summary>
    public string? AudioUrl { get; set; }

    /// <summary>Navigation property</summary>
    public POI? POI { get; set; }
}
