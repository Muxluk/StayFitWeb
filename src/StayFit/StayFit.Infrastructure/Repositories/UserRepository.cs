using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Data;

namespace StayFit.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<IEnumerable<User>> GetActiveAsync() =>
        await DbSet.Where(u => u.FoodLogs.Any()).ToListAsync();

    public async Task<IEnumerable<User>> GetByRoleAsync(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return new List<User>();
        }

        var normalizedRole = role.Trim().ToUpperInvariant();

        var roleUserEmails = await
            (from appUser in Context.Users
             join userRole in Context.UserRoles on appUser.Id equals userRole.UserId
             join appRole in Context.Roles on userRole.RoleId equals appRole.Id
             where appRole.NormalizedName == normalizedRole
             select appUser.Email)
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Distinct()
            .ToListAsync();

        if (roleUserEmails.Count == 0)
        {
            return new List<User>();
        }

        return await DbSet
            .Where(u => roleUserEmails.Contains(u.Email))
            .ToListAsync();
    }

    public async Task<User?> GetByEmailAsync(string email) =>
        await DbSet.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<IEnumerable<User>> GetUsersWithFoodLogsAsync() =>
        await DbSet.Include(u => u.FoodLogs).ThenInclude(fl => fl.Food).ToListAsync();
}
