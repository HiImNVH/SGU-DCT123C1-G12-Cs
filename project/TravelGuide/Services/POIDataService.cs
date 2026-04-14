using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TravelGuide.Constants;
using TravelGuide.Helpers;
using TravelGuide.Models;
using TravelGuide.Models.DTOs;

namespace TravelGuide.Services
{
    public class POIDataService
    {
        private readonly HttpClient _http;
        private readonly CacheService _cache;
        private readonly AuthService _auth;

        private readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public POIDataService(CacheService cache, AuthService auth)
        {
            _cache = cache;
            _auth  = auth;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            };
            _http = new HttpClient(handler)
            {
                BaseAddress = new Uri(AppConstants.BaseUrl),
                Timeout     = TimeSpan.FromSeconds(15)
            };
        }

        /// <summary>
        /// Lấy tất cả POI active - GET /api/poi
        /// Cache-first → API fallback khi offline
        /// </summary>
        public async Task<List<POISummaryDto>> GetAllActiveAsync()
        {
            Console.WriteLine("[log] - Lay danh sach POI active");

            // Offline: dùng cache
            if (!NetworkHelper.IsConnected)
            {
                Console.WriteLine("[warn] - Offline - lay tu cache");
                return await GetAllFromCacheAsync();
            }

            try
            {
                SetAuthHeader();
                var url = AppConstants.ApiPoi;
                Console.WriteLine($"[log] - GET {AppConstants.BaseUrl}{url}");

                var response = await _http.GetAsync(url);
                var json     = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[log] - Response status: {(int)response.StatusCode}");
                Console.WriteLine($"[log] - Response body: {json[..Math.Min(200, json.Length)]}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[error] - API loi {(int)response.StatusCode}");
                    return await GetAllFromCacheAsync();
                }

                var list = JsonSerializer.Deserialize<List<POISummaryDto>>(json, _jsonOpts) ?? new();
                Console.WriteLine($"[info] - Lay duoc {list.Count} POI tu API");
                return list;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[error] - Loi ket noi API: {ex.Message}");
                Console.WriteLine("[warn] - Fallback ve cache");
                return await GetAllFromCacheAsync();
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("[error] - Timeout khi goi GET /api/poi");
                return await GetAllFromCacheAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[error] - Loi GetAllActive: {ex.Message}");
                return new();
            }
        }

        /// <summary>
        /// Lấy chi tiết POI - GET /api/poi/{id}?lang=vi
        /// Cache-first → API → offline fallback
        /// </summary>
        public async Task<(POIDetailDto? data, bool fromCache)> GetPOIByIdAsync(Guid poiId, string lang)
        {
            Console.WriteLine($"[log] - Lay chi tiet POI: {poiId} lang={lang}");

            // 1. Check cache
            var cached = await _cache.GetPOIAsync(poiId, lang);
            if (cached != null)
            {
                Console.WriteLine("[info] - Load tu cache");
                return (MapCacheToDto(cached), true);
            }

            // 2. Không có cache + offline
            if (!NetworkHelper.IsConnected)
            {
                Console.WriteLine("[warn] - Offline, khong co cache");
                return (null, false);
            }

            // 3. Gọi API
            try
            {
                SetAuthHeader();
                var url = $"{AppConstants.ApiPoi}/{poiId}?lang={lang}";
                Console.WriteLine($"[log] - GET {AppConstants.BaseUrl}{url}");

                var response = await _http.GetAsync(url);
                var json     = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[log] - Response: {(int)response.StatusCode}");
                Console.WriteLine($"[log] - Body: {json[..Math.Min(300, json.Length)]}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[error] - API loi {(int)response.StatusCode}");
                    return (null, false);
                }

                var dto = JsonSerializer.Deserialize<POIDetailDto>(json, _jsonOpts);
                if (dto == null)
                {
                    Console.WriteLine("[error] - Deserialize POIDetailDto = null");
                    return (null, false);
                }

                // Fallback về vi nếu không có content
                if (dto.Content == null && lang != "vi")
                {
                    Console.WriteLine($"[warn] - Khong co content {lang}, fallback vi");
                    return await GetPOIByIdAsync(poiId, "vi");
                }

                // Lưu cache
                await _cache.SavePOIAsync(dto, lang);
                Console.WriteLine($"[info] - Da lay POI tu API: {dto.Name}");
                return (dto, false);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[error] - Loi ket noi API: {ex.Message}");
                return (null, false);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("[error] - Timeout khi goi GET /api/poi/{id}");
                return (null, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[error] - Loi GetPOIById: {ex.Message}");
                return (null, false);
            }
        }

        private async Task<List<POISummaryDto>> GetAllFromCacheAsync()
        {
            var cached = await _cache.GetAllPOIsAsync();
            return cached
                .GroupBy(c => c.POIId)
                .Select(g => g.First())
                .Select(c => new POISummaryDto
                {
                    Id        = Guid.Parse(c.POIId),
                    Name      = c.Name,
                    Category  = c.Category,
                    ImageUrl  = c.ImageUrl,
                    Latitude  = c.Latitude,
                    Longitude = c.Longitude,
                    IsActive  = true
                }).ToList();
        }

        private void SetAuthHeader()
        {
            var token = SecureStorage.GetAsync(AppConstants.PrefKeyAuthToken).Result;
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }

        private POIDetailDto MapCacheToDto(LocalCacheEntry c) => new()
        {
            Id        = Guid.Parse(c.POIId),
            Name      = c.Name,
            Category  = c.Category,
            ImageUrl  = c.LocalImagePath ?? c.ImageUrl,
            Latitude  = c.Latitude,
            Longitude = c.Longitude,
            Content   = new POIContentDto
            {
                LanguageCode  = c.LanguageCode,
                NarrationText = c.NarrationText,
                AudioUrl      = c.LocalAudioPath ?? c.AudioUrl
            }
        };
    }
}
