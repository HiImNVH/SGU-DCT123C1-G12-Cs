// Models/Entities/POIContent.cs
using System.ComponentModel.DataAnnotations;

namespace TravelGuide.API.Models.Entities
{
    public class POIContent
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid POIId { get; set; }

        [Required, MaxLength(10)]
        public string LanguageCode { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string NarrationText { get; set; }

        [MaxLength(500)]
        public string AudioUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public POI POI { get; set; }
        public Language Language { get; set; }
    }
}