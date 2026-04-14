namespace TravelGuide.Core.DTOs;

/// <summary>
/// Du lieu tom tat cua POI — dung cho hien thi danh sach ban do (Map markers).
/// Khong bao gom noi dung ngon ngu de giam tai du lieu.
/// Dung cho: GET /api/poi?active=true
/// </summary>
public class POISummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsActive { get; set; }
}
