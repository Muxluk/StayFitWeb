using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StayFit.Application.Common;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Infrastructure.Identity;

namespace StayFit.Infrastructure.Services;

/// <summary>
/// Адмін-сервіс для керування обліковими записами користувачів.
/// </summary>
public sealed class AdminUserService(
    UserManager<ApplicationUser> userManager,
    ILogger<AdminUserService> logger)
    : IAdminUserService
{
    public async Task<Result<IReadOnlyList<AdminUserListItemDto>>> SearchUsersAsync(
        AdminUserSearchRequestDto request,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Admin user search requested. UserId={UserId}, Email={Email}",
            request.UserId,
            request.Email);

        var query = userManager.Users.AsNoTracking();

        if (request.UserId.HasValue)
        {
            query = query.Where(u => u.Id == request.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = request.Email.Trim().ToLowerInvariant();
            query = query.Where(u => u.Email != null && u.Email.ToLower().Contains(email));
        }

        var users = await query
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

        logger.LogInformation("Admin user search completed. Found {Count} users", users.Count);
        return users;
    }

    public async Task<Result<AdminUserDetailsDto>> GetUserDetailsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Admin requested user details for userId={UserId}", userId);

        var user = await userManager.Users
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

        if (user is null)
        {
            logger.LogWarning("Admin user details lookup failed: userId={UserId} not found", userId);
            return Result<AdminUserDetailsDto>.Failure("Користувача не знайдено");
        }

        return user;
    }

    public async Task<Result> BlockUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Admin requested block for userId={UserId}", userId);

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            logger.LogWarning("Admin block failed: userId={UserId} not found", userId);
            return Result.Failure("Користувача не знайдено");
        }

        user.LockoutEnabled = true;
        var lockoutResult = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        if (!lockoutResult.Succeeded)
        {
            var errors = lockoutResult.Errors.Select(e => e.Description).ToArray();
            logger.LogWarning(
                "Admin block failed for userId={UserId}. Errors: {Errors}",
                userId,
                string.Join("; ", errors));
            return Result.Failure(errors);
        }

        logger.LogInformation("Admin blocked user userId={UserId}", userId);
        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(
        int userId,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Admin requested password reset for userId={UserId}", userId);

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            return Result.Failure("Новий пароль обов'язковий");
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            logger.LogWarning("Admin password reset failed: userId={UserId} not found", userId);
            return Result.Failure("Користувача не знайдено");
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await userManager.ResetPasswordAsync(user, token, newPassword);

        if (!resetResult.Succeeded)
        {
            var errors = resetResult.Errors.Select(e => e.Description).ToArray();
            logger.LogWarning(
                "Admin password reset failed for userId={UserId}. Errors: {Errors}",
                userId,
                string.Join("; ", errors));
            return Result.Failure(errors);
        }

        await userManager.ResetAccessFailedCountAsync(user);

        logger.LogInformation("Admin password reset succeeded for userId={UserId}", userId);
        return Result.Success();
    }
}
