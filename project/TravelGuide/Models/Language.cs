// Models/Language.cs
namespace TravelGuide.Models
{
    public class Language
    {
        public string Code { get; set; }        // PK: "vi", "en", "ja"
        public string Name { get; set; }        // "Vietnamese"
        public string NativeName { get; set; }  // "Tiếng Việt"
        public bool IsActive { get; set; } = true;
    }
}