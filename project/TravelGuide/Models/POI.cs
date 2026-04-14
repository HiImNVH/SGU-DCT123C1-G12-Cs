// Models/POI.cs
namespace TravelGuide.Models
{
    public class POI
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string? ImageUrl { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsActive { get; set; } = true;
        public List<POIContent> Contents { get; set; } = new();
    }
}
