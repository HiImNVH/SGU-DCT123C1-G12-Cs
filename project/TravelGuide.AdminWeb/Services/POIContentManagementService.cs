using System.Net.Http.Headers;
using System.Net.Http.Json;
using TravelGuide.Core.Constants;
using TravelGuide.Core.DTOs;

namespace TravelGuide.AdminWeb.Services;

public interface IPOIContentManagementService
{
    Task<bool> UpsertContentAsync(Guid poiId, POIContentDto dto, string token);
    bool ValidateLanguage(string languageCode);
    IReadOnlyDictionary<string, string> GetSupportedLanguages();
}

/// <summary>
/// ContentModule: upload text, audio URL cho POI theo ngon ngu
/// </summary>
public class POIContentManagementService : IPOIContentManagementService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<POIContentManagementService> _logger;

    public POIContentManagementService(IHttpClientFactory httpClientFactory, ILogger<POIContentManagementService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Upload hoac cap nhat noi dung ngon ngu cho POI
    /// </summary>
    public async Task<bool> UpsertContentAsync(Guid poiId, POIContentDto dto, string token)
    {
        _logger.LogInformation("[info] - Upsert content POI id={Id}, lang={Lang}", poiId, dto.LanguageCode);

        // Validate language truoc khi goi API
        if (!ValidateLanguage(dto.LanguageCode))
        {
            _logger.LogWarning("[warn] - Language khong hop le: {Lang}", dto.LanguageCode);
            return false;
        }

        if (string.IsNullOrWhiteSpace(dto.NarrationText))
        {
            _logger.LogWarning("[warn] - NarrationText khong duoc trong");
            return false;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("TravelGuideAPI");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsJsonAsync($"/api/admin/poi/{poiId}/content", dto);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[warn] - Upsert content that bai, status={Status}", response.StatusCode);
                return false;
            }

            _logger.LogInformation("[info] - Da luu content POI id={Id}, lang={Lang}", poiId, dto.LanguageCode);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[error] - Loi khi upsert content: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Validate language code co nam trong danh sach ho tro khong
    /// </summary>
    public bool ValidateLanguage(string languageCode) =>
        LanguageConstants.IsSupported(languageCode);

    /// <summary>
    /// Tra ve danh sach ngon ngu ho tro de hien thi dropdown
    /// </summary>
    public IReadOnlyDictionary<string, string> GetSupportedLanguages() =>
        LanguageConstants.Supported;
}
