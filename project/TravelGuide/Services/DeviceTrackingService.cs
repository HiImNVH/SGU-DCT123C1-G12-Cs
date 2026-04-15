// TravelGuide/Services/DeviceTrackingService.cs
// Thêm vào MAUI app để tự động gửi ping và scan event lên API

using System.Net.Http.Json;
using TravelGuide.Constants;
using TravelGuide.Models.DTOs;

namespace TravelGuide.Services
{
    /// <summary>
    /// Gửi thông tin thiết bị lên API để Admin theo dõi.
    /// - Ping: gọi mỗi khi app mở (OnAppearing của Shell)
    /// - Scan: gọi sau mỗi lần scan QR thành công
    /// </summary>
    public class DeviceTrackingService
    {
        private readonly HttpClient _http;
        private readonly AuthService _auth;

        // DeviceId cố định cho mỗi thiết bị — lưu trong Preferences
        private static string DeviceId
        {
            get
            {
                var id = Preferences.Get("device_id", "");
                if (string.IsNullOrEmpty(id))
                {
                    id = Guid.NewGuid().ToString();
                    Preferences.Set("device_id", id);
                }
                return id;
            }
        }

        public DeviceTrackingService(AuthService auth)
        {
            _auth = auth;
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            };
            _http = new HttpClient(handler)
            {
                BaseAddress = new Uri(AppConstants.BaseUrl),
                Timeout     = TimeSpan.FromSeconds(5) // không chặn UI nếu API chậm
            };
        }

        /// <summary>
        /// Gửi ping lên API — gọi trong App.xaml.cs OnStart hoặc Shell OnAppearing.
        /// Chạy fire-and-forget, không ảnh hưởng UI.
        /// </summary>
        public void PingAsync()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var request = new DevicePingRequest
                    {
                        DeviceId     = DeviceId,
                        Platform     = DeviceInfo.Platform.ToString(),
                        OsVersion    = DeviceInfo.VersionString,
                        AppVersion   = AppInfo.VersionString,
                        LanguageCode = Preferences.Get("preferred_language", "vi"),
                        Username     = _auth.GetCurrentUser()?.Username
                    };

                    await _http.PostAsJsonAsync("/api/device/ping", request);
                    Console.WriteLine($"[info] - Da gui ping: {DeviceId}");
                }
                catch (Exception ex)
                {
                    // Không quan trọng nếu ping fail — app vẫn chạy bình thường
                    Console.WriteLine($"[warn] - Ping that bai (binh thuong): {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Ghi nhận scan QR thành công — gọi sau khi navigate đến POIDetailPage.
        /// Chạy fire-and-forget.
        /// </summary>
        public void RecordScan()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var request = new { DeviceId };
                    await _http.PostAsJsonAsync("/api/device/scan", request);
                    Console.WriteLine($"[info] - Da ghi scan: {DeviceId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[warn] - Record scan that bai: {ex.Message}");
                }
            });
        }
    }
}
