using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Infrastructure.Identity;

namespace StayFit.Infrastructure.Repositories;

public class AdminUserRepository(UserManager<ApplicationUser> userManager) : IAdminUserRepository
{
    public async Task<IReadOnlyList<AdminUserListItemDto>> SearchUsersAsync(
        int? userId,
        string? email,
        CancellationToken cancellationToken = default)
    {
        var query = userManager.Users.AsNoTracking();

        if (userId.HasValue)
        {
            query = query.Where(u => u.Id == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            query = query.Where(u => u.Email != null && u.Email.ToLower().Contains(normalizedEmail));
        }

        return await query
            .OrderBy(u => u.Id)
            .Take(100)
            .Select(u => new AdminUserListItemDto
            {
                UserId = u.Id,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                IsLocked = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow,
                LockoutEnd = u.LockoutEnd,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminUserDetailsDto?> GetUserDetailsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await userManager.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new AdminUserDetailsDto
            {
                UserId = u.Id,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                IsLocked = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow,
                LockoutEnd = u.LockoutEnd,
                AccessFailedCount = u.AccessFailedCount,
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<(bool Succeeded, IReadOnlyList<string> Errors)> BlockUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return (false, ["Користувача не знайдено"]);
        }

        user.LockoutEnabled = true;
        var result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        if (!result.Succeeded)
        {
            return (false, result.Errors.Select(e => e.Description).ToArray());
        }

        return (true, Array.Empty<string>());
    }

    public async Task<(bool Succeeded, IReadOnlyList<string> Errors)> ResetPasswordAsync(
        int userId,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return (false, ["Користувача не знайдено"]);
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, newPassword);

        if (!result.Succeeded)
        {
            return (false, result.Errors.Select(e => e.Description).ToArray());
        }

        await userManager.ResetAccessFailedCountAsync(user);
        return (true, Array.Empty<string>());
    }
}
