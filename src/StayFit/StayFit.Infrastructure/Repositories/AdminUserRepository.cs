using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Infrastructure.Data;
using StayFit.Infrastructure.Identity;

namespace StayFit.Infrastructure.Repositories;

public class AdminUserRepository(
    UserManager<ApplicationUser> userManager,
    AppDbContext dbContext) : IAdminUserRepository
{
    public async Task<PagedResult<AdminUserListItemDto>> SearchUsersAsync(
        int? userId,
        string? email,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = userManager.Users.AsNoTracking();

        var hasUserId = userId.HasValue;
        var hasEmail = !string.IsNullOrWhiteSpace(email);

        if (hasUserId && hasEmail)
        {
            var normalizedEmail = email!.Trim().ToLowerInvariant();
            query = query.Where(u =>
                u.Id == userId!.Value ||
                (u.Email != null && u.Email.ToLower().Contains(normalizedEmail)));
        }
        else if (hasUserId)
        {
            query = query.Where(u => u.Id == userId!.Value);
        }
        else if (hasEmail)
        {
            var normalizedEmail = email!.Trim().ToLowerInvariant();
            query = query.Where(u => u.Email != null && u.Email.ToLower().Contains(normalizedEmail));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(u => u.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserListItemDto
            {
                UserId = u.Id,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                IsLocked = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow,
                LockoutEnd = u.LockoutEnd,
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminUserListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<AdminUserDetailsDto?> GetUserDetailsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from user in userManager.Users.AsNoTracking()
            join profile in dbContext.UserProfiles.AsNoTracking()
                on user.Id equals profile.UserId into profileJoin
            from profile in profileJoin.DefaultIfEmpty()
            where user.Id == userId
            select new AdminUserDetailsDto
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                LockoutEnd = user.LockoutEnd,
                AccessFailedCount = user.AccessFailedCount,
                Profile = profile == null
                    ? null
                    : new AdminUserProfileDto
                    {
                        FullName = profile.FullName,
                        DateOfBirth = profile.DateOfBirth,
                        Gender = profile.Gender,
                        Weight = profile.Weight,
                        Height = profile.Height,
                    },
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

    public async Task<(bool Succeeded, IReadOnlyList<string> Errors)> UnblockUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return (false, ["Користувача не знайдено"]);
        }

        var setLockoutResult = await userManager.SetLockoutEndDateAsync(user, null);
        if (!setLockoutResult.Succeeded)
        {
            return (false, setLockoutResult.Errors.Select(e => e.Description).ToArray());
        }

        await userManager.ResetAccessFailedCountAsync(user);
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

    public async Task<(bool Succeeded, IReadOnlyList<string> Errors)> UpdateUserAsync(
        int userId,
        AdminUpdateUserRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return (false, ["Користувача не знайдено"]);
        }

        user.UserName = request.UserName.Trim();
        user.Email = request.Email.Trim();

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return (false, updateResult.Errors.Select(e => e.Description).ToArray());
        }

        var hasProfileChanges =
            !string.IsNullOrWhiteSpace(request.FullName) ||
            request.DateOfBirth.HasValue ||
            !string.IsNullOrWhiteSpace(request.Gender) ||
            request.Weight.HasValue ||
            request.Height.HasValue;

        if (!hasProfileChanges)
        {
            return (true, Array.Empty<string>());
        }

        var profile = await dbContext.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is null)
        {
            profile = new UserProfile
            {
                UserId = userId,
                FullName = string.IsNullOrWhiteSpace(request.FullName) ? request.UserName.Trim() : request.FullName.Trim(),
                DateOfBirth = request.DateOfBirth,
                Gender = string.IsNullOrWhiteSpace(request.Gender) ? null : request.Gender.Trim(),
                Weight = request.Weight,
                Height = request.Height,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            await dbContext.UserProfiles.AddAsync(profile, cancellationToken);
        }
        else
        {
            profile.FullName = string.IsNullOrWhiteSpace(request.FullName) ? profile.FullName : request.FullName.Trim();
            profile.DateOfBirth = request.DateOfBirth;
            profile.Gender = string.IsNullOrWhiteSpace(request.Gender) ? null : request.Gender.Trim();
            profile.Weight = request.Weight;
            profile.Height = request.Height;
            profile.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return (true, Array.Empty<string>());
    }

    public async Task<(bool Succeeded, IReadOnlyList<string> Errors)> DeleteUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return (false, ["Користувача не знайдено"]);
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return (false, result.Errors.Select(e => e.Description).ToArray());
        }

        // Видалити профіль користувача, якщо існує
        var profile = await dbContext.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        
        if (profile is not null)
        {
            dbContext.UserProfiles.Remove(profile);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return (true, Array.Empty<string>());
    }
}
