using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using TravelGuide.API.Services;
using TravelGuide.Core.DTOs;

namespace TravelGuide.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Dang nhap bang username/password, nhan JWT token
    /// POST /api/auth/login
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] TravelGuide.Core.DTOs.LoginRequest request)
    {
        _logger.LogInformation("[log] - POST /api/auth/login - username={Username}", request.Username);

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("[warn] - Thieu username hoac password");
            return BadRequest(new { error = "Username va password khong duoc de trong" });
        }

        var result = await _authService.LoginAsync(request.Username, request.Password);
        if (result == null)
        {
            _logger.LogWarning("[warn] - Dang nhap that bai cho username={Username}", request.Username);
            return Unauthorized(new { error = "Invalid credentials" });
        }

        _logger.LogInformation("[info] - Dang nhap thanh cong username={Username}", request.Username);
        return Ok(result);
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] TravelGuide.Core.DTOs.RegisterRequest request)
    {
        _logger.LogInformation("[log] - POST /api/auth/register - username={Username}", request.Username);

        // Validate
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("[warn] - Thieu username hoac password khi dang ky");
            return BadRequest(new { error = "Username va password khong duoc de trong" });
        }

        if (request.Password.Length < 6)
        {
            _logger.LogWarning("[warn] - Password qua ngan");
            return BadRequest(new { error = "Password phai co it nhat 6 ky tu" });
        }

        var result = await _authService.RegisterAsync(
            request.Username,
            request.Password,
            request.PreferredLanguage ?? "vi");

        if (result == null)
        {
            _logger.LogWarning("[warn] - Dang ky that bai, username da ton tai: {Username}", request.Username);
            return BadRequest(new { error = "Username da duoc su dung" });
        }

        _logger.LogInformation("[info] - Dang ky thanh cong username={Username}", request.Username);
        return Ok(result);
    }
}