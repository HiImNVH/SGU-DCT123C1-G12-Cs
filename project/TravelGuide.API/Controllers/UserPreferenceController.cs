using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelGuide.API.Services;
using TravelGuide.Core.DTOs;

namespace TravelGuide.API.Controllers;

[ApiController]
[Route("api/user/preference")]
[Authorize]
public class UserPreferenceController : ControllerBase
{
    private readonly IUserPreferenceService _preferenceService;
    private readonly ILogger<UserPreferenceController> _logger;

    public UserPreferenceController(
        IUserPreferenceService preferenceService,
        ILogger<UserPreferenceController> logger)
    {
        _preferenceService = preferenceService;
        _logger = logger;
    }

    /// <summary>
    /// Cap nhat ngon ngu ua thich cua nguoi dung
    /// PUT /api/user/preference/language
    /// </summary>
    [HttpPut("language")]
    public async Task<IActionResult> UpdateLanguage([FromBody] UpdateLanguageRequest request)
    {
        _logger.LogInformation("[log] - PUT /api/user/preference/language - lang={Lang}", request.LanguageCode);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("[warn] - Khong lay duoc userId tu token");
            return Unauthorized(new { error = "Token khong hop le" });
        }

        if (string.IsNullOrWhiteSpace(request.LanguageCode))
        {
            _logger.LogWarning("[warn] - LanguageCode trong request bi trong");
            return BadRequest(new { error = "LanguageCode la bat buoc" });
        }

        var success = await _preferenceService.UpdateLanguageAsync(userId, request.LanguageCode);
        if (!success)
            return BadRequest(new { error = "Ngon ngu khong hop le hoac khong tim thay nguoi dung" });

        _logger.LogInformation("[info] - Da cap nhat ngon ngu userId={UserId} thanh {Lang}", userId, request.LanguageCode);
        return Ok(new { message = "Language preference updated" });
    }
}