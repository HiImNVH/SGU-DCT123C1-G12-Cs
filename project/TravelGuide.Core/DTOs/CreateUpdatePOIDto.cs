namespace TravelGuide.Core.DTOs;

/// <summary>
/// Du lieu de tao POI moi.
/// Dung cho: POST /api/admin/poi
/// </summary>
public class CreatePOIDto
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    /// <summary>Mac dinh true khi tao moi</summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Du lieu de cap nhat POI da ton tai.
/// Dung cho: PUT /api/admin/poi/{id}
/// </summary>
public class UpdatePOIDto
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsActive { get; set; }
}
