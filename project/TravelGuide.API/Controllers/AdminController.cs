using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelGuide.API.Services;
using TravelGuide.Core.DTOs;

namespace TravelGuide.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IPOIService _poiService;
    private readonly IQRService _qrService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IPOIService poiService,
        IQRService qrService,
        ILogger<AdminController> logger)
    {
        _poiService = poiService;
        _qrService = qrService;
        _logger = logger;
    }

    // ─────────────── POI CRUD ───────────────

    /// <summary>
    /// Tao POI moi
    /// POST /api/admin/poi
    /// </summary>
    [HttpPost("poi")]
    public async Task<IActionResult> CreatePOI([FromBody] CreatePOIDto dto)
    {
        _logger.LogInformation("[log] - POST /api/admin/poi - name={Name}", dto.Name);

        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Category))
        {
            _logger.LogWarning("[warn] - Thieu name hoac category khi tao POI");
            return BadRequest(new { error = "Name va Category la bat buoc" });
        }

        var id = await _poiService.CreateAsync(dto);
        _logger.LogInformation("[info] - Da tao POI moi id={Id}", id);

        return CreatedAtAction(nameof(GetPOIById), new { id }, new { id });
    }

    /// <summary>
    /// Lay chi tiet POI (Admin - khong can loc IsActive)
    /// GET /api/admin/poi/{id}
    /// </summary>
    [HttpGet("poi/{id:guid}")]
    public async Task<IActionResult> GetPOIById(Guid id)
    {
        _logger.LogInformation("[log] - GET /api/admin/poi/{Id}", id);

        var result = await _poiService.GetByIdAsync(id, "vi");
        if (result == null)
            return NotFound(new { error = "POI not found" });

        return Ok(result);
    }

    /// <summary>
    /// Cap nhat POI
    /// PUT /api/admin/poi/{id}
    /// </summary>
    [HttpPut("poi/{id:guid}")]
    public async Task<IActionResult> UpdatePOI(Guid id, [FromBody] UpdatePOIDto dto)
    {
        _logger.LogInformation("[log] - PUT /api/admin/poi/{Id}", id);

        var success = await _poiService.UpdateAsync(id, dto);
        if (!success)
            return NotFound(new { error = "POI not found" });

        _logger.LogInformation("[info] - Da cap nhat POI id={Id}", id);
        return Ok(new { message = "Updated successfully" });
    }

    /// <summary>
    /// Xoa mem POI (IsActive = false)
    /// DELETE /api/admin/poi/{id}
    /// </summary>
    [HttpDelete("poi/{id:guid}")]
    public async Task<IActionResult> DeletePOI(Guid id)
    {
        _logger.LogInformation("[log] - DELETE /api/admin/poi/{Id}", id);

        var success = await _poiService.DeactivateAsync(id);
        if (!success)
            return NotFound(new { error = "POI not found" });

        _logger.LogInformation("[info] - Da xoa mem POI id={Id}", id);
        return Ok(new { message = "POI deactivated" });
    }

    /// <summary>
    /// Upload noi dung da ngon ngu cho POI
    /// POST /api/admin/poi/{id}/content
    /// </summary>
    [HttpPost("poi/{id:guid}/content")]
    public async Task<IActionResult> UpsertContent(Guid id, [FromBody] POIContentDto dto)
    {
        _logger.LogInformation("[log] - POST /api/admin/poi/{Id}/content - lang={Lang}", id, dto.LanguageCode);

        if (string.IsNullOrWhiteSpace(dto.LanguageCode) || string.IsNullOrWhiteSpace(dto.NarrationText))
        {
            _logger.LogWarning("[warn] - Thieu LanguageCode hoac NarrationText");
            return BadRequest(new { error = "LanguageCode va NarrationText la bat buoc" });
        }

        var success = await _poiService.UpsertContentAsync(id, dto);
        if (!success)
            return NotFound(new { error = "POI not found" });

        _logger.LogInformation("[info] - Da luu content POI id={Id}, lang={Lang}", id, dto.LanguageCode);
        return Ok(new { message = "Content saved" });
    }

    // ─────────────── QR ───────────────

    /// <summary>
    /// Sinh QR Code cho POI, tra ve Base64 PNG
    /// GET /api/admin/poi/{id}/qr
    /// </summary>
    [HttpGet("poi/{id:guid}/qr")]
    public async Task<IActionResult> GetQR(Guid id)
    {
        _logger.LogInformation("[log] - GET /api/admin/poi/{Id}/qr", id);

        try
        {
            var base64 = await _qrService.GetBase64QRAsync(id);
            _logger.LogInformation("[info] - Da tao QR cho POI id={Id}", id);

            return Ok(new
            {
                poiId = id,
                qrImageBase64 = base64,
                format = "PNG"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[error] - Loi khi tao QR cho POI id={Id}: {Message}", id, ex.Message);
            return StatusCode(500, new { error = "Loi khi tao QR code" });
        }
    }
}