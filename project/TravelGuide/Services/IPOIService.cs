using TravelGuide.Models;

namespace TravelGuide.Services
{
    public interface IPOIService
    {
        Task<List<POI>> GetAllAsync();
        Task<List<POI>> GetNearbyAsync(double lat, double lng, int radius = 1000);
        Task<POIContent> GetContentAsync(Guid poiId, string languageCode);
        Task<POI> GetByIdAsync(Guid id);          // ← thêm
        Task<bool> RateAsync(Guid poiId, int rating, string? comment); // ← thêm
    }
}