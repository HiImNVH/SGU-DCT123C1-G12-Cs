namespace TravelGuide.Core.Models;

/// <summary>
/// Ma QR duoc gan vao mot POI.
/// Pham vi: SQL Server (server).
/// Quan he: 1 POI -> 1 QRCode (one-to-one).
///
/// EncodedValue thong thuong la poiId.ToString()
/// — MAUI doc gia tri nay sau khi scan roi goi API.
/// </summary>
public class QRCode
{
    /// <summary>Khoa chinh</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Khoa ngoai -> POI</summary>
    public Guid POIId { get; set; }

    /// <summary>Gia tri duoc ma hoa vao QR (thuong la poiId dang string)</summary>
    public string EncodedValue { get; set; } = string.Empty;

    /// <summary>Thoi diem tao QR</summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Navigation property</summary>
    public POI? POI { get; set; }
}
