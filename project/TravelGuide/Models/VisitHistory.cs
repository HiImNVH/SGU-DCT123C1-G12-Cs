// Models/VisitHistory.cs
using System;
using TravelGuide.Models;

namespace TravelGuide.Models
{
    /// <summary>
    /// Lịch sử người dùng đã ghé thăm một POI.
    /// </summary>
    public class VisitHistory
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid POIId { get; set; }
        public DateTime VisitedAt { get; set; }
        public int? Duration { get; set; }      // giây

        // Navigation
        public User User { get; set; }
        public POI POI { get; set; }
    }
}