using Microsoft.EntityFrameworkCore;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Data;

namespace StayFit.Infrastructure.Repositories;

public class NutritionGoalRepository : Repository<NutritionGoal>, INutritionGoalRepository
{
    public NutritionGoalRepository(AppDbContext context) : base(context) { }

    public async Task<NutritionGoal?> GetByUserIdAsync(string userId)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task<bool> ExistsForUserAsync(string userId)
    {
        return await _dbSet.AnyAsync(x => x.UserId == userId);
    }
}