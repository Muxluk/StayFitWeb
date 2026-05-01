using Microsoft.EntityFrameworkCore;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Data;

namespace StayFit.Infrastructure.Repositories;

public class HydrationRepository : IHydrationRepository
{
    private readonly AppDbContext _context;

    public HydrationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<HydrationGoal?> GetGoalByUserIdAsync(int userId)
    {
        return await _context.HydrationGoals.FirstOrDefaultAsync(g => g.UserId == userId);
    }

    public async Task UpdateGoalAsync(HydrationGoal goal)
    {
        _context.HydrationGoals.Update(goal);
        await _context.SaveChangesAsync();
    }

    public async Task AddGoalAsync(HydrationGoal goal)
    {
        await _context.HydrationGoals.AddAsync(goal);
        await _context.SaveChangesAsync();
    }

    public async Task AddWaterLogAsync(WaterLog log)
    {
        await _context.WaterLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<WaterLog>> GetLogsForDateAsync(int userId, DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        return await _context.WaterLogs
            .Where(l => l.UserId == userId && l.LoggedAt >= startOfDay && l.LoggedAt <= endOfDay)
            .OrderByDescending(l => l.LoggedAt)
            .ToListAsync();
    }
    public async Task<int> GetTotalVolumeForDateAsync(int userId, DateTime date)
    {
        var logs = await GetLogsForDateAsync(userId, date);
        return logs.Sum(l => l.VolumeMl);
    }

    public async Task<IEnumerable<WaterLog>> GetLogsForPeriodAsync(int userId, DateTime startDate, DateTime endDate)
    {
        return await _context.WaterLogs
            .Where(l => l.UserId == userId && l.LoggedAt.Date >= startDate.Date && l.LoggedAt.Date <= endDate.Date)
            .OrderBy(l => l.LoggedAt)
            .ToListAsync();
    }
    
}