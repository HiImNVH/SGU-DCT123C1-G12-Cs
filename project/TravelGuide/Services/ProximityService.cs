// Services/ProximityService.cs
using System.ComponentModel;
using TravelGuide.Models.DTOs;

namespace TravelGuide.Services
{
    /// <summary>
    /// ProximityService — Theo dõi vị trí người dùng, phát hiện POI gần và gửi notification.
    ///
    /// Logic:
    ///   • Khoảng cách ≤ AUTO_PLAY_RADIUS (15m)  → tự động phát TTS ngay lập tức
    ///   • Khoảng cách ≤ NOTIFY_RADIUS    (40m)  → hiển thị banner hỏi có muốn nghe không
    ///   • Nếu 2 POI cùng trong vùng → ưu tiên POI gần nhất
    ///   • Mỗi POI chỉ trigger 1 lần cho đến khi người dùng đi xa > EXIT_RADIUS (80m)
    /// </summary>
    public class ProximityService : IDisposable
    {
        // ── Ngưỡng khoảng cách ───────────────────────────────────────
        private const double AUTO_PLAY_RADIUS = 10.0;  // mét — tự phát luôn
        private const double NOTIFY_RADIUS    = 30.0;  // mét — hiện banner hỏi
        private const double EXIT_RADIUS      = 80.0;  // mét — reset trigger cho POI đó

        // ── Events ───────────────────────────────────────────────────
        /// <summary>Khi vào vùng AUTO_PLAY → phát TTS ngay, không hỏi</summary>
        public event Action<POISummaryDto>? AutoPlayTriggered;

        /// <summary>Khi vào vùng NOTIFY → hiện banner hỏi user</summary>
        public event Action<POISummaryDto>? NearbyPOIDetected;

        /// <summary>Cập nhật vị trí người dùng (để UI hiển thị)</summary>
        public event Action<Location>? LocationUpdated;

        // ── State ────────────────────────────────────────────────────
        private List<POISummaryDto> _allPois     = new();
        private readonly HashSet<Guid> _autoPlayed = new(); // POI đã auto-play, chờ exit
        private readonly HashSet<Guid> _notified   = new(); // POI đã notify, chờ exit
        private CancellationTokenSource? _cts;
        private bool _isRunning;
        private Location? _lastLocation;

        public bool IsRunning => _isRunning;
        public Location? LastLocation => _lastLocation;

        // ── Public API ───────────────────────────────────────────────

        /// <summary>Cập nhật danh sách POI (gọi sau khi load từ API)</summary>
        public void UpdatePOIs(List<POISummaryDto> pois)
        {
            _allPois = pois ?? new();
            Console.WriteLine($"[proximity] - Cap nhat danh sach POI: {_allPois.Count} diem");
        }

        /// <summary>Bắt đầu theo dõi vị trí — gọi khi app vào foreground</summary>
        public async Task StartAsync()
        {
            if (_isRunning) return;

            // Kiểm tra quyền
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    Console.WriteLine("[proximity] - Khong co quyen vi tri");
                    return;
                }
            }

            _isRunning = true;
            _cts       = new CancellationTokenSource();
            Console.WriteLine("[proximity] - Bat dau theo doi vi tri");
            _ = WatchLocationAsync(_cts.Token);
        }

        /// <summary>Dừng theo dõi — gọi khi app vào background hoặc user tắt</summary>
        public void Stop()
        {
            _isRunning = false;
            _cts?.Cancel();
            _cts = null;
            Console.WriteLine("[proximity] - Dung theo doi vi tri");
        }

        /// <summary>Reset trigger cho POI cụ thể (user bấm "Không" hoặc đã nghe xong)</summary>
        public void ResetPOI(Guid poiId)
        {
            _autoPlayed.Remove(poiId);
            _notified.Remove(poiId);
        }

        // ── Core loop ────────────────────────────────────────────────

        private async Task WatchLocationAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var location = await Geolocation.GetLocationAsync(
                        new GeolocationRequest
                        {
                            DesiredAccuracy = GeolocationAccuracy.Best,
                            Timeout         = TimeSpan.FromSeconds(8)
                        }, ct);

                    if (location != null)
                    {
                        _lastLocation = location;
                        LocationUpdated?.Invoke(location);
                        ProcessLocation(location);
                        // Cập nhật 3 giây/lần khi đang di chuyển
                        await Task.Delay(3000, ct);
                    }
                    else
                    {
                        await Task.Delay(5000, ct);
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Console.WriteLine($"[proximity] - Loi GPS: {ex.Message}");
                    await Task.Delay(8000, ct);
                }
            }
        }

        private void ProcessLocation(Location userLoc)
        {
            if (_allPois.Count == 0) return;

            // Tính khoảng cách đến từng POI, sắp xếp gần → xa
            var withDistance = _allPois
                .Select(poi => (poi, dist: GetDistance(userLoc, poi)))
                .OrderBy(x => x.dist)
                .ToList();

            // Reset trigger cho POI mà user đã đi ra xa
            foreach (var (poi, dist) in withDistance)
            {
                if (dist > EXIT_RADIUS)
                {
                    _autoPlayed.Remove(poi.Id);
                    _notified.Remove(poi.Id);
                }
            }

            // Lấy POI gần nhất
            var (nearest, nearestDist) = withDistance.First();

            // Vùng AUTO_PLAY — tự phát ngay, không hỏi
            if (nearestDist <= AUTO_PLAY_RADIUS && !_autoPlayed.Contains(nearest.Id))
            {
                _autoPlayed.Add(nearest.Id);
                _notified.Add(nearest.Id); // tránh notify thêm
                Console.WriteLine($"[proximity] - AUTO PLAY: {nearest.Name} ({nearestDist:F1}m)");
                MainThread.BeginInvokeOnMainThread(() => AutoPlayTriggered?.Invoke(nearest));
                return;
            }

            // Vùng NOTIFY — hiện banner, ưu tiên POI gần nhất chưa notify
            if (nearestDist <= NOTIFY_RADIUS && !_notified.Contains(nearest.Id))
            {
                _notified.Add(nearest.Id);
                Console.WriteLine($"[proximity] - NOTIFY: {nearest.Name} ({nearestDist:F1}m)");
                MainThread.BeginInvokeOnMainThread(() => NearbyPOIDetected?.Invoke(nearest));
            }
        }

        // ── Utils ────────────────────────────────────────────────────

        /// <summary>Haversine formula — khoảng cách giữa 2 toạ độ (mét)</summary>
        public static double GetDistance(Location from, POISummaryDto poi)
        {
            const double R = 6371000; // bán kính Trái Đất (mét)
            var dLat = ToRad(poi.Latitude  - from.Latitude);
            var dLon = ToRad(poi.Longitude - from.Longitude);
            var lat1 = ToRad(from.Latitude);
            var lat2 = ToRad(poi.Latitude);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                  + Math.Cos(lat1) * Math.Cos(lat2)
                  * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private static double ToRad(double deg) => deg * Math.PI / 180;

        public void Dispose() => Stop();
    }
}
