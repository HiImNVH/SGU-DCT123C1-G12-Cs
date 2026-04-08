// TravelGuide.API/Controllers/UserController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelGuide.API.Data;

namespace TravelGuide.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UserController(AppDbContext db) { _db = db; }

        // GET /api/users (Admin)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _db.Users.Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.FullName,
                Role = u.Role.ToString(), // ⭐ Chuyển enum thành string
                u.IsActive,
                u.PreferredLanguage,
                u.CreatedAt
            }).ToListAsync();

            return Ok(users); // ⭐ Trả về JSON array trực tiếp
        }

        // GET /api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.FullName,
                Role = user.Role.ToString(),
                user.IsActive,
                user.PreferredLanguage
            });
        }

        // PUT /api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.FullName = request.FullName ?? user.FullName;
            user.Email = request.Email ?? user.Email;
            user.AvatarUrl = request.AvatarUrl ?? user.AvatarUrl;
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(user);
        }

        // PATCH /api/users/{id}/language
        [HttpPatch("{id}/language")]
        public async Task<IActionResult> UpdateLanguage(Guid id, [FromBody] UpdateLanguageRequest request)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.PreferredLanguage = request.LanguageCode;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // ⭐ THÊM ENDPOINT MỚI: POST /api/users/toggle/{id}
        [HttpPost("toggle/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { success = false, message = "User not found" });

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new { success = true, isActive = user.IsActive });
        }

        // ⭐ GIỮ NGUYÊN ENDPOINT CŨ (cho tương thích)
        [HttpPatch("{id}/toggle-active")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleActivePatch(Guid id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { success = true, isActive = user.IsActive });
        }
    }

    public class UpdateUserRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
    }

    public class UpdateLanguageRequest
    {
        public string LanguageCode { get; set; }
    }
}