// Services/CacheService.cs
using TravelGuide.Models;
using TravelGuide.Models.DTOs;
using TravelGuide.Repositories;

namespace TravelGuide.Services
{
    /// <summary>
    /// CacheModule - Lưu SQLite: POI, Content, Audio. Get data từ local. Clear/update cache.
    /// Thư viện: sqlite-net-pcl
    /// </summary>
    public class CacheService
    {
        private readonly LocalCacheRepository _repo;

        public CacheService(LocalCacheRepository repo)
        {
            _repo = repo;
        }

        /// <summary>Lấy POI từ local DB</summary>
        public async Task<LocalCacheEntry?> GetPOIAsync(Guid poiId, string lang)
        {
            Console.WriteLine($"[log] - Kiem tra cache: {poiId} ({lang})");
            var entry = await _repo.GetAsync(poiId, lang);

            if (entry != null && IsExpired(entry))
            {
                Console.WriteLine("[warn] - Cache da het han (>24 gio), can cap nhat");
                return null; // buộc gọi API để refresh
            }

            return entry;
        }

        /// <summary>Lưu/cập nhật cache từ POIDetailDto</summary>
        public async Task SavePOIAsync(POIDetailDto dto, string lang)
        {
            Console.WriteLine($"[log] - Luu cache: {dto.Id} ({lang})");

            var entry = new LocalCacheEntry
            {
                POIId = dto.Id.ToString(),
                LanguageCode = lang,
                Name = dto.Name,
                Category = dto.Category,
                ImageUrl = dto.ImageUrl,
                NarrationText = dto.Content?.NarrationText ?? "",
                AudioUrl = dto.Content?.AudioUrl,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                CachedAt = DateTime.UtcNow
            };

            await _repo.InsertOrReplaceAsync(entry);
            Console.WriteLine("[info] - Da luu cache thanh cong");
        }

        /// <summary>Lấy toàn bộ POI đã cache (dùng cho Map)</summary>
        public async Task<List<LocalCacheEntry>> GetAllPOIsAsync()
        {
            Console.WriteLine("[log] - Lay toan bo POI tu cache (Map)");
            return await _repo.GetAllAsync();
        }

        /// <summary>Tải audio về máy, trả về đường dẫn local</summary>
        public async Task<string?> SaveAudioFileAsync(string url, Guid poiId, string lang)
        {
            if (string.IsNullOrEmpty(url)) return null;

            try
            {
                Console.WriteLine($"[log] - Tai audio ve may: {url}");
                using var http = new HttpClient();
                var bytes = await http.GetByteArrayAsync(url);

                var dir = Path.Combine(FileSystem.AppDataDirectory, "audio");
                Directory.CreateDirectory(dir);

                var fileName = $"{poiId}_{lang}.mp3";
                var filePath = Path.Combine(dir, fileName);
                await File.WriteAllBytesAsync(filePath, bytes);

                // Cập nhật LocalAudioPath trong SQLite
                var entry = await _repo.GetAsync(poiId, lang);
                if (entry != null)
                {
                    entry.LocalAudioPath = filePath;
                    await _repo.InsertOrReplaceAsync(entry);
                }

                Console.WriteLine($"[info] - Da luu audio: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[error] - Loi tai audio: {ex.Message}");
                return null;
            }
        }

        /// <summary>Kiểm tra cache cũ hơn 24 giờ</summary>
        public bool IsExpired(LocalCacheEntry entry)
        {
            return (DateTime.UtcNow - entry.CachedAt).TotalHours > 24;
        }

        /// <summary>Xóa cache cũ hơn 7 ngày</summary>
        public async Task ClearExpiredAsync()
        {
            Console.WriteLine("[log] - Xoa cache cu");
            await _repo.DeleteExpiredAsync();
        }
    }
}
