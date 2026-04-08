// Models/POIContent.cs
using System;

namespace TravelGuide.Models
{
    /// <summary>
    /// Nội dung thuyết minh của một POI theo từng ngôn ngữ.
    /// </summary>
    public class POIContent
    {
        public Guid Id { get; set; }
        public Guid POIId { get; set; }
        public string LanguageCode { get; set; }    // "vi", "en", "ja"...
        public string Title { get; set; }
        public string NarrationText { get; set; }   // Text để TTS đọc
        public string AudioUrl { get; set; }        // URL file audio (nếu có)
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public POI POI { get; set; }
        public Language Language { get; set; }
    }
}