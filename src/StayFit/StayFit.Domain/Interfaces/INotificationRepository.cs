using StayFit.Domain.Entities;

namespace StayFit.Domain.Interfaces;

/// <summary>
/// Репозиторій для роботи з сповіщеннями
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Отримати всі непрочитані сповіщення користувача
    /// </summary>
    Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(int userId);

    /// <summary>
    /// Отримати кількість непрочитаних сповіщень користувача
    /// </summary>
    Task<int> GetUnreadCountAsync(int userId);

    /// <summary>
    /// Отримати сповіщення за ID
    /// </summary>
    Task<Notification?> GetByIdAsync(int id);

    /// <summary>
    /// Додати нове сповіщення
    /// </summary>
    Task AddAsync(Notification notification);

    /// <summary>
    /// Оновити сповіщення
    /// </summary>
    Task UpdateAsync(Notification notification);

    /// <summary>
    /// Отримати всі сповіщення користувача (включаючи прочитані)
    /// </summary>
    Task<IEnumerable<Notification>> GetAllNotificationsAsync(int userId);

    /// <summary>
    /// Видалити всі сповіщення користувача
    /// </summary>
    Task DeleteAllByUserIdAsync(int userId);

    /// <summary>
    /// Видалити сповіщення за ID
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Перевіряє, чи вже існує CalorieThreshold-сповіщення для користувача за сьогодні
    /// </summary>
    Task<bool> HasCalorieThresholdNotificationTodayAsync(int userId);
}
