namespace TravelGuide.API.Models.DTOs
{
    public class POIDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int Radius { get; set; } = 50;
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<POIContentDto> Contents { get; set; } = new();
        public double AverageRating { get; set; } = 0;
        public int TotalRatings { get; set; } = 0;
    }

    public class POIContentDto
    {
        public Guid Id { get; set; }
        public string LanguageCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string NarrationText { get; set; } = string.Empty;
        public string AudioUrl { get; set; } = string.Empty;
    }

    public class CreatePOIRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int Radius { get; set; } = 50;
        public string? ImageUrl { get; set; }
        public string Category { get; set; } = string.Empty;
        public List<CreatePOIContentRequest>? Contents { get; set; }
    }

    public class CreatePOIContentRequest
    {
        public string LanguageCode { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string NarrationText { get; set; } = string.Empty;
        public string? AudioUrl { get; set; }
    }
}