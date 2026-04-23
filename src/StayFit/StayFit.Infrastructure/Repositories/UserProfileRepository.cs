using Microsoft.EntityFrameworkCore;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Data;

namespace StayFit.Infrastructure.Repositories;

/// <summary>
/// Репозиторій для управління профілями користувачів
/// </summary>
public class UserProfileRepository : Repository<UserProfile>, IUserProfileRepository
{
    public UserProfileRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<UserProfile?> GetByUserIdAsync(int userId)
    {
        return await DbSet.FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<bool> ExistsForUserAsync(int userId)
    {
        return await DbSet.AnyAsync(p => p.UserId == userId);
    }

    public async Task<bool> UpdateProfilePhotoPathAsync(int userId, string profilePhotoPath)
    {
        var profile = await DbSet.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
        {
            return false;
        }

        profile.ProfilePhotoPath = profilePhotoPath;
        profile.UpdatedAt = DateTime.UtcNow;

        await Context.SaveChangesAsync();
        return true;
    }
}
