// Services/ApiPOIService.cs
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TravelGuide.Models;

namespace TravelGuide.Services
{
    public class ApiPOIService : IPOIService
    {
        private readonly HttpClient _http;
        private readonly IAuthService _authService;
        private const string BaseUrl = "http://10.0.2.2:5042";

        public ApiPOIService(IAuthService authService)
        {
            _authService = authService;
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            };
            _http = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        }

        private async Task SetAuthHeader()
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<POI>> GetAllAsync()
        {
            try
            {
                await SetAuthHeader();
                var response = await _http.GetAsync("/api/poi");
                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<POI>>(json, options) ?? new List<POI>();
            }
            catch { return new List<POI>(); }
        }

        public async Task<List<POI>> GetNearbyAsync(double lat, double lng, int radius = 1000)
        {
            try
            {
                await SetAuthHeader();
                var response = await _http.GetAsync($"/api/poi/nearby?lat={lat}&lng={lng}&radius={radius}");
                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<POI>>(json, options) ?? new List<POI>();
            }
            catch { return new List<POI>(); }
        }

        public async Task<POIContent> GetContentAsync(Guid poiId, string languageCode)
        {
            try
            {
                await SetAuthHeader();
                var response = await _http.GetAsync($"/api/poi/{poiId}/content/{languageCode}");
                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<POIContent>(json, options);
            }
            catch { return null; }
        }

        public async Task<POI> GetByIdAsync(Guid id)
        {
            try
            {
                await SetAuthHeader();
                var response = await _http.GetAsync($"/api/poi/{id}");
                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<POI>(json, options);
            }
            catch { return null; }
        }

        public async Task<bool> RateAsync(Guid poiId, int rating, string? comment)
        {
            try
            {
                await SetAuthHeader();
                var response = await _http.PostAsJsonAsync(
                    $"/api/poi/{poiId}/ratings",
                    new { rating, comment });
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }
    }
}