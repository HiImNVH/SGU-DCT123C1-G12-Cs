// Repositories/LocalCacheRepository.cs
using SQLite;
using TravelGuide.Constants;
using TravelGuide.Models;

namespace TravelGuide.Repositories
{
    /// <summary>
    /// Truy cập SQLite trực tiếp - SELECT WHERE POIId = ? AND LanguageCode = ?
    /// </summary>
    public class LocalCacheRepository
    {
        private SQLiteAsyncConnection _db;

        private async Task<SQLiteAsyncConnection> GetDbAsync()
        {
            if (_db != null) return _db;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "travelguide_cache.db");
            _db = new SQLiteAsyncConnection(dbPath);
            await _db.CreateTableAsync<LocalCacheEntry>();
            Console.WriteLine("[log] - Da khoi tao SQLite: " + dbPath);
            return _db;
        }

        /// <summary>SELECT WHERE POIId = ? AND LanguageCode = ?</summary>
        public async Task<LocalCacheEntry?> GetAsync(Guid poiId, string lang)
        {
            try
            {
                var db = await GetDbAsync();
                var id = LocalCacheEntry.MakeId(poiId, lang);
                var entry = await db.FindAsync<LocalCacheEntry>(id);
                Console.WriteLine(entry != null
                    ? $"[info] - Tim thay cache: {poiId} ({lang})"
                    : $"[log] - Khong co cache: {poiId} ({lang})");
                return entry;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[error] - Loi doc cache: {ex.Message}");
                return null;
            }
        }

        /// <summary>SELECT * (dùng cho Map)</summary>
        public async Task<List<LocalCacheEntry>> GetAllAsync()
        {
            try
            {
                var db = await GetDbAsync();
                return await db.Table<LocalCacheEntry>().ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[error] - Loi lay danh sach cache: {ex.Message}");
                return new List<LocalCacheEntry>();
            }
        }

        /// <summary>UPSERT vào SQLite</summary>
        public async Task InsertOrReplaceAsync(LocalCacheEntry entry)
        {
            try
            {
                var db = await GetDbAsync();
                entry.Id = LocalCacheEntry.MakeId(Guid.Parse(entry.POIId), entry.LanguageCode);
                await db.InsertOrReplaceAsync(entry);
                Console.WriteLine($"[info] - Da luu cache: {entry.POIId} ({entry.LanguageCode})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[error] - Loi luu cache: {ex.Message}");
            }
        }

        /// <summary>Xóa cache cũ hơn 7 ngày</summary>
        public async Task DeleteExpiredAsync()
        {
            try
            {
                var db = await GetDbAsync();
                var cutoff = DateTime.UtcNow.AddDays(-AppConstants.CacheExpireDays);
                await db.ExecuteAsync(
                    "DELETE FROM LocalCacheEntry WHERE CachedAt < ?",
                    cutoff);
                Console.WriteLine("[log] - Da xoa cache cu hon 7 ngay");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[error] - Loi xoa cache: {ex.Message}");
            }
        }
    }
}
