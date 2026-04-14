using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace TravelGuide.AdminWeb.Services;

public class QRResult
{
    public Guid PoiId { get; set; }
    public string QrImageBase64 { get; set; } = string.Empty;
    public string Format { get; set; } = "PNG";

    /// <summary>Data URI de dung truc tiep trong the img src</summary>
    public string DataUri => $"data:image/png;base64,{QrImageBase64}";
}

public interface IQRManagementService
{
    Task<QRResult?> GenerateQRAsync(Guid poiId, string token);
    string GetDownloadFileName(Guid poiId);
}

/// <summary>
/// QRModule: generate QR tu API, preview va download
/// </summary>
public class QRManagementService : IQRManagementService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<QRManagementService> _logger;

    public QRManagementService(IHttpClientFactory httpClientFactory, ILogger<QRManagementService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Goi API generate QR, tra ve base64 PNG
    /// </summary>
    public async Task<QRResult?> GenerateQRAsync(Guid poiId, string token)
    {
        _logger.LogInformation("[info] - Generate QR cho POI id={Id}", poiId);
        try
        {
            var client = _httpClientFactory.CreateClient("TravelGuideAPI");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"/api/admin/poi/{poiId}/qr");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[warn] - Generate QR that bai, status={Status}", response.StatusCode);
                return null;
            }

            var data = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            if (data == null)
            {
                _logger.LogWarning("[warn] - Khong doc duoc QR response");
                return null;
            }

            var result = new QRResult
            {
                PoiId = poiId,
                QrImageBase64 = data["qrImageBase64"]?.ToString() ?? string.Empty,
                Format = data["format"]?.ToString() ?? "PNG"
            };

            _logger.LogInformation("[info] - Da generate QR thanh cong cho POI id={Id}", poiId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[error] - Loi khi generate QR: {Message}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Tra ve ten file khi download QR
    /// </summary>
    public string GetDownloadFileName(Guid poiId) =>
        $"QR_POI_{poiId:N}.png";
}
