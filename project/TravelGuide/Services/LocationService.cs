using TravelGuide.Models;

namespace TravelGuide.Services
{
    /// <summary>
    /// Geofencing service — TTS-02: tự động phát khi vào vùng POI
    /// </summary>
    public class LocationService : ILocationService
    {
        public event EventHandler<POIEventArgs> OnEnteredRegion;
        public event EventHandler<POIEventArgs> OnExitedRegion;

        private List<POI> _activePOIs = new();
        private HashSet<Guid> _insideRegions = new();
        private bool _isMonitoring;
        private CancellationTokenSource _cts;

        public async Task<Location> GetCurrentLocationAsync()
        {
            try
            {
                var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                    return null;

                return await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(10)
                });
            }
            catch { return null; }
        }

        public async Task StartMonitoringAsync(IEnumerable<POI> pois)
        {
            _activePOIs = pois.ToList();
            _isMonitoring = true;
            _cts = new CancellationTokenSource();

            _ = Task.Run(async () =>
            {
                while (_isMonitoring && !_cts.Token.IsCancellationRequested)
                {
                    var location = await GetCurrentLocationAsync();
                    if (location != null)
                        CheckGeofences(location.Latitude, location.Longitude);

                    await Task.Delay(5000, _cts.Token); // check mỗi 5 giây
                }
            }, _cts.Token);

            await Task.CompletedTask;
        }

        public async Task StopMonitoringAsync()
        {
            _isMonitoring = false;
            _cts?.Cancel();
            await Task.CompletedTask;
        }

        private void CheckGeofences(double lat, double lng)
        {
            foreach (var poi in _activePOIs)
            {
                bool isInside = poi.IsInRange((decimal)lat, (decimal)lng);
                bool wasInside = _insideRegions.Contains(poi.Id);

                if (isInside && !wasInside)
                {
                    _insideRegions.Add(poi.Id);
                    OnEnteredRegion?.Invoke(this, new POIEventArgs { POI = poi });
                }
                else if (!isInside && wasInside)
                {
                    _insideRegions.Remove(poi.Id);
                    OnExitedRegion?.Invoke(this, new POIEventArgs { POI = poi });
                }
            }
        }
    }
}