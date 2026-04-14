using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TravelGuide.API.Data;

/// <summary>
/// Chi dung cho EF Tools (Add-Migration, Update-Database).
/// Khong chay khi app chay binh thuong.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\MSSQLLocalDB;Database=TravelGuideDb;Trusted_Connection=True;TrustServerCertificate=True;"
        );

        return new AppDbContext(optionsBuilder.Options);
    }
}