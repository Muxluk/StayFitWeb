using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StayFit.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        // Design-time connection string (Neon PostgreSQL)
        optionsBuilder.UseNpgsql(
            "Host=ep-ancient-tooth-an0wah2y-pooler.c-6.us-east-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_XJ8a3YzyAkhl;Ssl Mode=Require;");

        return new AppDbContext(optionsBuilder.Options);
    }
}
