// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TravelGuide.API.Data;
using TravelGuide.API.Models.Entities;

namespace TravelGuide.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { success = false, message = "Vui lòng nhập đầy đủ thông tin" });

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
                return Unauthorized(new { success = false, message = "Sai tên đăng nhập hoặc mật khẩu" });

            // TODO: sau này dùng BCrypt, hiện tại so sánh thẳng
            if (user.PasswordHash != request.Password)
                return Unauthorized(new { success = false, message = "Sai tên đăng nhập hoặc mật khẩu" });

            if (!user.IsActive)
                return Unauthorized(new { success = false, message = "Tài khoản đã bị khóa" });

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                success = true,
                token,
                user = new
                {
                    user.Id,
                    user.Username,
                    user.FullName,
                    user.Email,
                    user.Role,
                    user.PreferredLanguage,
                    user.AvatarUrl
                }
            });
        }

        // POST /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { success = false, message = "Vui lòng nhập đầy đủ thông tin" });

            if (await _db.Users.AnyAsync(u => u.Username == request.Username))
                return Conflict(new { success = false, message = "Tên đăng nhập đã tồn tại" });

            var newUser = new User
            {
                Id               = Guid.NewGuid(),
                Username         = request.Username,
                PasswordHash     = request.Password, // TODO: hash sau
                Email            = request.Email ?? "",
                FullName         = request.FullName ?? "",
                AvatarUrl        = "",
                PreferredLanguage = request.PreferredLanguage ?? "vi",
                Role             = UserRole.User,
                IsActive         = true,
                CreatedAt        = DateTime.UtcNow
            };

            _db.Users.Add(newUser);
            await _db.SaveChangesAsync();

            var token = GenerateJwtToken(newUser);

            return Ok(new
            {
                success = true,
                token,
                user = new
                {
                    newUser.Id,
                    newUser.Username,
                    newUser.FullName,
                    newUser.Email,
                    newUser.Role,
                    newUser.PreferredLanguage
                }
            });
        }

        private string GenerateJwtToken(User user)
        {
            var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Secret"]));
            var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddHours(
                              double.Parse(_config["JwtSettings:ExpirationHours"] ?? "24"));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name,           user.Username),
                new Claim(ClaimTypes.Role,           user.Role.ToString()),
                new Claim("language",                user.PreferredLanguage)
            };

            var token = new JwtSecurityToken(
                issuer:             _config["JwtSettings:Issuer"],
                audience:           _config["JwtSettings:Audience"],
                claims:             claims,
                expires:            expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // Request models
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        public string Username         { get; set; }
        public string Password         { get; set; }
        public string Email            { get; set; }
        public string FullName         { get; set; }
        public string PreferredLanguage { get; set; } = "vi";
    }
}