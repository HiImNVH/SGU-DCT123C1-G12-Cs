using Microsoft.EntityFrameworkCore;
using TravelGuide.API.Models.Entities;

namespace TravelGuide.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<POI> POIs { get; set; }
        public DbSet<POIContent> POIContents { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<VisitHistory> VisitHistories { get; set; }
        public DbSet<POIRating> POIRatings { get; set; } // ← thêm

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email).IsUnique()
                .HasFilter("[Email] IS NOT NULL AND [Email] != ''");

            modelBuilder.Entity<POIContent>()
                .HasOne(c => c.POI)
                .WithMany(p => p.Contents)
                .HasForeignKey(c => c.POIId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<POIContent>()
                .HasOne(c => c.Language)
                .WithMany()
                .HasForeignKey(c => c.LanguageCode)
                .HasPrincipalKey(l => l.Code);

            modelBuilder.Entity<POI>()
                .HasOne(p => p.Creator)
                .WithMany()
                .HasForeignKey(p => p.CreatedBy)
                .OnDelete(DeleteBehavior.NoAction);

            // POIRating
            modelBuilder.Entity<POIRating>()
                .HasOne(r => r.POI)
                .WithMany(p => p.Ratings)
                .HasForeignKey(r => r.POIId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<POIRating>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Mỗi user chỉ đánh giá 1 lần mỗi POI
            modelBuilder.Entity<POIRating>()
                .HasIndex(r => new { r.POIId, r.UserId }).IsUnique();

            // Seed Languages
            modelBuilder.Entity<Language>().HasData(
                new Language { Code = "vi", Name = "Vietnamese", NativeName = "Tiếng Việt", IsActive = true },
                new Language { Code = "en", Name = "English", NativeName = "English", IsActive = true },
                new Language { Code = "ja", Name = "Japanese", NativeName = "日本語", IsActive = true },
                new Language { Code = "ko", Name = "Korean", NativeName = "한국어", IsActive = true },
                new Language { Code = "zh", Name = "Chinese", NativeName = "中文", IsActive = true },
                new Language { Code = "fr", Name = "French", NativeName = "Français", IsActive = true }
            );
        }
    }
}