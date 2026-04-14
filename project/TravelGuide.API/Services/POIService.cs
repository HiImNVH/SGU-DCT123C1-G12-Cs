using TravelGuide.Core.Models;
using TravelGuide.API.Repositories;
using TravelGuide.Core.Constants;
using TravelGuide.Core.DTOs;

namespace TravelGuide.API.Services;

public interface IPOIService
{
    Task<POIDetailDto?> GetByIdAsync(Guid id, string lang);
    Task<List<POISummaryDto>> GetAllAsync(bool? active = null); // ✅ đổi từ GetAllActiveAsync()
    Task<Guid> CreateAsync(CreatePOIDto dto);
    Task<bool> UpdateAsync(Guid id, UpdatePOIDto dto);
    Task<bool> DeactivateAsync(Guid id);
    Task<bool> UpsertContentAsync(Guid id, POIContentDto dto);
}

public class POIService : IPOIService
{
    private readonly IPOIRepository _poiRepository;
    private readonly ILogger<POIService> _logger;

    public POIService(IPOIRepository poiRepository, ILogger<POIService> logger)
    {
        _poiRepository = poiRepository;
        _logger = logger;
    }

    /// <summary>
    /// Lay POI + noi dung theo ngon ngu, tu fallback ve "vi" neu khong co noi dung ngon ngu yeu cau
    /// </summary>
    public async Task<POIDetailDto?> GetByIdAsync(Guid id, string lang)
    {
        _logger.LogInformation("[info] - Bat dau lay POI id={Id} lang={Lang}", id, lang);

        var poi = await _poiRepository.GetByIdWithContentAsync(id, lang);
        if (poi == null)
        {
            _logger.LogWarning("[warn] - Khong tim thay POI id={Id}", id);
            return null;
        }

        // --- POIContentModule: load content theo ngon ngu, fallback ve "vi" ---
        var content = poi.Contents.FirstOrDefault(c => c.LanguageCode == lang)
                   ?? poi.Contents.FirstOrDefault(c => c.LanguageCode == LanguageConstants.Default);

        if (content == null)
        {
            _logger.LogWarning("[warn] - Khong co noi dung cho POI id={Id}, lang={Lang} va ca fallback vi", id, lang);
        }
        else
        {
            _logger.LogInformation("[info] - Da tai noi dung POI id={Id}, languageCode={Lang}", id, content.LanguageCode);
        }

        return new POIDetailDto
        {
            Id = poi.Id,
            Name = poi.Name,
            Category = poi.Category,
            ImageUrl = poi.ImageUrl,
            Latitude = poi.Latitude,
            Longitude = poi.Longitude,
            Content = content == null ? null : new POIContentDto
            {
                LanguageCode = content.LanguageCode,
                NarrationText = content.NarrationText,
                AudioUrl = content.AudioUrl
            }
        };
    }

    /// <summary>
    /// Lay danh sach POI dang hoat dong (IsActive = true)
    /// </summary>
    public async Task<List<POISummaryDto>> GetAllAsync(bool? active = null)
    {
        _logger.LogInformation("[info] - Lay danh sach POI, active={Active}", active);

        // ✅ Lọc theo active nếu có, lấy tất cả nếu không truyền
        var pois = active.HasValue
            ? await _poiRepository.GetAllByActiveStatusAsync(active.Value)
            : await _poiRepository.GetAllAsync();

        _logger.LogInformation("[log] - Tim thay {Count} POI", pois.Count);

        return pois.Select(p => new POISummaryDto
        {
            Id = p.Id,
            Name = p.Name,
            Category = p.Category,
            ImageUrl = p.ImageUrl,
            Latitude = p.Latitude,
            Longitude = p.Longitude,
            IsActive = p.IsActive
        }).ToList();
    }

    /// <summary>
    /// Tao POI moi, tra ve Guid moi tao
    /// </summary>
    public async Task<Guid> CreateAsync(CreatePOIDto dto)
    {
        _logger.LogInformation("[info] - Bat dau tao POI moi: name={Name}", dto.Name);

        var poi = new POI
        {
            Name = dto.Name,
            Category = dto.Category,
            ImageUrl = dto.ImageUrl,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            IsActive = dto.IsActive
        };

        var newId = await _poiRepository.AddAsync(poi);
        _logger.LogInformation("[info] - Da tao POI moi id={Id}", newId);

        return newId;
    }

    /// <summary>
    /// Cap nhat thong tin POI
    /// </summary>
    public async Task<bool> UpdateAsync(Guid id, UpdatePOIDto dto)
    {
        _logger.LogInformation("[info] - Cap nhat POI id={Id}", id);

        var poi = await _poiRepository.GetByIdAsync(id);
        if (poi == null)
        {
            _logger.LogWarning("[warn] - Khong tim thay POI id={Id} de cap nhat", id);
            return false;
        }

        poi.Name = dto.Name;
        poi.Category = dto.Category;
        poi.ImageUrl = dto.ImageUrl;
        poi.Latitude = dto.Latitude;
        poi.Longitude = dto.Longitude;
        poi.IsActive = dto.IsActive;

        await _poiRepository.UpdateAsync(poi);
        _logger.LogInformation("[info] - Da cap nhat POI id={Id}", id);

        return true;
    }

    /// <summary>
    /// Xoa mem POI (IsActive = false)
    /// </summary>
    public async Task<bool> DeactivateAsync(Guid id)
    {
        _logger.LogInformation("[info] - Xoa mem POI id={Id}", id);

        var poi = await _poiRepository.GetByIdAsync(id);
        if (poi == null)
        {
            _logger.LogWarning("[warn] - Khong tim thay POI id={Id} de xoa", id);
            return false;
        }

        await _poiRepository.SoftDeleteAsync(id);
        _logger.LogInformation("[info] - Da xoa mem POI id={Id}", id);

        return true;
    }

    /// <summary>
    /// Them hoac cap nhat noi dung ngon ngu cho POI
    /// </summary>
    public async Task<bool> UpsertContentAsync(Guid id, POIContentDto dto)
    {
        _logger.LogInformation("[info] - Upsert content POI id={Id}, lang={Lang}", id, dto.LanguageCode);

        var poi = await _poiRepository.GetByIdAsync(id);
        if (poi == null)
        {
            _logger.LogWarning("[warn] - Khong tim thay POI id={Id} de upsert content", id);
            return false;
        }

        var content = new POIContent
        {
            POIId = id,
            LanguageCode = dto.LanguageCode,
            NarrationText = dto.NarrationText,
            AudioUrl = dto.AudioUrl
        };

        await _poiRepository.UpsertContentAsync(content);
        _logger.LogInformation("[info] - Da luu content POI id={Id}, lang={Lang}", id, dto.LanguageCode);

        return true;
    }
}