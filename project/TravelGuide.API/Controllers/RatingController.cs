using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelGuide.API.Data;
using TravelGuide.API.Models.Entities;

namespace TravelGuide.API.Controllers
{
    [ApiController]
    [Route("api/poi/{poiId}/ratings")]
    [Authorize]
    public class RatingController : ControllerBase
    {
        private readonly AppDbContext _db;
        public RatingController(AppDbContext db) { _db = db; }

        // GET /api/poi/{poiId}/ratings
        [HttpGet]
        public async Task<IActionResult> GetRatings(Guid poiId)
        {
            var ratings = await _db.POIRatings
                .Where(r => r.POIId == poiId)
                .Include(r => r.User)
                .Select(r => new
                {
                    r.Id,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    Username = r.User.FullName ?? r.User.Username
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var summary = new
            {
                TotalRatings  = ratings.Count,
                AverageRating = ratings.Any()
                    ? Math.Round(ratings.Average(r => r.Rating), 1) : 0,
                Ratings       = ratings
            };

            return Ok(summary);
        }

        // POST /api/poi/{poiId}/ratings
        [HttpPost]
        public async Task<IActionResult> AddRating(
            Guid poiId, [FromBody] AddRatingRequest request)
        {
            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest(new { message = "Đánh giá phải từ 1 đến 5 sao" });

            var userId = Guid.Parse(User.FindFirst(
                ClaimTypes.NameIdentifier)!.Value);

            // Kiểm tra POI tồn tại
            var poi = await _db.POIs.FindAsync(poiId);
            if (poi == null) return NotFound();

            // Kiểm tra đã đánh giá chưa
            var existing = await _db.POIRatings
                .FirstOrDefaultAsync(r => r.POIId == poiId && r.UserId == userId);

            if (existing != null)
            {
                // Cập nhật đánh giá cũ
                existing.Rating    = request.Rating;
                existing.Comment   = request.Comment;
                existing.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                // Thêm đánh giá mới
                _db.POIRatings.Add(new POIRating
                {
                    Id        = Guid.NewGuid(),
                    POIId     = poiId,
                    UserId    = userId,
                    Rating    = request.Rating,
                    Comment   = request.Comment,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();

            // Trả về summary mới
            var totalRatings = await _db.POIRatings
                .CountAsync(r => r.POIId == poiId);
            var avgRating = await _db.POIRatings
                .Where(r => r.POIId == poiId)
                .AverageAsync(r => (double)r.Rating);

            return Ok(new
            {
                success       = true,
                totalRatings,
                averageRating = Math.Round(avgRating, 1)
            });
        }

        // DELETE /api/poi/{poiId}/ratings (xóa đánh giá của mình)
        [HttpDelete]
        public async Task<IActionResult> DeleteRating(Guid poiId)
        {
            var userId = Guid.Parse(User.FindFirst(
                ClaimTypes.NameIdentifier)!.Value);

            var rating = await _db.POIRatings
                .FirstOrDefaultAsync(r => r.POIId == poiId && r.UserId == userId);

            if (rating == null) return NotFound();

            _db.POIRatings.Remove(rating);
            await _db.SaveChangesAsync();

            return Ok(new { success = true });
        }
    }

    public class AddRatingRequest
    {
        public int    Rating  { get; set; }
        public string? Comment { get; set; }
    }
}
