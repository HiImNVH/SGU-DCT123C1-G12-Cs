using TravelGuide.Models;
using TravelGuide.Models.DTOs;

namespace TravelGuide.Services
{
    /// <summary>
    /// Mock service — thay bằng ApiAuthService khi có backend
    /// </summary>
    public class MockAuthService : IAuthService
    {
        private User _currentUser;

        private readonly List<User> _users = new()
        {
            new User
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                PasswordHash = "admin123",
                FullName = "Quản trị viên",
                Email = "admin@travel.com",
                Role = UserRole.Admin,
                PreferredLanguage = "vi",
                IsActive = true
            },
            new User
            {
                Id = Guid.NewGuid(),
                Username = "user1",
                PasswordHash = "user123",
                FullName = "Nguyễn Văn A",
                Email = "user1@travel.com",
                Role = UserRole.User,
                PreferredLanguage = "vi",
                IsActive = true
            }
        };

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            await Task.Delay(500); // giả lập network

            var user = _users.FirstOrDefault(u =>
                u.Username == request.Username &&
                u.PasswordHash == request.Password);

            if (user == null)
                return new AuthResponse { Success = false, ErrorMessage = "Sai tên đăng nhập hoặc mật khẩu" };

            if (!user.IsActive)
                return new AuthResponse { Success = false, ErrorMessage = "Tài khoản đã bị khóa" };

            _currentUser = user;
            await SecureStorage.SetAsync("auth_token", "mock_jwt_token");
            await SecureStorage.SetAsync("user_id", user.Id.ToString());

            return new AuthResponse { Success = true, Token = "mock_jwt_token", User = user };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            await Task.Delay(500);

            if (_users.Any(u => u.Username == request.Username))
                return new AuthResponse { Success = false, ErrorMessage = "Tên đăng nhập đã tồn tại" };

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = request.Password,
                Email = request.Email,
                FullName = request.FullName,
                PreferredLanguage = request.PreferredLanguage,
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _users.Add(newUser);
            _currentUser = newUser;
            await SecureStorage.SetAsync("auth_token", "mock_jwt_token");

            return new AuthResponse { Success = true, Token = "mock_jwt_token", User = newUser };
        }
        public Task<bool> UpdateLanguageAsync(Guid userId, string languageCode)
        {
            return Task.FromResult(true); // giả lập luôn thành công
        }
        public async Task LogoutAsync()
        {
            _currentUser = null;
            SecureStorage.Remove("auth_token");
            SecureStorage.Remove("user_id");
            await Task.CompletedTask;
        }

        public User GetCurrentUser() => _currentUser;

        public bool IsLoggedIn() => _currentUser != null;
    }
}
