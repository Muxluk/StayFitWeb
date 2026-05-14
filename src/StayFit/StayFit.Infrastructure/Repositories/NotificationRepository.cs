using Microsoft.EntityFrameworkCore;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Data;

namespace StayFit.Infrastructure.Repositories;

/// <summary>
/// Репозиторій для роботи з сповіщеннями
/// </summary>
public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(AppDbContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Отримати всі непрочитані сповіщення користувача
    /// </summary>
    public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(int userId)
    {
        return await DbSet
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Отримати кількість непрочитаних сповіщень користувача
    /// </summary>
    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await DbSet.CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    /// <summary>
    /// Отримати всі сповіщення користувача (включаючи прочитані)
    /// </summary>
    public async Task<IEnumerable<Notification>> GetAllNotificationsAsync(int userId)
    {
        return await DbSet
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Видалити всі сповіщення користувача
    /// </summary>
    public async Task DeleteAllByUserIdAsync(int userId)
    {
        var notifications = await DbSet.Where(n => n.UserId == userId).ToListAsync();
        DbSet.RemoveRange(notifications);
        await Context.SaveChangesAsync();
    }

    public async Task<bool> HasCalorieThresholdNotificationTodayAsync(int userId)
    {
        var today = DateTime.UtcNow.Date;
        return await DbSet.AnyAsync(n =>
            n.UserId == userId &&
            n.Type == "CalorieThreshold" &&
            n.CreatedAt >= today);
    }
}
