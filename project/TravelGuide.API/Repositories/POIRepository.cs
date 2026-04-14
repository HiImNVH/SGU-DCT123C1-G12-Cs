using Microsoft.EntityFrameworkCore;
using TravelGuide.API.Data;
using TravelGuide.Core.Models;
namespace TravelGuide.API.Repositories;

public interface IPOIRepository
{
    Task<POI?> GetByIdWithContentAsync(Guid id, string lang);
    Task<List<POI>> GetAllActiveAsync();
    Task<POI?> GetByIdAsync(Guid id);
    Task<Guid> AddAsync(POI poi);
    Task UpdateAsync(POI poi);
    Task SoftDeleteAsync(Guid id);
    Task UpsertContentAsync(POIContent content);
	Task<List<POI>> GetAllAsync();
Task<List<POI>> GetAllByActiveStatusAsync(bool isActive);
}

public class POIRepository : IPOIRepository
{
    private readonly AppDbContext _db;

    public POIRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// JOIN POI + POIContent, loc theo ngon ngu. Fallback ve "vi" neu khong co.
    /// </summary>
    public async Task<POI?> GetByIdWithContentAsync(Guid id, string lang)
    {
        var poi = await _db.POIs
            .Include(p => p.Contents)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        return poi;
    }

    /// <summary>
    /// WHERE IsActive = true
    /// </summary>
    public async Task<List<POI>> GetAllActiveAsync()
    {
        return await _db.POIs
            .Where(p => p.IsActive)
            .ToListAsync();
    }

    public async Task<POI?> GetByIdAsync(Guid id)
    {
        return await _db.POIs.FindAsync(id);
    }

    /// <summary>
    /// INSERT POI moi
    /// </summary>
    public async Task<Guid> AddAsync(POI poi)
    {
        _db.POIs.Add(poi);
        await _db.SaveChangesAsync();
        return poi.Id;
    }

    /// <summary>
    /// UPDATE thong tin POI
    /// </summary>
    public async Task UpdateAsync(POI poi)
    {
        _db.POIs.Update(poi);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// UPDATE SET IsActive = false
    /// </summary>
    public async Task SoftDeleteAsync(Guid id)
    {
        var poi = await _db.POIs.FindAsync(id);
        if (poi != null)
        {
            poi.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// INSERT hoac UPDATE POIContent (upsert theo POIId + LanguageCode)
    /// </summary>
    public async Task UpsertContentAsync(POIContent content)
    {
        var existing = await _db.POIContents
            .FirstOrDefaultAsync(c => c.POIId == content.POIId && c.LanguageCode == content.LanguageCode);

        if (existing == null)
        {
            _db.POIContents.Add(content);
        }
        else
        {
            existing.NarrationText = content.NarrationText;
            existing.AudioUrl = content.AudioUrl;
        }

        await _db.SaveChangesAsync();
    }
/// <summary>
/// Lay tat ca POI khong phan biet IsActive
/// </summary>
public async Task<List<POI>> GetAllAsync()
{
    return await _db.POIs.ToListAsync();
}

/// <summary>
/// Lay POI theo trang thai IsActive
/// </summary>
public async Task<List<POI>> GetAllByActiveStatusAsync(bool isActive)
{
    return await _db.POIs
        .Where(p => p.IsActive == isActive)
        .ToListAsync();
}
}