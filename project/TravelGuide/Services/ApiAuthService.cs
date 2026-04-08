// Services/ApiAuthService.cs
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TravelGuide.Models;
using TravelGuide.Models.DTOs;

namespace TravelGuide.Services
{
    public class ApiAuthService : IAuthService
    {
        private readonly HttpClient _http;
        private User _currentUser;

        // ⚠️ Android emulator dùng 10.0.2.2 thay cho localhost
        private const string BaseUrl = "http://10.0.2.2:5042";

        public ApiAuthService()
        {
            var handler = new HttpClientHandler
            {
                // Bỏ qua SSL certificate khi dev
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            };
            _http = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("/api/auth/login", request);
                var json     = await response.Content.ReadAsStringAsync();
                var result   = JsonSerializer.Deserialize<ApiAuthResponse>(json,
                                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.Success == true)
                {
                    _currentUser = result.User;
                    await SecureStorage.SetAsync("auth_token", result.Token);
                    return new AuthResponse { Success = true, Token = result.Token, User = result.User };
                }

                return new AuthResponse { Success = false, ErrorMessage = result?.Message ?? "Đăng nhập thất bại" };
            }
            catch (Exception ex)
            {
                return new AuthResponse { Success = false, ErrorMessage = $"Lỗi kết nối: {ex.Message}" };
            }
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("/api/auth/register", request);
                var json     = await response.Content.ReadAsStringAsync();
                var result   = JsonSerializer.Deserialize<ApiAuthResponse>(json,
                                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.Success == true)
                {
                    _currentUser = result.User;
                    await SecureStorage.SetAsync("auth_token", result.Token);
                    return new AuthResponse { Success = true, Token = result.Token, User = result.User };
                }

                return new AuthResponse { Success = false, ErrorMessage = result?.Message ?? "Đăng ký thất bại" };
            }
            catch (Exception ex)
            {
                return new AuthResponse { Success = false, ErrorMessage = $"Lỗi kết nối: {ex.Message}" };
            }
        }
        public async Task<bool> UpdateLanguageAsync(Guid userId, string languageCode)
        {
            try
            {
                var token = await SecureStorage.GetAsync("auth_token");

                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var content = JsonContent.Create(new
                {
                    languageCode = languageCode
                });

                // 🚨 API ĐÚNG CỦA BẠN
                var request = new HttpRequestMessage(new HttpMethod("PATCH"),
                    $"/api/users/{userId}/language")
                {
                    Content = content
                };

                var response = await client.SendAsync(request);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        public async Task LogoutAsync()
        {
            _currentUser = null;
            SecureStorage.Remove("auth_token");
            SecureStorage.Remove("user_id");
            await Task.CompletedTask;
        }

        public User GetCurrentUser() => _currentUser;
        public bool IsLoggedIn()     => _currentUser != null;
    }

    // Response model từ API
    public class ApiAuthResponse
    {
        public bool   Success { get; set; }
        public string Token   { get; set; }
        public string Message { get; set; }
        public User   User    { get; set; }
    }
}