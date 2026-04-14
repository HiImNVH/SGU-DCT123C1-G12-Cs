namespace TravelGuide.Core.DTOs;

/// <summary>
/// Du lieu day du cua mot POI — tra ve sau khi scan QR.
/// Bao gom noi dung ngon ngu da duoc chon (hoac fallback "vi").
/// Dung cho: GET /api/poi/{id}?lang=vi
/// </summary>
public class POIDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    /// <summary>
    /// Noi dung theo ngon ngu yeu cau (hoac fallback "vi").
    /// Co the null neu POI chua co noi dung nao.
    /// </summary>
    public POIContentDto? Content { get; set; }
}
