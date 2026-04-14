namespace TravelGuide.Core.Constants;

/// <summary>
/// Danh sach ngon ngu ho tro toan he thong.
/// "vi" la ngon ngu mac dinh va fallback bat buoc.
/// Danh sach co the them/bot ve sau ma khong anh huong module khac.
/// </summary>
public static class LanguageConstants
{
    /// <summary>Ngon ngu mac dinh va fallback khi khong co noi dung theo ngon ngu yeu cau</summary>
    public const string Default = "vi";

    /// <summary>
    /// Danh sach ngon ngu ho tro: LanguageCode -> DisplayName
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> Supported =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "vi", "Tiếng Việt" },
            { "en", "English" },
            { "zh", "中文" },
            { "ja", "日本語" },
            { "ko", "한국어" },
            { "fr", "Français" }
        };

    /// <summary>Kiem tra xem language code co duoc ho tro hay khong</summary>
    public static bool IsSupported(string? code) =>
        !string.IsNullOrWhiteSpace(code) && Supported.ContainsKey(code);
}
