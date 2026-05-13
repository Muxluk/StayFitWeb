using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StayFit.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(
    "Host=stayfit-db.c3s4wm8qc1g8.eu-north-1.rds.amazonaws.com;Port=5432;Database=stayfit-db;Username=admin1;Password=Kip,N+h2c5D!kHf;Ssl Mode=Require;Trust Server Certificate=true;");

        return new AppDbContext(optionsBuilder.Options);
    }
}
