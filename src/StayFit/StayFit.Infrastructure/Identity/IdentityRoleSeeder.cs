using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace StayFit.Infrastructure.Identity;

public static class IdentityRoleSeeder
{
    public const string AdminRole = "Admin";
    private const string AdminSeedSectionPath = "IdentitySeed:Admin";

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

    public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("IdentityRoleSeeder");

        var adminSection = configuration.GetSection(AdminSeedSectionPath);
        var email = adminSection["Email"];
        var userName = adminSection["UserName"];
        var password = adminSection["Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                $"Missing admin seed configuration. Set '{AdminSeedSectionPath}:Email', '{AdminSeedSectionPath}:UserName', and '{AdminSeedSectionPath}:Password'.");
        }

        var adminUser = await userManager.FindByEmailAsync(email);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true,
            };

            var createResult = await userManager.CreateAsync(adminUser, password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create admin user '{email}': {errors}");
            }

            logger.LogInformation("Admin user '{Email}' was created during startup.", email);
        }

        if (!await userManager.IsInRoleAsync(adminUser, AdminRole))
        {
            var addToRoleResult = await userManager.AddToRoleAsync(adminUser, AdminRole);
            if (!addToRoleResult.Succeeded)
            {
                var errors = string.Join("; ", addToRoleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException(
                    $"Failed to assign '{AdminRole}' role to admin user '{email}': {errors}");
            }

            logger.LogInformation("Role '{RoleName}' was assigned to admin user '{Email}'.", AdminRole, email);
        }
    }
}
