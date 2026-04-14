// Models/Language.cs
namespace TravelGuide.Models
{
    public class Language
    {
        public string Code { get; set; }        // "vi", "en", "ja"...
        public string Name { get; set; }        // "Vietnamese"
        public string NativeName { get; set; }  // "Tiếng Việt"
        public bool IsActive { get; set; } = true;

        /// <summary>Danh sách ngôn ngữ hỗ trợ theo PRD (LanguagePreference)</summary>
        public static readonly List<Language> Supported = new()
        {
            new Language { Code = "vi", Name = "Vietnamese", NativeName = "Tiếng Việt (mặc định)", IsActive = true },
            new Language { Code = "en", Name = "English",    NativeName = "English",                IsActive = true },
            new Language { Code = "zh", Name = "Chinese",    NativeName = "中文",                   IsActive = true },
            new Language { Code = "ja", Name = "Japanese",   NativeName = "日本語",                 IsActive = true },
            new Language { Code = "ko", Name = "Korean",     NativeName = "한국어",                  IsActive = true },
            new Language { Code = "fr", Name = "French",     NativeName = "Français",               IsActive = true },
        };
    }
}
