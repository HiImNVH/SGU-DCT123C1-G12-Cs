// TravelGuide.AdminWeb/Services/DeviceManagementService.cs
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TravelGuide.Core.DTOs;

namespace TravelGuide.AdminWeb.Services;

public interface IDeviceManagementService
{
    Task<DeviceStatsDto?> GetStatsAsync(string token);
}

public class DeviceManagementService : IDeviceManagementService
{
    private readonly IHttpClientFactory _factory;
    private readonly ILogger<DeviceManagementService> _logger;

    public DeviceManagementService(IHttpClientFactory factory, ILogger<DeviceManagementService> logger)
    {
        _factory = factory;
        _logger  = logger;
    }

    public async Task<DeviceStatsDto?> GetStatsAsync(string token)
    {
        try
        {
            var client = _factory.CreateClient("TravelGuideAPI");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var result = await client.GetFromJsonAsync<DeviceStatsDto>("/api/device/stats");
            _logger.LogInformation("[info] - Lay thong ke thiet bi thanh cong: {Total} thiet bi", result?.TotalDevices ?? 0);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[error] - Loi lay thong ke thiet bi");
            return null;
        }
    }
}
