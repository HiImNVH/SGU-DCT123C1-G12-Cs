using System.Net.Http.Json;

namespace TravelGuide.AdminWeb.Services;

public class TranslationResult
{
    public string TargetLanguage { get; set; } = string.Empty;
    public string TranslatedText { get; set; } = string.Empty;
    public bool Success { get; set; }
}

public interface ITranslationService
{
    Task<List<TranslationResult>> TranslateToAllLanguagesAsync(string vietnameseText);
    Task<TranslationResult> TranslateAsync(string text, string targetLang);
}

/// <summary>
/// Dich van ban tieng Viet sang cac ngon ngu khac dung MyMemory API (mien phi)
/// </summary>
public class TranslationService : ITranslationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TranslationService> _logger;

    // Ngon ngu can dich (bo qua "vi" vi da la nguon)
    private readonly Dictionary<string, string> _targetLanguages = new()
{
    { "en", "en" },   // ✅ bỏ -US
    { "zh", "zh" },   // ✅ bỏ -CN
    { "ja", "ja" },   // ✅ bỏ -JP
    { "ko", "ko" },   // ✅ bỏ -KR
    { "fr", "fr" }    // ✅ bỏ -FR
};

    public TranslationService(IHttpClientFactory httpClientFactory, ILogger<TranslationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Dich van ban tieng Viet sang tat ca ngon ngu con lai
    /// </summary>
    public async Task<List<TranslationResult>> TranslateToAllLanguagesAsync(string vietnameseText)
    {
        _logger.LogInformation("[info] - Bat dau dich van ban sang {Count} ngon ngu", _targetLanguages.Count);

        var results = new List<TranslationResult>();

        foreach (var lang in _targetLanguages)
        {
            var result = await TranslateAsync(vietnameseText, lang.Key);
            results.Add(result);
            // Delay nho de tranh rate limit
            await Task.Delay(300);
        }

        _logger.LogInformation("[info] - Da dich xong {Count} ngon ngu", results.Count(r => r.Success));
        return results;
    }

    /// <summary>
    /// Dich van ban sang mot ngon ngu cu the
    /// </summary>
    public async Task<TranslationResult> TranslateAsync(string text, string targetLang)
    {
        _logger.LogInformation("[info] - Dich sang {Lang}", targetLang);
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            var targetCode = _targetLanguages.GetValueOrDefault(targetLang, targetLang);
            // ✅ Format đúng: vi|en (không có region code)
            var url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair=vi|{targetCode}";

            var response = await client.GetFromJsonAsync<MyMemoryResponse>(url);

            if (response?.ResponseStatus == 200 &&
                !string.IsNullOrWhiteSpace(response.ResponseData?.TranslatedText))
            {
                _logger.LogInformation("[info] - Dich thanh cong sang {Lang}: {Text}",
                    targetLang, response.ResponseData.TranslatedText[..Math.Min(50, response.ResponseData.TranslatedText.Length)]);
                return new TranslationResult
                {
                    TargetLanguage = targetLang,
                    TranslatedText = response.ResponseData.TranslatedText,
                    Success = true
                };
            }

            _logger.LogWarning("[warn] - Dich that bai sang {Lang}, status={Status}",
                targetLang, response?.ResponseStatus);
            return new TranslationResult { TargetLanguage = targetLang, Success = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[error] - Loi khi dich sang {Lang}: {Message}", targetLang, ex.Message);
            return new TranslationResult { TargetLanguage = targetLang, Success = false };
        }
    }

    // Response model cua MyMemory API
    private class MyMemoryResponse
    {
        public ResponseData ResponseData { get; set; } = new();
        public int ResponseStatus { get; set; }
    }

    private class ResponseData
    {
        public string TranslatedText { get; set; } = string.Empty;
    }
}