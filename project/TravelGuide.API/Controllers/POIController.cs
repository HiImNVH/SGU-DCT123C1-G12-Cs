// Controllers/POIController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelGuide.API.Data;
using TravelGuide.API.Models.Entities;

namespace TravelGuide.API.Controllers
{
    [ApiController]
    [Route("api/poi")]
    [Authorize]
    public class POIController : ControllerBase
    {
        private readonly AppDbContext _db;

        public POIController(AppDbContext db)
        {
            _db = db;
        }

        // GET /api/poi
[HttpGet]
public async Task<IActionResult> GetAll()
{
    var pois = await _db.POIs
        .Where(p => p.IsActive)
        .Include(p => p.Contents)
        .Include(p => p.Ratings)   // ← thêm
        .Select(p => new
        {
            p.Id, p.Name, p.Description,
            p.Latitude, p.Longitude, p.Radius,
            p.ImageUrl, p.Category, p.IsActive,
            TotalRatings  = p.Ratings.Count,
            AverageRating = p.Ratings.Any()
                ? Math.Round(p.Ratings.Average(r => (double)r.Rating), 1) : 0.0,
            Contents = p.Contents.Select(c => new
            {
                c.Id, c.LanguageCode, c.Title,
                c.NarrationText, c.AudioUrl
            })
        })
        .ToListAsync();

    return Ok(pois);
}

// GET /api/poi/{id}
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id)
{
    var poi = await _db.POIs
        .Include(p => p.Contents)
        .Include(p => p.Ratings)   // ← thêm
        .Where(p => p.Id == id && p.IsActive)
        .Select(p => new
        {
            p.Id, p.Name, p.Description,
            p.Latitude, p.Longitude, p.Radius,
            p.ImageUrl, p.Category, p.IsActive,
            TotalRatings  = p.Ratings.Count,
            AverageRating = p.Ratings.Any()
                ? Math.Round(p.Ratings.Average(r => (double)r.Rating), 1) : 0.0,
            Contents = p.Contents.Select(c => new
            {
                c.Id, c.LanguageCode, c.Title,
                c.NarrationText, c.AudioUrl
            })
        })
        .FirstOrDefaultAsync();

    if (poi == null) return NotFound();
    return Ok(poi);
}

        // GET /api/poi/nearby?lat=...&lng=...&radius=...
        [HttpGet("nearby")]
        public async Task<IActionResult> GetNearby(
            [FromQuery] double lat,
            [FromQuery] double lng,
            [FromQuery] int radius = 1000)
        {
            var pois = await _db.POIs
                .Where(p => p.IsActive)
                .Include(p => p.Contents)
                .ToListAsync();

            // Tính khoảng cách và lọc
            var nearby = pois
                .Where(p => GetDistance(lat, lng, (double)p.Latitude, (double)p.Longitude) <= radius)
                .OrderBy(p => GetDistance(lat, lng, (double)p.Latitude, (double)p.Longitude))
                .Select(p => new
                {
                    p.Id, p.Name, p.Description,
                    p.Latitude, p.Longitude, p.Radius,
                    p.ImageUrl, p.Category,
                    Distance = Math.Round(GetDistance(lat, lng, (double)p.Latitude, (double)p.Longitude)),
                    Contents = p.Contents.Select(c => new
                    {
                        c.Id, c.LanguageCode, c.Title, c.NarrationText
                    })
                })
                .ToList();

            return Ok(nearby);
        }

        // GET /api/poi/{id}/content/{lang}
        [HttpGet("{id}/content/{lang}")]
        public async Task<IActionResult> GetContent(Guid id, string lang)
        {
            var content = await _db.POIContents
                .FirstOrDefaultAsync(c => c.POIId == id && c.LanguageCode == lang);

            // Fallback sang tiếng Việt nếu không có ngôn ngữ yêu cầu
            if (content == null)
                content = await _db.POIContents
                    .FirstOrDefaultAsync(c => c.POIId == id && c.LanguageCode == "vi");

            if (content == null) return NotFound();
            return Ok(content);
        }

        // POST /api/poi (Admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreatePOIRequest request)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var poi = new POI
            {
                Id          = Guid.NewGuid(),
                Name        = request.Name,
                Description = request.Description,
                Latitude    = request.Latitude,
                Longitude   = request.Longitude,
                Radius      = request.Radius,
                ImageUrl    = request.ImageUrl ?? "",
                Category    = request.Category,
                IsActive    = true,
                CreatedAt   = DateTime.UtcNow,
                CreatedBy   = userId
            };

            // Thêm nội dung đa ngôn ngữ
            if (request.Contents != null)
            {
                foreach (var c in request.Contents)
                {
                    poi.Contents.Add(new POIContent
                    {
                        Id             = Guid.NewGuid(),
                        POIId          = poi.Id,
                        LanguageCode   = c.LanguageCode,
                        Title          = c.Title,
                        NarrationText  = c.NarrationText,
                        AudioUrl       = "",
                        CreatedAt      = DateTime.UtcNow
                    });
                }
            }

            _db.POIs.Add(poi);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                poi.Id,
                poi.Name,
                poi.Description,
                poi.Latitude,
                poi.Longitude,
                poi.Radius,
                poi.ImageUrl,
                poi.Category,
                poi.IsActive,
                poi.CreatedAt,
                Contents = poi.Contents.Select(c => new
                {
                    c.Id,
                    c.LanguageCode,
                    c.Title,
                    c.NarrationText,
                    c.AudioUrl
                })
            });
        }

        // PUT /api/poi/{id} (Admin only)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreatePOIRequest request)
        {
            var poi = await _db.POIs.FindAsync(id);
            if (poi == null) return NotFound();

            poi.Name        = request.Name;
            poi.Description = request.Description;
            poi.Latitude    = request.Latitude;
            poi.Longitude   = request.Longitude;
            poi.Radius      = request.Radius;
            poi.Category    = request.Category;
            poi.ImageUrl    = request.ImageUrl ?? poi.ImageUrl;
            poi.UpdatedAt   = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // DELETE /api/poi/{id} (Admin only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var poi = await _db.POIs.FindAsync(id);
            if (poi == null) return NotFound();

            poi.IsActive  = false; // Soft delete
            poi.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new { success = true });
        }

        private double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000;
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                  + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
                  * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private double ToRad(double deg) => deg * Math.PI / 180;
    }



    public class CreatePOIRequest
    {
        public string Name        { get; set; }
        public string Description { get; set; }
        public decimal Latitude   { get; set; }
        public decimal Longitude  { get; set; }
        public int Radius         { get; set; } = 50;
        public string ImageUrl    { get; set; }
        public string Category    { get; set; }
        public List<CreatePOIContentRequest> Contents { get; set; }
    }

    public class CreatePOIContentRequest
    {
        public string LanguageCode  { get; set; }
        public string Title         { get; set; }
        public string NarrationText { get; set; }
    }
}