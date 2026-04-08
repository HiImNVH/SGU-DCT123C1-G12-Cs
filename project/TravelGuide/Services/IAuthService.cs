using TravelGuide.Models;
using TravelGuide.Models.DTOs;

namespace TravelGuide.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<bool> UpdateLanguageAsync(Guid userId, string languageCode);
        Task LogoutAsync();
        User GetCurrentUser();
        bool IsLoggedIn();
    }
}