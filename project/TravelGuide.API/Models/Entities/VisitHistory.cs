// Models/Entities/VisitHistory.cs
using System.ComponentModel.DataAnnotations;

namespace TravelGuide.API.Models.Entities
{
    public class VisitHistory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid POIId { get; set; }

        public DateTime VisitedAt { get; set; } = DateTime.UtcNow;

        public int? Duration { get; set; } // giây

        // Navigation
        public User User { get; set; }
        public POI POI { get; set; }
    }
}