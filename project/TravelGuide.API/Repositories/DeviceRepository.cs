// TravelGuide.API/Repositories/DeviceRepository.cs
using Microsoft.EntityFrameworkCore;
using TravelGuide.API.Data;
using TravelGuide.Core.Models;

namespace TravelGuide.API.Repositories;

public interface IDeviceRepository
{
    Task UpsertAsync(DeviceSession session);
    Task IncrementScanAsync(string deviceId);
    Task<List<DeviceSession>> GetAllAsync();
    Task<int> CountActiveAsync(DateTime since);
    Task<int> CountTotalAsync();
}

public class DeviceRepository : IDeviceRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<DeviceRepository> _logger;

    public DeviceRepository(AppDbContext db, ILogger<DeviceRepository> logger)
    {
        _db     = db;
        _logger = logger;
    }

    /// <summary>
    /// Upsert: nếu DeviceId đã tồn tại → cập nhật LastSeenAt + SessionCount
    ///         nếu chưa → tạo mới
    /// </summary>
    public async Task UpsertAsync(DeviceSession session)
    {
        var existing = await _db.DeviceSessions
            .FirstOrDefaultAsync(d => d.DeviceId == session.DeviceId);

        if (existing == null)
        {
            session.FirstSeenAt  = DateTime.UtcNow;
            session.LastSeenAt   = DateTime.UtcNow;
            session.SessionCount = 1;
            _db.DeviceSessions.Add(session);
            _logger.LogInformation("[info] - Thiet bi moi dang ky: {DeviceId} ({Platform})", session.DeviceId, session.Platform);
        }
        else
        {
            existing.LastSeenAt    = DateTime.UtcNow;
            existing.SessionCount += 1;
            existing.LanguageCode  = session.LanguageCode;
            existing.OsVersion     = session.OsVersion;
            existing.AppVersion    = session.AppVersion;
            if (session.Username != null)
                existing.Username  = session.Username;

            _logger.LogInformation("[info] - Cap nhat phien thiet bi: {DeviceId}, session={Count}", session.DeviceId, existing.SessionCount);
        }

        await _db.SaveChangesAsync();
    }

    /// <summary>Tăng ScanCount khi thiết bị scan QR thành công</summary>
    public async Task IncrementScanAsync(string deviceId)
    {
        var device = await _db.DeviceSessions
            .FirstOrDefaultAsync(d => d.DeviceId == deviceId);

        if (device != null)
        {
            device.ScanCount   += 1;
            device.LastSeenAt   = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            _logger.LogInformation("[info] - Tang scan count: {DeviceId}, total={Count}", deviceId, device.ScanCount);
        }
    }

    public async Task<List<DeviceSession>> GetAllAsync()
        => await _db.DeviceSessions
            .OrderByDescending(d => d.LastSeenAt)
            .ToListAsync();

    public async Task<int> CountActiveAsync(DateTime since)
        => await _db.DeviceSessions
            .CountAsync(d => d.LastSeenAt >= since);

    public async Task<int> CountTotalAsync()
        => await _db.DeviceSessions.CountAsync();
}
