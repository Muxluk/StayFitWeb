using Microsoft.EntityFrameworkCore;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Infrastructure.Data;

namespace StayFit.Infrastructure.Repositories;

/// <summary>
/// Repository для журналу безпеки акаунта
/// </summary>
public class SecurityLogRepository : ISecurityLogRepository
{
    private readonly AppDbContext _context;

    public SecurityLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SecurityLogEntry> AddLogEntryAsync(SecurityLogEntry entry)
    {
        await _context.SecurityLogs.AddAsync(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task<(IEnumerable<SecurityLogEntry> entries, int totalCount)> GetUserLogsAsync(
        int userId, 
        int pageNumber, 
        int pageSize)
    {
        var query = _context.SecurityLogs
            .Where(log => log.UserId == userId)
            .OrderByDescending(log => log.CreatedAt);

        var totalCount = await query.CountAsync();
        
        var entries = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (entries, totalCount);
    }

    public async Task<IEnumerable<SecurityLogEntry>> GetRecentLogsAsync(int userId, int count)
    {
        return await _context.SecurityLogs
            .Where(log => log.UserId == userId)
            .OrderByDescending(log => log.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<SecurityLogEntry>> GetLogsByDateRangeAsync(
        int userId, 
        DateTime startDate, 
        DateTime endDate)
    {
        return await _context.SecurityLogs
            .Where(log => log.UserId == userId &&
                         log.CreatedAt >= startDate &&
                         log.CreatedAt <= endDate)
            .OrderByDescending(log => log.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> DeleteOldLogsAsync(int retentionDays)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
        
        var entriesToDelete = await _context.SecurityLogs
            .Where(log => log.CreatedAt < cutoffDate)
            .ToListAsync();

        _context.SecurityLogs.RemoveRange(entriesToDelete);
        return await _context.SaveChangesAsync();
    }
}
