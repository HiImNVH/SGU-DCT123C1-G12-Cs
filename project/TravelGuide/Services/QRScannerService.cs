// Services/QRScannerService.cs
using ZXing.Net.Maui;

namespace TravelGuide.Services
{
    /// <summary>
    /// QRScannerModule - Scan QR, trả về poiId, handle lỗi QR invalid
    /// Thư viện: ZXing.Net.MAUI
    /// </summary>
    public class QRScannerService
    {
        /// <summary>
        /// Giải mã chuỗi QR → poiId; trả về null nếu không hợp lệ
        /// </summary>
        public Guid? DecodePoiId(string raw)
        {
            Console.WriteLine("[log] - Bat dau giai ma QR");

            if (string.IsNullOrWhiteSpace(raw))
            {
                Console.WriteLine("[error] - QR khong hop le: chuoi rong");
                return null;
            }

            // QR chứa poiId dạng GUID string
            if (Guid.TryParse(raw.Trim(), out var guid))
            {
                Console.WriteLine($"[info] - Giai ma QR thanh cong: {guid}");
                return guid;
            }

            // Thử parse nếu có prefix (ví dụ: "poi:xxxxxxxx-xxxx-...")
            if (raw.Contains(":"))
            {
                var parts = raw.Split(':', 2);
                if (parts.Length == 2 && Guid.TryParse(parts[1].Trim(), out var guidFromPrefix))
                {
                    Console.WriteLine($"[info] - Giai ma QR (co prefix) thanh cong: {guidFromPrefix}");
                    return guidFromPrefix;
                }
            }

            Console.WriteLine($"[error] - QR khong hop le: '{raw}'");
            return null;
        }
    }
}
