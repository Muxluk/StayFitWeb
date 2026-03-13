using Microsoft.EntityFrameworkCore;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Data;

namespace StayFit.Infrastructure.Repositories;

public class FoodLogRepository : Repository<FoodLog>, IFoodLogRepository
{
    public FoodLogRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<FoodLog>> GetByUserIdAsync(int userId) =>
        await _dbSet
            .Include(fl => fl.Food)
            .Where(fl => fl.UserId == userId)
            .OrderByDescending(fl => fl.LoggedAt)
            .ToListAsync();

    public async Task<IEnumerable<FoodLog>> GetByUserIdAndDateAsync(int userId, DateTime date) =>
        await _dbSet
            .Include(fl => fl.Food)
            .Where(fl => fl.UserId == userId && fl.LoggedAt.Date == date.Date)
            .OrderByDescending(fl => fl.LoggedAt)
            .ToListAsync();
}
