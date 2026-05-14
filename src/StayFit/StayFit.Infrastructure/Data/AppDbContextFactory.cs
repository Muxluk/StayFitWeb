using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace StayFit.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var webProjectDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "StayFit.Web"));

        var config = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(webProjectDirectory, "appsettings.json"), optional: false)
            .AddJsonFile(Path.Combine(webProjectDirectory, "appsettings.Development.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"));

        return new AppDbContext(optionsBuilder.Options);
    }
}