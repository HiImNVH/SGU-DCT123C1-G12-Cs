using System.Net.Http.Json;
using TravelGuide.AdminWeb.Models;
using TravelGuide.Core.DTOs;

namespace TravelGuide.AdminWeb.Services;

public interface IAdminAuthService
{
    Task<bool> LoginAsync(string username, string password);
    void Logout();
    AdminSession GetSession();
}

public class AdminAuthService : IAdminAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AdminAuthService> _logger;
    private readonly AdminSession _session;  // ✅ inject thay vi new()

    public AdminAuthService(
        IHttpClientFactory httpClientFactory,
        ILogger<AdminAuthService> logger,
        AdminSession session)              // ✅ thêm AdminSession vào constructor
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _session = session;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        _logger.LogInformation("[info] - Admin dang nhap: username={Username}", username);

        try
        {
            var client = _httpClientFactory.CreateClient("TravelGuideAPI");
            var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
            {
                Username = username,
                Password = password
            });

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[warn] - Dang nhap that bai, status={Status}", response.StatusCode);
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<TokenResult>();
            if (result == null)
            {
                _logger.LogWarning("[warn] - Khong doc duoc token tu response");
                return false;
            }

            if (!string.Equals(result.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("[warn] - User {Username} khong co role Admin", username);
                return false;
            }

            _session.Token = result.Token;
            _session.Username = username;
            _session.Role = result.Role;
            _session.PreferredLanguage = result.PreferredLanguage;
            _session.ExpiresAt = result.ExpiresAt;

            _logger.LogInformation("[info] - Dang nhap thanh cong: username={Username}", username);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[error] - Loi khi dang nhap: {Type} - {Message}",
                ex.GetType().Name, ex.Message);
            return false;
        }
    }

    public void Logout()
    {
        _logger.LogInformation("[info] - Admin dang xuat: username={Username}", _session.Username);
        _session.Token = string.Empty;
        _session.Username = string.Empty;
        _session.Role = string.Empty;
        _session.ExpiresAt = DateTime.MinValue;
    }

    public AdminSession GetSession() => _session;
}