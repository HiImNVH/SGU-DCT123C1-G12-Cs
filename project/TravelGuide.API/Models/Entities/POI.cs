using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelGuide.API.Models.Entities
{
    public class POI
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required, Column(TypeName = "decimal(10,8)")]
        public decimal Latitude { get; set; }

        [Required, Column(TypeName = "decimal(11,8)")]
        public decimal Longitude { get; set; }

        public int Radius { get; set; } = 50;

        [MaxLength(500)]
        public string ImageUrl { get; set; }

        [Required, MaxLength(50)]
        public string Category { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Guid CreatedBy { get; set; }

        // Navigation
        public ICollection<POIContent>  Contents { get; set; } = new List<POIContent>();
        public ICollection<POIRating>   Ratings  { get; set; } = new List<POIRating>();
        public User Creator { get; set; }

        // Computed (không lưu DB)
        [NotMapped]
        public int    TotalRatings   => Ratings?.Count ?? 0;
        [NotMapped]
        public double AverageRating  => Ratings?.Any() == true
            ? Math.Round(Ratings.Average(r => r.Rating), 1) : 0;
    }
}