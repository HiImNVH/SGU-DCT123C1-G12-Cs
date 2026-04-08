using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using TravelGuide.Admin.Models;

namespace TravelGuide.Admin.Services
{
    public class AdminApiService : IAdminApiService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;
        private readonly ILogger<AdminApiService> _logger;
        private string? _token;
        private bool _initialized = false;

        public AdminApiService(
    IHttpClientFactory httpClientFactory,
    IJSRuntime js,
    ILogger<AdminApiService> logger)
        {
            _http = httpClientFactory.CreateClient("AdminApi");
            _js = js;
            _logger = logger;
        }

        public bool IsLoggedIn() => !string.IsNullOrEmpty(_token);

        public async Task InitializeAsync()
        {
            if (_initialized) return;

            try
            {
                _token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");
                if (!string.IsNullOrEmpty(_token))
                {
                    SetAuthHeader();
                    _logger.LogInformation("[AUTH] Khoi phuc token thanh cong tu localStorage");
                }
                else
                {
                    _logger.LogInformation("[AUTH] Khong co token, yeu cau dang nhap");
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("[AUTH] JSInterop chua san sang (prerendering): {Msg}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("[AUTH] Loi khoi phuc token: {Msg}", ex.Message);
            }
            finally
            {
                _initialized = true;
            }
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            _logger.LogInformation("[AUTH] Dang nhap voi tai khoan: {User}", username);
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/login",
                    new { username, password });

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("[AUTH] Dang nhap that bai - HTTP {Code}", response.StatusCode);
                    return false;
                }

                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (result.TryGetProperty("success", out var s) && s.GetBoolean())
                {
                    _token = result.GetProperty("token").GetString();
                    
                    // Lưu token vào localStorage
                    try
                    {
                        await _js.InvokeVoidAsync("localStorage.setItem", "authToken", _token);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("[AUTH] Khong the luu token vao localStorage: {Msg}", ex.Message);
                    }
                    
                    SetAuthHeader();
                    _logger.LogInformation("[AUTH] Dang nhap thanh cong: {User}", username);
                    return true;
                }

                _logger.LogWarning("[AUTH] Dang nhap that bai: sai thong tin");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("[AUTH] Loi ket noi khi dang nhap: {Msg}", ex.Message);
                return false;
            }
        }

        public async Task Logout()
        {
            _logger.LogInformation("[AUTH] Dang xuat khoi he thong");
            _token = null;
            
            try
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[AUTH] Khong the xoa token tu localStorage: {Msg}", ex.Message);
            }
            
            _http.DefaultRequestHeaders.Authorization = null;
        }

        private void SetAuthHeader()
        {
            if (!string.IsNullOrEmpty(_token))
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);
        }

        public async Task<List<POIDto>> GetPOIsAsync()
        {
            _logger.LogInformation("[POI] Lay danh sach POI");
            try
            {
                SetAuthHeader();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var json = await _http.GetStringAsync("api/poi");
                var list = JsonSerializer.Deserialize<List<POIDto>>(json, options) ?? new();
                _logger.LogInformation("[POI] Lay duoc {Count} dia diem", list.Count);
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError("[POI] Loi lay danh sach POI: {Msg}", ex.Message);
                return new();
            }
        }

        public async Task<POIDto?> GetPOIAsync(Guid id)
        {
            _logger.LogInformation("[POI] Lay chi tiet POI: {Id}", id);
            try
            {
                SetAuthHeader();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var json = await _http.GetStringAsync($"api/poi/{id}");
                return JsonSerializer.Deserialize<POIDto>(json, options);
            }
            catch (Exception ex)
            {
                _logger.LogError("[POI] Loi lay chi tiet POI {Id}: {Msg}", id, ex.Message);
                return null;
            }
        }

        public async Task<bool> CreatePOIAsync(CreatePOIRequest request)
        {
            _logger.LogInformation("[POI] Tao moi POI: {Name}", request.Name);
            try
            {
                SetAuthHeader();
                var response = await _http.PostAsJsonAsync("api/poi", request);
                Console.WriteLine($"[DEBUG] Status: {response.StatusCode}");
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Response: {content}");
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[POI] Tao POI thanh cong: {Name}", request.Name);
                    return true;
                }

                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError("[POI] Tao POI that bai ({Code}): {Err}",
                    response.StatusCode, err);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("[POI] Loi tao POI: {Msg}", ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdatePOIAsync(Guid id, CreatePOIRequest request)
        {
            _logger.LogInformation("[POI] Cap nhat POI: {Id}", id);
            try
            {
                SetAuthHeader();
                var response = await _http.PutAsJsonAsync($"api/poi/{id}", request);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[POI] Cap nhat POI thanh cong: {Id}", id);
                    return true;
                }

                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError("[POI] Cap nhat POI that bai ({Code}): {Err}",
                    response.StatusCode, err);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("[POI] Loi cap nhat POI: {Msg}", ex.Message);
                return false;
            }
        }

        public async Task<bool> DeletePOIAsync(Guid id)
        {
            _logger.LogInformation("[POI] Xoa POI: {Id}", id);
            try
            {
                SetAuthHeader();
                var response = await _http.DeleteAsync($"api/poi/{id}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[POI] Xoa POI thanh cong: {Id}", id);
                    return true;
                }

                _logger.LogWarning("[POI] Xoa POI that bai: {Id}", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("[POI] Loi xoa POI: {Msg}", ex.Message);
                return false;
            }
        }

        public async Task<List<UserDto>> GetUsersAsync()
        {
            _logger.LogInformation("[USER] Lay danh sach nguoi dung");
            try
            {
                SetAuthHeader();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var json = await _http.GetStringAsync("api/users");
                var list = JsonSerializer.Deserialize<List<UserDto>>(json, options) ?? new();
                _logger.LogInformation("[USER] Lay duoc {Count} nguoi dung", list.Count);
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError("[USER] Loi lay danh sach nguoi dung: {Msg}", ex.Message);
                return new();
            }
        }

        public async Task<bool> ToggleUserActiveAsync(Guid id)
        {
            _logger.LogInformation("[USER] Thay doi trang thai tai khoan: {Id}", id);
            try
            {
                SetAuthHeader();
                var response = await _http.PostAsync($"api/users/toggle/{id}", null);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[USER] Thay doi trang thai thanh cong: {Id}", id);
                    return true;
                }

                _logger.LogWarning("[USER] Thay doi trang thai that bai: {Id}", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("[USER] Loi thay doi trang thai: {Msg}", ex.Message);
                return false;
            }
        }
    }
}