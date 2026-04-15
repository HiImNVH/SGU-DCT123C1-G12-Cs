// TravelGuide.API/Controllers/DeviceController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelGuide.API.Services;
using TravelGuide.Core.DTOs;

namespace TravelGuide.API.Controllers;

[ApiController]
[Route("api/device")]
public class DeviceController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly ILogger<DeviceController> _logger;

    public DeviceController(IDeviceService deviceService, ILogger<DeviceController> logger)
    {
        _deviceService = deviceService;
        _logger        = logger;
    }

    /// <summary>
    /// App gửi ping mỗi khi mở → ghi nhận thiết bị đang hoạt động.
    /// Không cần auth — Guest cũng ping được.
    /// POST /api/device/ping
    /// </summary>
    [HttpPost("ping")]
    public async Task<IActionResult> Ping([FromBody] DevicePingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceId))
            return BadRequest(new { error = "DeviceId không được để trống" });

        _logger.LogInformation("[log] - POST /api/device/ping - deviceId={DeviceId}", request.DeviceId);
        await _deviceService.PingAsync(request);
        return Ok(new { message = "ok" });
    }

    /// <summary>
    /// App gửi sau mỗi lần scan QR thành công → tăng ScanCount.
    /// Không cần auth.
    /// POST /api/device/scan
    /// </summary>
    [HttpPost("scan")]
    public async Task<IActionResult> RecordScan([FromBody] DeviceScanRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceId))
            return BadRequest(new { error = "DeviceId không được để trống" });

        _logger.LogInformation("[log] - POST /api/device/scan - deviceId={DeviceId}", request.DeviceId);
        await _deviceService.RecordScanAsync(request.DeviceId);
        return Ok(new { message = "ok" });
    }

    /// <summary>
    /// Admin xem thống kê thiết bị.
    /// GET /api/device/stats  (yêu cầu JWT Admin)
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetStats()
    {
        _logger.LogInformation("[log] - GET /api/device/stats");
        var stats = await _deviceService.GetStatsAsync();
        return Ok(stats);
    }
}
