using Microsoft.EntityFrameworkCore;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Infrastructure.Data;

namespace StayFit.Infrastructure.Repositories;

public class SystemStatisticsRepository : ISystemStatisticsRepository
{
    private readonly AppDbContext _context;

    public SystemStatisticsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SystemStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        
        var totalProducts = await _context.Set<Food>().CountAsync(cancellationToken);
        
        var mealEntriesCount = await _context.Set<MealEntry>().CountAsync(cancellationToken);
        var foodLogsCount = await _context.Set<FoodLog>().CountAsync(cancellationToken);
        
        var activeSessions = await _context.Set<UserSession>()
            .CountAsync(s => s.IsActive && s.ExpiresAt > DateTime.UtcNow, cancellationToken);

        return new SystemStatisticsDto
        {
            TotalUsers = totalUsers,
            TotalProducts = totalProducts,
            TotalDiaryEntries = mealEntriesCount + foodLogsCount,
            ActiveSessions = activeSessions
        };
    }
}