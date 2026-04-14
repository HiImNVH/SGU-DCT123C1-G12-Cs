namespace TravelGuide.Core.DTOs;

/// <summary>
/// Noi dung da ngon ngu cua mot POI.
/// Dung cho:
///   - POST /api/admin/poi/{id}/content  (Admin upload)
///   - Nested trong POIDetailDto          (tra ve cho client)
/// </summary>
public class POIContentDto
{
    /// <summary>Ma ngon ngu theo BCP-47: vi, en, zh, ja, ko, fr</summary>
    public string LanguageCode { get; set; } = string.Empty;

    /// <summary>Van ban thuyet minh — dung cho TTS neu khong co AudioUrl</summary>
    public string NarrationText { get; set; } = string.Empty;

    /// <summary>URL file audio da thu san (MP3/WAV) — co the null</summary>
    public string? AudioUrl { get; set; }
}
