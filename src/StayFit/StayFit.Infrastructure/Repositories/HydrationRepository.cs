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
        // Ensure we query using UTC timestamps because PostgreSQL 'timestamp with time zone' expects UTC
        DateTime startLocal = date.Date;
        DateTime endLocal = startLocal.AddDays(1).AddTicks(-1);

        DateTime startUtc = date.Kind == DateTimeKind.Utc
            ? startLocal  // already UTC
            : DateTime.SpecifyKind(startLocal, DateTimeKind.Local).ToUniversalTime();

        DateTime endUtc = date.Kind == DateTimeKind.Utc
            ? endLocal
            : DateTime.SpecifyKind(endLocal, DateTimeKind.Local).ToUniversalTime();

        return await _context.WaterLogs
            .Where(l => l.UserId == userId && l.LoggedAt >= startUtc && l.LoggedAt <= endUtc)
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
        // Convert provided dates to UTC range (inclusive of endDate)
        DateTime startLocal = startDate.Date;
        DateTime endLocalExclusive = endDate.Date.AddDays(1).AddTicks(-1);

        DateTime startUtc = startDate.Kind == DateTimeKind.Utc
            ? startLocal
            : DateTime.SpecifyKind(startLocal, DateTimeKind.Local).ToUniversalTime();

        DateTime endUtc = endDate.Kind == DateTimeKind.Utc
            ? endLocalExclusive
            : DateTime.SpecifyKind(endLocalExclusive, DateTimeKind.Local).ToUniversalTime();

        return await _context.WaterLogs
            .Where(l => l.UserId == userId && l.LoggedAt >= startUtc && l.LoggedAt <= endUtc)
            .OrderBy(l => l.LoggedAt)
            .ToListAsync();
    }
    
}