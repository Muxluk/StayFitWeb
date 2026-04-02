using Microsoft.EntityFrameworkCore;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Data;

namespace StayFit.Infrastructure.Repositories;

public class MealRepository : Repository<MealEntry>, IMealRepository
{
    private readonly AppDbContext _context;

    public MealRepository(AppDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MealEntry>> GetMealsWithFoodsAsync(string userEmail, DateTime date)
    {
        var startOfDayLocal = DateTime.SpecifyKind(date.Date, DateTimeKind.Local);
        var startOfDayUtc = startOfDayLocal.ToUniversalTime();
        var endOfDayUtc = startOfDayUtc.AddDays(1);

        return await _context.Set<MealEntry>()
            .Include(m => m.FoodLogs)
                .ThenInclude(fl => fl.Food)
            .Where(m => m.UserEmail == userEmail && m.Time >= startOfDayUtc && m.Time < endOfDayUtc)
            .OrderBy(m => m.Time)
            .ToListAsync();
    }

    public async Task<IEnumerable<MealEntry>> GetMealsForDateRangeAsync(string userEmail, DateTime startDate, DateTime endDate)
    {
        var startOfDayLocal = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Local);
        var startOfDayUtc = startOfDayLocal.ToUniversalTime();
        
        var endOfDayLocal = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Local);
        var endOfDayUtc = endOfDayLocal.ToUniversalTime().AddDays(1);

        return await _context.Set<MealEntry>()
            .Include(m => m.FoodLogs)
                .ThenInclude(fl => fl.Food)
            .Where(m => m.UserEmail == userEmail && m.Time >= startOfDayUtc && m.Time < endOfDayUtc)
            .OrderBy(m => m.Time)
            .ToListAsync();
    }
}