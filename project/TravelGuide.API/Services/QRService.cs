using Microsoft.EntityFrameworkCore;
using QRCoder;
using TravelGuide.API.Data;
using TravelGuide.Core.Models;

namespace TravelGuide.API.Services;

public interface IQRService
{
    Task<byte[]> GenerateQRAsync(Guid poiId);
    Task<string> GetBase64QRAsync(Guid poiId);
}

public class QRService : IQRService
{
    private readonly AppDbContext _db;
    private readonly ILogger<QRService> _logger;

    public QRService(AppDbContext db, ILogger<QRService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Sinh anh QR chua poiId, tra ve byte[] PNG
    /// </summary>
    public async Task<byte[]> GenerateQRAsync(Guid poiId)
    {
        _logger.LogInformation("[info] - Bat dau generate QR cho poiId={PoiId}", poiId);

        var encodedValue = poiId.ToString();

        // Luu hoac cap nhat QRCode record
        var existing = await _db.QRCodes.FirstOrDefaultAsync(q => q.POIId == poiId);
        if (existing == null)
        {
            _db.QRCodes.Add(new QRCode
            {
                POIId = poiId,
                EncodedValue = encodedValue,
                GeneratedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.EncodedValue = encodedValue;
            existing.GeneratedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();

        // Tao anh QR
        using var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(encodedValue, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        var pngBytes = qrCode.GetGraphic(10);

        _logger.LogInformation("[info] - Da tao QR thanh cong cho poiId={PoiId}, size={Size} bytes", poiId, pngBytes.Length);

        return pngBytes;
    }

    /// <summary>
    /// Tra ve QR dang Base64 de nhung vao web
    /// </summary>
    public async Task<string> GetBase64QRAsync(Guid poiId)
    {
        _logger.LogInformation("[info] - Lay Base64 QR cho poiId={PoiId}", poiId);

        var pngBytes = await GenerateQRAsync(poiId);
        var base64 = Convert.ToBase64String(pngBytes);

        _logger.LogInformation("[log] - Da chuyen QR sang Base64 cho poiId={PoiId}", poiId);

        return base64;
    }
}