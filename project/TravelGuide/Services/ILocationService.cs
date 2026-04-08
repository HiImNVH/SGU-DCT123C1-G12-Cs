using TravelGuide.Models;

namespace TravelGuide.Services
{
    public class POIEventArgs : EventArgs
    {
        public POI POI { get; set; }
    }

    public interface ILocationService
    {
        Task<Location> GetCurrentLocationAsync();
        Task StartMonitoringAsync(IEnumerable<POI> pois);
        Task StopMonitoringAsync();
        event EventHandler<POIEventArgs> OnEnteredRegion;
        event EventHandler<POIEventArgs> OnExitedRegion;
    }
}