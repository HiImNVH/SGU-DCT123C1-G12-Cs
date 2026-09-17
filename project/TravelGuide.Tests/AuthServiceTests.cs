using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using TravelGuide.API.Repositories;
using TravelGuide.API.Services;
using TravelGuide.Core.Enums;
using TravelGuide.Core.Models;

namespace TravelGuide.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_WhenUsernameExists_ReturnsNull()
    {
        var repository = new FakeUserRepository(new User { Username = "existing-user" });
        var service = CreateService(repository);

        var result = await service.RegisterAsync("existing-user", "password123", "vi");

        Assert.Null(result);
        Assert.Equal(0, repository.AddCalls);
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsWrong_ReturnsNull()
    {
        var user = new User
        {
            Username = "traveller",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct-password")
        };
        var service = CreateService(new FakeUserRepository(user));

        var result = await service.LoginAsync("traveller", "wrong-password");

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_WhenAdminCredentialsAreValid_ReturnsAdminToken()
    {
        var user = new User
        {
            Username = "demo-admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct-password"),
            Role = UserRole.Admin
        };
        var service = CreateService(new FakeUserRepository(user));

        var result = await service.LoginAsync("demo-admin", "correct-password");

        Assert.NotNull(result);
        Assert.Equal("Admin", result.Role);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    private static AuthService CreateService(IUserRepository repository)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "unit-test-key-that-is-at-least-32-characters-long",
                ["Jwt:Issuer"] = "TravelGuideTests",
                ["Jwt:Audience"] = "TravelGuideTests"
            })
            .Build();

        return new AuthService(repository, configuration, NullLogger<AuthService>.Instance);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly Dictionary<string, User> _users;

        public FakeUserRepository(params User[] users) =>
            _users = users.ToDictionary(user => user.Username, StringComparer.OrdinalIgnoreCase);

        public int AddCalls { get; private set; }

        public Task<User?> GetByUsernameAsync(string username) =>
            Task.FromResult(_users.GetValueOrDefault(username));

        public Task<User?> GetByIdAsync(Guid id) =>
            Task.FromResult(_users.Values.FirstOrDefault(user => user.Id == id));

        public Task AddAsync(User user)
        {
            AddCalls++;
            _users[user.Username] = user;
            return Task.CompletedTask;
        }

        public Task UpdateLanguageAsync(Guid userId, string langCode)
        {
            var user = _users.Values.FirstOrDefault(item => item.Id == userId);
            if (user is not null) user.PreferredLanguage = langCode;
            return Task.CompletedTask;
        }
    }
}
