// Models/Entities/User.cs
using System.ComponentModel.DataAnnotations;

namespace TravelGuide.API.Models.Entities
{
    public enum UserRole { User, Admin }

    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string Username { get; set; }

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(500)]
        public string AvatarUrl { get; set; }

        [Required, MaxLength(10)]
        public string PreferredLanguage { get; set; } = "vi";

        public UserRole Role { get; set; } = UserRole.User;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}