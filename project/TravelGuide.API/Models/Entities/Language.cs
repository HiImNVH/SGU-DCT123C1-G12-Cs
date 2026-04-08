// Models/Entities/Language.cs
using System.ComponentModel.DataAnnotations;

namespace TravelGuide.API.Models.Entities
{
    public class Language
    {
        [Key, MaxLength(10)]
        public string Code { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string NativeName { get; set; }

        public bool IsActive { get; set; } = true;
    }
}