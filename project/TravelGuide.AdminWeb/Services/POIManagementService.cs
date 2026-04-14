using System.Net.Http.Headers;
using System.Net.Http.Json;
using TravelGuide.Core.DTOs;

namespace TravelGuide.AdminWeb.Services;

public interface IPOIManagementService
{
    Task<List<POISummaryDto>> GetAllAsync(string token);
    Task<POIDetailDto?> GetByIdAsync(Guid id, string token);
    Task<Guid?> CreateAsync(CreatePOIDto dto, string token);
    Task<bool> UpdateAsync(Guid id, UpdatePOIDto dto, string token);
    Task<bool> DeleteAsync(Guid id, string token);
}

/// <summary>
/// POIManagementModule: CRUD POI thong qua API
/// </summary>
public class POIManagementService : IPOIManagementService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<POIManagementService> _logger;

    public POIManagementService(IHttpClientFactory httpClientFactory, ILogger<POIManagementService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private HttpClient CreateClient(string token)
    {
        var client = _httpClientFactory.CreateClient("TravelGuideAPI");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>
    /// Lay danh sach tat ca POI
    /// </summary>
    public async Task<List<POISummaryDto>> GetAllAsync(string token)
{
    _logger.LogInformation("[info] - Lay danh sach tat ca POI (Admin)");
    try
    {
        var client = CreateClient(token);
        // ✅ Gọi endpoint Admin riêng → trả cả POI đã xóa
        var result = await client.GetFromJsonAsync<List<POISummaryDto>>("/api/poi/all");
        _logger.LogInformation("[log] - Da lay {Count} POI", result?.Count ?? 0);
        return result ?? new List<POISummaryDto>();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "[error] - Loi khi lay danh sach POI: {Message}", ex.Message);
        return new List<POISummaryDto>();
    }
}

    /// <summary>
    /// Lay chi tiet mot POI
    /// </summary>
    public async Task<POIDetailDto?> GetByIdAsync(Guid id, string token)
    {
        _logger.LogInformation("[info] - Lay chi tiet POI id={Id}", id);
        try
        {
            var client = CreateClient(token);
            return await client.GetFromJsonAsync<POIDetailDto>($"/api/admin/poi/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[error] - Loi khi lay POI id={Id}: {Message}", id, ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Tao POI moi, tra ve Guid neu thanh cong
    /// </summary>
    public async Task<Guid?> CreateAsync(CreatePOIDto dto, string token)
    {
        _logger.LogInformation("[info] - Tao POI moi: name={Name}", dto.Name);
        try
        {
            var client = CreateClient(token);
            var response = await client.PostAsJsonAsync("/api/admin/poi", dto);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[warn] - Tao POI that bai, status={Status}", response.StatusCode);
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
            var newId = result?["id"];
            _logger.LogInformation("[info] - Da tao POI moi id={Id}", newId);
            return newId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[error] - Loi khi tao POI: {Message}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Cap nhat POI
    /// </summary>
    public async Task<bool> UpdateAsync(Guid id, UpdatePOIDto dto, string token)
    {
        _logger.LogInformation("[info] - Cap nhat POI id={Id}", id);
        try
        {
            var client = CreateClient(token);
            var response = await client.PutAsJsonAsync($"/api/admin/poi/{id}", dto);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[warn] - Cap nhat POI that bai, status={Status}", response.StatusCode);
                return false;
            }
            _logger.LogInformation("[info] - Da cap nhat POI id={Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[error] - Loi khi cap nhat POI id={Id}: {Message}", id, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Xoa mem POI (IsActive = false)
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id, string token)
    {
        _logger.LogInformation("[info] - Xoa POI id={Id}", id);
        try
        {
            var client = CreateClient(token);
            var response = await client.DeleteAsync($"/api/admin/poi/{id}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[warn] - Xoa POI that bai, status={Status}", response.StatusCode);
                return false;
            }
            _logger.LogInformation("[info] - Da xoa POI id={Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[error] - Loi khi xoa POI id={Id}: {Message}", id, ex.Message);
            return false;
        }
    }
}
