// Models/POI.cs
using System;
using System.Collections.Generic;

namespace TravelGuide.Models
{
    public class POI
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int Radius { get; set; } = 50;       // mét, default 50m
        public string ImageUrl { get; set; }
        public string Category { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid CreatedBy { get; set; }

        public int TotalRatings { get; set; }
        public double AverageRating { get; set; }

        // Navigation
        public ICollection<POIContent> Contents { get; set; } = new List<POIContent>();

        // Helper methods (từ Class Diagram)
        public bool IsInRange(decimal lat, decimal lng)
        {
            var distance = GetDistance((double)lat, (double)lng);
            return distance <= Radius;
        }

        public double GetDistance(double lat, double lng)
        {
            const double R = 6371000; // bán kính trái đất (mét)
            var dLat = ToRad(lat - (double)Latitude);
            var dLng = ToRad(lng - (double)Longitude);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                  + Math.Cos(ToRad((double)Latitude)) * Math.Cos(ToRad(lat))
                  * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private double ToRad(double deg) => deg * Math.PI / 180;
    }
}