using Microsoft.EntityFrameworkCore;
using TravelGuide.Core.Models; // ✅ dùng Core

namespace TravelGuide.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<POI> POIs => Set<POI>();
    public DbSet<POIContent> POIContents => Set<POIContent>();
    public DbSet<User> Users => Set<User>();
    public DbSet<QRCode> QRCodes => Set<QRCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<POI>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Category).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<POIContent>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.POIId, x.LanguageCode }).IsUnique();
            e.HasOne(x => x.POI)
             .WithMany(p => p.Contents)
             .HasForeignKey(x => x.POIId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.PreferredLanguage).HasDefaultValue("vi");
        });

        modelBuilder.Entity<QRCode>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.POI)
             .WithOne(p => p.QRCode)
             .HasForeignKey<QRCode>(x => x.POIId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}