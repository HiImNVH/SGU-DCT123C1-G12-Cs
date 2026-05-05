// TravelGuide.AdminWeb/Models/QRResult.cs
namespace TravelGuide.AdminWeb.Models
{
    public class QRResult
    {
        public string QrImageBase64 { get; set; } = string.Empty;
        public string DataUri { get; set; } = string.Empty;
        public string DeepLink { get; set; } = string.Empty;
    }
}