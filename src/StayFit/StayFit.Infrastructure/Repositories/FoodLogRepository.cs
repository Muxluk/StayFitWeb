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
        // Convert local date boundaries to UTC
        // DateTime.Today in Local -> convert to UTC range for that calendar day
        var localDate = date.Date; // Get just the date part (00:00:00)
        
        // Start of day in local time, then convert to UTC
        var startOfDayLocal = localDate;
        var startOfDayUtc = startOfDayLocal.Kind == DateTimeKind.Local 
            ? startOfDayLocal.ToUniversalTime() 
            : startOfDayLocal;
        
        // End of day in local time, then convert to UTC
        var endOfDayLocal = localDate.AddDays(1);
        var endOfDayUtc = endOfDayLocal.Kind == DateTimeKind.Local 
            ? endOfDayLocal.ToUniversalTime() 
            : endOfDayLocal;

        return await DbSet
            .Include(fl => fl.Food)
            .Where(fl => fl.UserId == userId && fl.LoggedAt >= startOfDayUtc && fl.LoggedAt < endOfDayUtc)
            .OrderByDescending(fl => fl.LoggedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<FoodLog>> GetByFoodIdAsync(int foodId) =>
        await DbSet
            .Where(fl => fl.FoodId == foodId)
            .ToListAsync();
}
