using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelGuide.API.Services;
using TravelGuide.Core.Constants;

namespace TravelGuide.API.Controllers;

[ApiController]
[Route("api/poi")]
public class POIController : ControllerBase
{
    private readonly IPOIService _poiService;
    private readonly ILogger<POIController> _logger;

    public POIController(IPOIService poiService, ILogger<POIController> logger)
    {
        _poiService = poiService;
        _logger = logger;
    }

    /// <summary>
    /// PUBLIC - UserApp goi: chi tra ve POI dang hoat dong (IsActive = true)
    /// GET /api/poi
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("[log] - GET /api/poi - chi lay POI active");
        var result = await _poiService.GetAllAsync(active: true);
        return Ok(result);
    }

    /// <summary>
    /// ADMIN - Lay tat ca POI ke ca da xoa, yeu cau JWT Role=Admin
    /// GET /api/poi/all
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllForAdmin()
    {
        _logger.LogInformation("[log] - GET /api/poi/all - Admin lay tat ca POI");
        var result = await _poiService.GetAllAsync(active: null);
        return Ok(result);
    }

    /// <summary>
    /// Lay chi tiet mot POI kem noi dung theo ngon ngu
    /// GET /api/poi/{id}?lang=vi
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromQuery] string lang = LanguageConstants.Default)
    {
        _logger.LogInformation("[log] - GET /api/poi/{Id}?lang={Lang}", id, lang);

        var result = await _poiService.GetByIdAsync(id, lang);
        if (result == null)
        {
            _logger.LogWarning("[warn] - POI id={Id} khong ton tai", id);
            return NotFound(new { error = "POI not found" });
        }

        return Ok(result);
    }
}