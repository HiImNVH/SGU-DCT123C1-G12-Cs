// TravelGuide.API/Services/DeviceService.cs
using TravelGuide.API.Repositories;
using TravelGuide.Core.DTOs;
using TravelGuide.Core.Models;

namespace TravelGuide.API.Services;

public interface IDeviceService
{
    Task PingAsync(DevicePingRequest request);
    Task RecordScanAsync(string deviceId);
    Task<DeviceStatsDto> GetStatsAsync();
}

public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _repo;
    private readonly ILogger<DeviceService> _logger;

    public DeviceService(IDeviceRepository repo, ILogger<DeviceService> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task PingAsync(DevicePingRequest request)
    {
        var session = new DeviceSession
        {
            DeviceId     = request.DeviceId,
            Platform     = request.Platform,
            OsVersion    = request.OsVersion,
            AppVersion   = request.AppVersion,
            LanguageCode = request.LanguageCode,
            Username     = request.Username
        };
        await _repo.UpsertAsync(session);
    }

    public async Task RecordScanAsync(string deviceId)
        => await _repo.IncrementScanAsync(deviceId);

    public async Task<DeviceStatsDto> GetStatsAsync()
    {
        var now    = DateTime.UtcNow;
        var all    = await _repo.GetAllAsync();

        var stats = new DeviceStatsDto
        {
            TotalDevices   = all.Count,
            ActiveToday    = all.Count(d => d.LastSeenAt >= now.AddHours(-24)),
            ActiveThisWeek = all.Count(d => d.LastSeenAt >= now.AddDays(-7)),
            ActiveThisMonth= all.Count(d => d.LastSeenAt >= now.AddDays(-30)),
            LoggedInDevices= all.Count(d => d.Username != null),
            GuestDevices   = all.Count(d => d.Username == null),
            TotalScans     = all.Sum(d => d.ScanCount),

            ByLanguage = all
                .GroupBy(d => d.LanguageCode)
                .Select(g => new LanguageStatDto { LanguageCode = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList(),

            ByPlatform = all
                .GroupBy(d => d.Platform)
                .Select(g => new PlatformStatDto { Platform = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList(),

            RecentDevices = all
                .Take(50)
                .Select(d => new DeviceListItemDto
                {
                    DeviceId     = d.DeviceId,
                    Platform     = d.Platform,
                    OsVersion    = d.OsVersion,
                    AppVersion   = d.AppVersion,
                    LanguageCode = d.LanguageCode,
                    Username     = d.Username,
                    LastSeenAt   = d.LastSeenAt,
                    FirstSeenAt  = d.FirstSeenAt,
                    ScanCount    = d.ScanCount,
                    SessionCount = d.SessionCount,
                    IsOnline     = d.LastSeenAt >= now.AddMinutes(-5)
                }).ToList()
        };

        _logger.LogInformation("[info] - Lay thong ke thiet bi: total={Total}, today={Today}", stats.TotalDevices, stats.ActiveToday);
        return stats;
    }
}
