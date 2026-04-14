// ── 2. File Services/AuthService.cs đầy đủ ──────────────────────
using System.Net.Http.Json;
using System.Text.Json;
using TravelGuide.Constants;
using TravelGuide.Models;
using TravelGuide.Models.DTOs;

namespace TravelGuide.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private User? _currentUser;

        private readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AuthService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            };
            _http = new HttpClient(handler)
            {
                BaseAddress = new Uri(AppConstants.BaseUrl),
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        // ── Login ────────────────────────────────────────────────────────
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            Console.WriteLine($"[log] - Bat dau dang nhap: {request.Username}");
            try
            {
                var response = await _http.PostAsJsonAsync(AppConstants.ApiAuth, request);
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[log] - Login response: {(int)response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var tokenResult = JsonSerializer.Deserialize<TokenResult>(json, _jsonOpts);
                    if (tokenResult == null || string.IsNullOrEmpty(tokenResult.Token))
                        return Fail("Đăng nhập thất bại");

                    SetCurrentUser(request.Username, tokenResult);
                    await SecureStorage.SetAsync(AppConstants.PrefKeyAuthToken, tokenResult.Token);
                    Console.WriteLine($"[info] - Dang nhap thanh cong: role={tokenResult.Role}");
                    return new AuthResponse { Success = true, Token = tokenResult.Token, User = _currentUser };
                }

                Console.WriteLine($"[error] - Dang nhap that bai HTTP {(int)response.StatusCode}");
                return Fail("Sai tên đăng nhập hoặc mật khẩu");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[error] - Loi ket noi: {ex.Message}");
                return Fail("Không thể kết nối đến server. Kiểm tra API đang chạy chưa.");
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("[error] - Timeout khi dang nhap");
                return Fail("Kết nối timeout. Thử lại.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[error] - Loi khong xac dinh: {ex.Message}");
                return Fail($"Lỗi: {ex.Message}");
            }
        }

        // ── Register ─────────────────────────────────────────────────────
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            Console.WriteLine($"[log] - Bat dau dang ky: {request.Username}");
            try
            {
                var response = await _http.PostAsJsonAsync(AppConstants.ApiRegister, request);
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[log] - Register response: {(int)response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var tokenResult = JsonSerializer.Deserialize<TokenResult>(json, _jsonOpts);
                    if (tokenResult == null || string.IsNullOrEmpty(tokenResult.Token))
                        return Fail("Tạo tài khoản thất bại");

                    SetCurrentUser(request.Username, tokenResult);
                    await SecureStorage.SetAsync(AppConstants.PrefKeyAuthToken, tokenResult.Token);
                    Console.WriteLine($"[info] - Dang ky thanh cong: {request.Username}");
                    return new AuthResponse { Success = true, Token = tokenResult.Token, User = _currentUser };
                }

                // Đọc error message từ API
                string errorMsg = "Tạo tài khoản thất bại";
                try
                {
                    var err = JsonSerializer.Deserialize<Dictionary<string, string>>(json, _jsonOpts);
                    if (err?.TryGetValue("error", out var msg) == true && !string.IsNullOrEmpty(msg))
                        errorMsg = msg == "Username da duoc su dung"
                            ? "Tên đăng nhập đã được sử dụng"
                            : msg;
                }
                catch { }

                Console.WriteLine($"[error] - Dang ky that bai: {errorMsg}");
                return Fail(errorMsg);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[error] - Loi ket noi: {ex.Message}");
                return Fail("Không thể kết nối đến server.");
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("[error] - Timeout khi dang ky");
                return Fail("Kết nối timeout. Thử lại.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[error] - Loi: {ex.Message}");
                return Fail($"Lỗi: {ex.Message}");
            }
        }

        // ── Logout ───────────────────────────────────────────────────────
        public async Task LogoutAsync()
        {
            Console.WriteLine("[log] - Dang xuat");
            _currentUser = null;
            SecureStorage.Remove(AppConstants.PrefKeyAuthToken);
            await Task.CompletedTask;
        }

        public bool IsAuthenticated()
        {
            var token = SecureStorage.GetAsync(AppConstants.PrefKeyAuthToken).Result;
            return !string.IsNullOrEmpty(token);
        }

        public User? GetCurrentUser() => _currentUser;
        public bool IsAdmin() => _currentUser?.Role == UserRole.Admin;

        public string GetCurrentLanguage() =>
            _currentUser?.PreferredLanguage
                ?? Preferences.Get(AppConstants.PrefKeyLanguage, AppConstants.DefaultLanguage);

        public async Task<bool> UpdateLanguageAsync(string languageCode)
        {
            try
            {
                var token = await SecureStorage.GetAsync(AppConstants.PrefKeyAuthToken);
                if (string.IsNullOrEmpty(token)) return false;

                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _http.PutAsJsonAsync(
                    AppConstants.ApiUserLanguage,
                    new UpdateLanguageRequest { LanguageCode = languageCode });

                if (response.IsSuccessStatusCode)
                {
                    if (_currentUser != null) _currentUser.PreferredLanguage = languageCode;
                    Console.WriteLine($"[info] - Cap nhat ngon ngu server: {languageCode}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[error] - Loi cap nhat ngon ngu: {ex.Message}");
                return false;
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────
        private void SetCurrentUser(string username, TokenResult tokenResult)
        {
            _currentUser = new User
            {
                Username = username,
                PreferredLanguage = tokenResult.PreferredLanguage ?? AppConstants.DefaultLanguage,
                Role = Enum.TryParse<UserRole>(tokenResult.Role, true, out var role)
                                    ? role : UserRole.User
            };
        }

        private static AuthResponse Fail(string message) =>
            new() { Success = false, ErrorMessage = message };
    }
}