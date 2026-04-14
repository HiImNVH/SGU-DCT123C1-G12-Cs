// Models/POIContent.cs
namespace TravelGuide.Models
{
    public class POIContent
    {
        public Guid Id { get; set; }
        public Guid POIId { get; set; }
        public string LanguageCode { get; set; }   // "vi", "en", "ja"...
        public string NarrationText { get; set; }  // Văn bản thuyết minh (dùng cho TTS nếu không có audio)
        public string? AudioUrl { get; set; }      // URL file audio đã thu sẵn (MP3/WAV)
    }
}
