// TravelGuide.AdminWeb/Services/QRManagementService.cs
using System.Net.Http.Headers;
using TravelGuide.AdminWeb.Models;

namespace TravelGuide.AdminWeb.Services
{
    /// <summary>
    /// Interface cho QR Management Service
    /// </summary>
    public interface IQRManagementService
    {
        Task<QRResult?> GenerateQRAsync(Guid poiId, string token);
        Task<string?> GetQRBase64Async(string poiId, string? adminJwt);
        string BuildDeepLink(string poiId);
        string BuildFallbackUrl(string poiId);
    }

    /// <summary>
    /// Service quản lý tạo QR code cho POI
    /// </summary>
    public class QRManagementService : IQRManagementService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // Deep-link scheme khớp với IntentFilter trong MainActivity.cs
        private const string DeepLinkScheme = "travelguide";

        // URL fallback khi chưa cài app
        private const string FallbackBaseUrl =
            "https://hiimnvh.github.io/SGU-DCT123C1-G12-Cs/qr-redirect.html";

        public QRManagementService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Tạo QR code và trả về QRResult với đầy đủ thông tin
        /// </summary>
        public async Task<QRResult?> GenerateQRAsync(Guid poiId, string token)
        {
            try
            {
                var poiIdStr = poiId.ToString();
                var base64Image = await GetQRBase64Async(poiIdStr, token);
                
                if (string.IsNullOrEmpty(base64Image))
                    return null;

                var deepLink = BuildDeepLink(poiIdStr);
                var dataUri = $"data:image/png;base64,{base64Image}";

                return new QRResult
                {
                    QrImageBase64 = base64Image,
                    DataUri = dataUri,
                    DeepLink = deepLink
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Lấy QR code (base64 PNG) cho POI.
        /// API backend sẽ encode chuỗi được truyền vào thành ảnh QR.
        /// </summary>
        public async Task<string?> GetQRBase64Async(string poiId, string? adminJwt)
        {
            if (string.IsNullOrWhiteSpace(poiId)) return null;

            try
            {
                var encodedValue = BuildDeepLink(poiId);
                var httpClient = _httpClientFactory.CreateClient("TravelGuideAPI");

                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"/api/admin/poi/{poiId}/qr?encodedValue={Uri.EscapeDataString(encodedValue)}");

                if (!string.IsNullOrWhiteSpace(adminJwt))
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", adminJwt);

                var resp = await httpClient.SendAsync(request);
                if (!resp.IsSuccessStatusCode) return null;

                var json = await resp.Content.ReadFromJsonAsync<QRResponse>();
                return json?.QrImageBase64;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Tạo deep-link URL: travelguide://poi/{poiId}
        /// Android sẽ intercept URL này nếu app đã cài (qua IntentFilter).
        /// </summary>
        public string BuildDeepLink(string poiId)
            => $"{DeepLinkScheme}://poi/{poiId}";

        /// <summary>
        /// Tạo HTTP fallback URL cho QR — dùng khi muốn QR hoạt động cả trên
        /// trình duyệt (chưa cài app). Trỏ đến trang redirect trên GitHub Pages.
        /// </summary>
        public string BuildFallbackUrl(string poiId)
            => $"{FallbackBaseUrl}?poi={Uri.EscapeDataString(poiId)}";

        private record QRResponse(string? QrImageBase64, string? Format);
    }
}