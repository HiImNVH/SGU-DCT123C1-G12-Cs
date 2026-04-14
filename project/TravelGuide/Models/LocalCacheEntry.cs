// Models/LocalCacheEntry.cs
using SQLite;

namespace TravelGuide.Models
{
    /// <summary>
    /// Bản ghi cache ngoại tuyến - lưu vào SQLite trên thiết bị
    /// Khoá chính composite: POIId + LanguageCode
    /// </summary>
    [Table("LocalCacheEntry")]
    public class LocalCacheEntry
    {
        [PrimaryKey]
        public string Id { get; set; } // "{POIId}_{LanguageCode}"

        public string POIId { get; set; }
        public string LanguageCode { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string? ImageUrl { get; set; }
        public string NarrationText { get; set; }
        public string? AudioUrl { get; set; }
        public string? LocalAudioPath { get; set; }  // Đường dẫn file audio đã tải về máy
        public string? LocalImagePath { get; set; }  // Đường dẫn ảnh đã tải về máy
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime CachedAt { get; set; }       // Thời điểm cache (dùng để kiểm tra hết hạn)

        // Helper để tạo Id
        public static string MakeId(Guid poiId, string lang) => $"{poiId}_{lang}";
    }
}
