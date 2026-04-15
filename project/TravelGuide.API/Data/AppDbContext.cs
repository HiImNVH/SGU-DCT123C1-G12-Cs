// TravelGuide.API/Data/AppDbContext.cs
// Thêm DbSet<DeviceSession> vào AppDbContext hiện tại

// Tìm dòng:
//     public DbSet<QRCode> QRCodes => Set<QRCode>();
// Thêm ngay bên dưới:
//     public DbSet<DeviceSession> DeviceSessions => Set<DeviceSession>();

// Tìm protected override void OnModelCreating, thêm block này vào cuối trước dấu }:
/*
modelBuilder.Entity<DeviceSession>(e =>
{
    e.HasKey(x => x.DeviceId);
    e.Property(x => x.DeviceId).HasMaxLength(100);
    e.Property(x => x.Platform).HasMaxLength(50);
    e.Property(x => x.OsVersion).HasMaxLength(50);
    e.Property(x => x.AppVersion).HasMaxLength(20);
    e.Property(x => x.LanguageCode).HasMaxLength(10).HasDefaultValue("vi");
    e.Property(x => x.Username).HasMaxLength(100);
});
*/

// ── File AppDbContext.cs hoàn chỉnh (copy đè toàn bộ) ──────────────
using Microsoft.EntityFrameworkCore;
using TravelGuide.Core.Models;

namespace TravelGuide.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<POI>           POIs           => Set<POI>();
    public DbSet<POIContent>    POIContents    => Set<POIContent>();
    public DbSet<User>          Users          => Set<User>();
    public DbSet<QRCode>        QRCodes        => Set<QRCode>();
    public DbSet<DeviceSession> DeviceSessions => Set<DeviceSession>(); // ← THÊM

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

        // ── DeviceSession ────────────────────────────────────────────────
        modelBuilder.Entity<DeviceSession>(e =>
        {
            e.HasKey(x => x.DeviceId);
            e.Property(x => x.DeviceId).HasMaxLength(100);
            e.Property(x => x.Platform).HasMaxLength(50);
            e.Property(x => x.OsVersion).HasMaxLength(50);
            e.Property(x => x.AppVersion).HasMaxLength(20);
            e.Property(x => x.LanguageCode).HasMaxLength(10).HasDefaultValue("vi");
            e.Property(x => x.Username).HasMaxLength(100);
        });
    }
}
