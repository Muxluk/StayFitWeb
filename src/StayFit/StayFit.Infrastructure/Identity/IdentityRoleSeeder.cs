using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace StayFit.Infrastructure.Identity;

public static class IdentityRoleSeeder
{
    public const string AdminRole = "Admin";

    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("IdentityRoleSeeder");

        if (await roleManager.RoleExistsAsync(AdminRole))
        {
            return;
        }

        var createResult = await roleManager.CreateAsync(new IdentityRole<int>(AdminRole));
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create '{AdminRole}' role: {errors}");
        }

        logger.LogInformation("Role '{RoleName}' was created during startup.", AdminRole);
    }
}
