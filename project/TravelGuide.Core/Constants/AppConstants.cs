namespace TravelGuide.Core.Constants;

/// <summary>
/// Hang so dung chung toan he thong.
/// </summary>
public static class AppConstants
{
    // ── Cache ──────────────────────────────────────────
    /// <summary>Cache het han sau 24 gio (dung de check LocalCacheEntry)</summary>
    public const int CacheExpiryHours = 24;

    /// <summary>Xoa cache cu hon 7 ngay (dung trong LocalCacheRepository.DeleteExpiredAsync)</summary>
    public const int CacheExpiryDays = 7;

    // ── JWT ────────────────────────────────────────────
    /// <summary>JWT het han sau 7 ngay</summary>
    public const int JwtExpiryDays = 7;

    // ── API ────────────────────────────────────────────
    /// <summary>Timeout goi API tinh bang giay (yeu cau phi chuc nang: &lt; 5s)</summary>
    public const int ApiTimeoutSeconds = 5;
}
