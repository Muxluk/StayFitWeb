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

    public Task<IEnumerable<User>> GetByRoleAsync(string role)
    {
        // TODO: Реалізувати фільтр за роллю відповідно до моделі ролей.
        return Task.FromResult<IEnumerable<User>>(new List<User>());
    }

    public async Task<User?> GetByEmailAsync(string email) =>
        await DbSet.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<IEnumerable<User>> GetUsersWithFoodLogsAsync() =>
        await DbSet.Include(u => u.FoodLogs).ThenInclude(fl => fl.Food).ToListAsync();
}
