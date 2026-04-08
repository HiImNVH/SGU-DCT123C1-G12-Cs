using System.Text.Json;
using System.Web;

namespace TravelGuide.Admin.Services
{
    public class TranslationService : ITranslationService
    {
        private readonly HttpClient _http;
        private readonly ILogger<TranslationService> _logger;

        public TranslationService(IHttpClientFactory httpClientFactory, ILogger<TranslationService> logger)
        {
            _http = httpClientFactory.CreateClient("MyMemory");
            _logger = logger;
        }

        public async Task<string?> TranslateAsync(string text, string sourceLang, string targetLang)
        {
            if (string.IsNullOrEmpty(text)) return null;

            try
            {
                var encodedText = HttpUtility.UrlEncode(text);
                var url = $"get?q={encodedText}&langpair={sourceLang}|{targetLang}";

                _logger.LogInformation("[TRANSLATE] Translating from {Source} to {Target}", sourceLang, targetLang);

                var response = await _http.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("[TRANSLATE] Failed: {Status}", response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(json);
                
                var translatedText = result
                    .GetProperty("responseData")
                    .GetProperty("translatedText")
                    .GetString();

                _logger.LogInformation("[TRANSLATE] Success: {Length} chars", translatedText?.Length ?? 0);
                return translatedText;
            }
            catch (Exception ex)
            {
                _logger.LogError("[TRANSLATE] Error: {Message}", ex.Message);
                return null;
            }
        }
    }
}