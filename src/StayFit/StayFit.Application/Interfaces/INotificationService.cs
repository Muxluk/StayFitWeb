using StayFit.Application.Common;

namespace StayFit.Application.Interfaces;

/// <summary>
/// Інтерфейс для роботи з системою сповіщень
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Отримати всі непрочитані сповіщення користувача
    /// </summary>
    /// <param name="userId">ID користувача</param>
    /// <returns>Список DTO непрочитаних сповіщень</returns>
    Task<Result<IEnumerable<NotificationDto>>> GetUnreadNotificationsAsync(int userId);

    /// <summary>
    /// Отримати кількість непрочитаних сповіщень
    /// </summary>
    /// <param name="userId">ID користувача</param>
    /// <returns>Кількість непрочитаних сповіщень</returns>
    Task<Result<int>> GetUnreadCountAsync(int userId);

    /// <summary>
    /// Позначити сповіщення як прочитане
    /// </summary>
    /// <param name="notificationId">ID сповіщення</param>
    /// <param name="userId">ID користувача (для перевірки доступу)</param>
    /// <returns>Результат операції</returns>
    Task<Result> MarkAsReadAsync(int notificationId, int userId);

    /// <summary>
    /// Позначити всі сповіщення користувача як прочитані
    /// </summary>
    /// <param name="userId">ID користувача</param>
    /// <returns>Результат операції</returns>
    Task<Result> MarkAllAsReadAsync(int userId);

    /// <summary>
    /// Очистити всі сповіщення користувача
    /// </summary>
    /// <param name="userId">ID користувача</param>
    /// <returns>Результат операції</returns>
    Task<Result> ClearAllNotificationsAsync(int userId);

    /// <summary>
    /// Створити сповіщення про перевищення калорій
    /// </summary>
    /// <param name="userId">ID користувача</param>
    /// <param name="calorieOverage">На скільки калорій було перевищено норму</param>
    /// <returns>Результат операції</returns>
    Task<Result> CreateCalorieThresholdNotificationAsync(int userId, decimal calorieOverage);

    /// <summary>
    /// Створити сповіщення про додавання продукту до щоденника
    /// </summary>
    /// <param name="userId">ID користувача</param>
    /// <param name="foodName">Назва продукту</param>
    /// <param name="grams">Кількість грамів</param>
    /// <returns>Результат операції</returns>
    Task<Result> CreateFoodAddedNotificationAsync(int userId, string foodName, double grams);

    /// <summary>
    /// Створити сповіщення про видалення продукту зі щоденника
    /// </summary>
    /// <param name="userId">ID користувача</param>
    /// <param name="foodName">Назва продукту</param>
    /// <returns>Результат операції</returns>
    Task<Result> CreateFoodRemovedNotificationAsync(int userId, string foodName);

    /// <summary>
    /// Створити сповіщення про встановлення цілі калорій
    /// </summary>
    /// <param name="userId">ID користувача</param>
    /// <param name="calorieGoal">Цільова кількість калорій</param>
    /// <returns>Результат операції</returns>
    Task<Result> CreateNutritionGoalSetNotificationAsync(int userId, int calorieGoal);

    /// <summary>
    /// Надіслати адміністраторам сповіщення про нове звернення до техпідтримки
    /// </summary>
    /// <param name="ticketId">ID нового тікету</param>
    /// <param name="subject">Тема звернення</param>
    Task<Result> NotifyAdminsNewSupportTicketAsync(int ticketId, string subject);

    /// <summary>
    /// Надіслати адміністраторам сповіщення про новий продукт на модерацію
    /// </summary>
    /// <param name="foodName">Назва продукту</param>
    Task<Result> NotifyAdminsNewFoodModerationAsync(string foodName);
}

/// <summary>
/// DTO для сповіщення
/// </summary>
public class NotificationDto
{
    /// <summary>
    /// ID сповіщення
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Заголовок сповіщення
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Опис сповіщення
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Тип сповіщення
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Чи прочитано сповіщення
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Дата створення
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
