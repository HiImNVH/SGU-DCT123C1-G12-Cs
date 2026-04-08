// TravelGuide.Admin/Services/ITranslationService.cs
public interface ITranslationService
{
    /// <summary>
    /// Dịch văn bản từ ngôn ngữ nguồn sang ngôn ngữ đích
    /// </summary>
    Task<string?> TranslateAsync(string text, string sourceLang, string targetLang);
}