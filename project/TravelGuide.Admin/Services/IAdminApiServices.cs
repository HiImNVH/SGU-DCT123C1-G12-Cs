using TravelGuide.Admin.Models;

namespace TravelGuide.Admin.Services
{
    public interface IAdminApiService
    {
        // Authentication
        Task InitializeAsync();
        Task<bool> LoginAsync(string username, string password);
        Task Logout();
        bool IsLoggedIn();
        
        // POI Management
        Task<List<POIDto>> GetPOIsAsync();
        Task<POIDto?> GetPOIAsync(Guid id); // ✅ Đảm bảo có dấu ?
        Task<bool> CreatePOIAsync(CreatePOIRequest request);
        Task<bool> UpdatePOIAsync(Guid id, CreatePOIRequest request);
        Task<bool> DeletePOIAsync(Guid id);
        
        // User Management
        Task<List<UserDto>> GetUsersAsync();
        Task<bool> ToggleUserActiveAsync(Guid id);
    }
}