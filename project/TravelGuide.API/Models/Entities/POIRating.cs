using System.ComponentModel.DataAnnotations;

namespace TravelGuide.API.Models.Entities
{
    public class POIRating
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid POIId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public POI POI { get; set; }
        public User User { get; set; }
    }
}