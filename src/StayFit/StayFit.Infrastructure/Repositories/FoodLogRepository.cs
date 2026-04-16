using Microsoft.EntityFrameworkCore;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Data;

namespace StayFit.Infrastructure.Repositories;

public class FoodLogRepository : Repository<FoodLog>, IFoodLogRepository
{
    public FoodLogRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<IEnumerable<FoodLog>> GetByUserIdAsync(int userId) =>
        await DbSet
            .Include(fl => fl.Food)
            .Where(fl => fl.UserId == userId)
            .OrderByDescending(fl => fl.LoggedAt)
            .ToListAsync();

    public async Task<IEnumerable<FoodLog>> GetByUserIdAndDateAsync(int userId, DateTime date)
    {
        // UI-bound dates often come with DateTimeKind.Unspecified.
        // Treat selected calendar date as local day and query UTC boundaries.
        var localDay = date.Kind == DateTimeKind.Utc
            ? date.ToLocalTime().Date
            : date.Date;

        var startOfDayLocal = DateTime.SpecifyKind(localDay, DateTimeKind.Local);
        var endOfDayLocal = startOfDayLocal.AddDays(1);

        var startOfDayUtc = startOfDayLocal.ToUniversalTime();
        var endOfDayUtc = endOfDayLocal.ToUniversalTime();

        return await DbSet
            .Include(fl => fl.Food)
            .Where(fl => fl.UserId == userId && fl.LoggedAt >= startOfDayUtc && fl.LoggedAt < endOfDayUtc)
            .OrderByDescending(fl => fl.LoggedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<FoodLog>> GetLatestByUserIdAsync(int userId, int count)
    {
        if (count <= 0)
        {
            return Array.Empty<FoodLog>();
        }

        return await DbSet
            .Include(fl => fl.Food)
            .Where(fl => fl.UserId == userId)
            .OrderByDescending(fl => fl.LoggedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<FoodLog>> GetByFoodIdAsync(int foodId) =>
        await DbSet
            .Where(fl => fl.FoodId == foodId)
            .ToListAsync();

    public async Task<IEnumerable<FoodLog>> GetByUserIdAndDateRangeAsync(int userId, DateTime from, DateTime to)
    {
        var fromUtc = DateTime.SpecifyKind(from.Date, DateTimeKind.Local).ToUniversalTime();
        var toUtc = DateTime.SpecifyKind(to.Date.AddDays(1), DateTimeKind.Local).ToUniversalTime();

        return await DbSet
            .Include(fl => fl.Food)
            .Where(fl => fl.UserId == userId && fl.LoggedAt >= fromUtc && fl.LoggedAt < toUtc)
            .OrderBy(fl => fl.LoggedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<FoodLog>> GetByMealIdAsync(int mealId) =>
        await DbSet
            .Include(fl => fl.Food)
            .Where(fl => fl.MealEntryId == mealId)
            .ToListAsync();
}
