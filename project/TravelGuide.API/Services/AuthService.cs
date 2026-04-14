using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TravelGuide.API.Repositories;
using TravelGuide.Core.Constants;
using TravelGuide.Core.DTOs;
using TravelGuide.Core.Enums;

namespace TravelGuide.API.Services;

public interface IAuthService
{
    Task<TokenResult?> LoginAsync(string username, string password);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Goi API dang nhap: xac thuc username/password, tao JWT token
    /// </summary>
    public async Task<TokenResult?> LoginAsync(string username, string password)
    {
        _logger.LogInformation("[info] - Bat dau dang nhap cho user={Username}", username);

        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null)
        {
            _logger.LogWarning("[warn] - Khong tim thay user={Username}", username);
            return null;
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!isPasswordValid)
        {
            _logger.LogWarning("[warn] - Mat khau sai cho user={Username}", username);
            return null;
        }

        var token = GenerateJwtToken(user.Id, user.Username, user.Role);
        var expiresAt = DateTime.UtcNow.AddDays(AppConstants.JwtExpiryDays);

        _logger.LogInformation("[info] - Dang nhap thanh cong cho user={Username}, role={Role}", username, user.Role);

        return new TokenResult
        {
            Token = token,
            ExpiresAt = expiresAt,
            PreferredLanguage = user.PreferredLanguage,
            Role = user.Role.ToString()
        };
    }

    private string GenerateJwtToken(Guid userId, string username, UserRole role)
    {
        _logger.LogInformation("[log] - Tao JWT token cho userId={UserId}", userId);

        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key chua duoc cau hinh");
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "TravelGuide";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "TravelGuide";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(AppConstants.JwtExpiryDays),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}