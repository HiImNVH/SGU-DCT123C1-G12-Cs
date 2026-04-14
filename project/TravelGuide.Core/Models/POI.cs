namespace TravelGuide.Core.Models;

/// <summary>
/// Diem tham quan (Point of Interest).
/// Pham vi: SQL Server (server) + SQLite (local cache tren MAUI).
/// Quan he: 1 POI -> nhieu POIContent | 1 POI -> 1 QRCode.
/// </summary>
public class POI
{
    /// <summary>Khoa chinh, dinh danh duy nhat</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Ten gian hang / diem tham quan</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Danh muc (Am thuc, Thu cong, Di tich...)</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>URL anh dai dien (co the null)</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Vi do de hien thi tren ban do</summary>
    public double Latitude { get; set; }

    /// <summary>Kinh do de hien thi tren ban do</summary>
    public double Longitude { get; set; }

    /// <summary>Trang thai hien thi — xoa mem khi IsActive = false</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Navigation property: noi dung da ngon ngu</summary>
    public List<POIContent> Contents { get; set; } = new();

    /// <summary>Navigation property: ma QR tuong ung</summary>
    public QRCode? QRCode { get; set; }
}
