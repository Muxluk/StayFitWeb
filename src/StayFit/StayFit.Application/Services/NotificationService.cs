using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StayFit.Application.Common;
using StayFit.Application.Interfaces;
using StayFit.Application.Options;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Application.Services;

/// <summary>
/// Сервіс для роботи з системою сповіщень
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IOptions<NotificationSettings> _notificationSettings;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        IOptions<NotificationSettings> notificationSettings,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _notificationSettings = notificationSettings;
        _logger = logger;
    }

    /// <summary>
    /// Отримати всі непрочитані сповіщення користувача
    /// </summary>
    public async Task<Result<IEnumerable<NotificationDto>>> GetUnreadNotificationsAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Отримання непрочитаних сповіщень для користувача {UserId}", userId);

            var notifications = await _notificationRepository.GetUnreadNotificationsAsync(userId);
            var dtos = notifications.Select(MapToDto);

            _logger.LogInformation("Отримано {Count} непрочитаних сповіщень для користувача {UserId}",
                dtos.Count(), userId);

            return Result<IEnumerable<NotificationDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні непрочитаних сповіщень для користувача {UserId}", userId);
            return Result<IEnumerable<NotificationDto>>.Failure("Помилка при отриманні сповіщень");
        }
    }

    /// <summary>
    /// Отримати кількість непрочитаних сповіщень
    /// </summary>
    public async Task<Result<int>> GetUnreadCountAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Отримання кількості непрочитаних сповіщень для користувача {UserId}", userId);

            var count = await _notificationRepository.GetUnreadCountAsync(userId);

            _logger.LogInformation("Кількість непрочитаних сповіщень для користувача {UserId}: {Count}",
                userId, count);

            return Result<int>.Success(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні кількості непрочитаних сповіщень для користувача {UserId}", userId);
            return Result<int>.Failure("Помилка при отриманні кількості сповіщень");
        }
    }

    /// <summary>
    /// Позначити сповіщення як прочитане
    /// </summary>
    public async Task<Result> MarkAsReadAsync(int notificationId, int userId)
    {
        try
        {
            _logger.LogInformation("Позначення сповіщення {NotificationId} як прочитаного для користувача {UserId}",
                notificationId, userId);

            var notification = await _notificationRepository.GetByIdAsync(notificationId);

            if (notification == null)
            {
                _logger.LogWarning("Сповіщення {NotificationId} не знайдено", notificationId);
                return Result.Failure("Сповіщення не знайдено");
            }

            if (notification.UserId != userId)
            {
                _logger.LogWarning("Користувач {UserId} намагається отримати доступ до сповіщення {NotificationId}, " +
                    "яке належить користувачу {NotificationUserId}", userId, notificationId, notification.UserId);
                return Result.Failure("Вам не дозволено змінювати це сповіщення");
            }

            if (notification.IsRead)
            {
                _logger.LogInformation("Сповіщення {NotificationId} вже позначено як прочитане", notificationId);
                return Result.Success();
            }

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            await _notificationRepository.UpdateAsync(notification);

            _logger.LogInformation("Сповіщення {NotificationId} успішно позначено як прочитане", notificationId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при позначенні сповіщення {NotificationId} як прочитаного", notificationId);
            return Result.Failure("Помилка при позначенні сповіщення як прочитаного");
        }
    }

    /// <summary>
    /// Позначити всі сповіщення користувача як прочитані
    /// </summary>
    public async Task<Result> MarkAllAsReadAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Позначення всіх сповіщень користувача {UserId} як прочитаних", userId);

            var unreadNotifications = await _notificationRepository.GetUnreadNotificationsAsync(userId);

            if (!unreadNotifications.Any())
            {
                _logger.LogInformation("Користувач {UserId} не має непрочитаних сповіщень", userId);
                return Result.Success();
            }

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _notificationRepository.UpdateAsync(notification);
            }

            _logger.LogInformation("Всі {Count} сповіщень користувача {UserId} успішно позначені як прочитані",
                unreadNotifications.Count(), userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при позначенні всіх сповіщень користувача {UserId} як прочитаних", userId);
            return Result.Failure("Помилка при позначенні всіх сповіщень як прочитаних");
        }
    }

    /// <summary>
    /// Очистити всі сповіщення користувача
    /// </summary>
    public async Task<Result> ClearAllNotificationsAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Очищення всіх сповіщень для користувача {UserId}", userId);

            await _notificationRepository.DeleteAllByUserIdAsync(userId);

            _logger.LogInformation("Всі сповіщення користувача {UserId} успішно очищені", userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при очищенні всіх сповіщень для користувача {UserId}", userId);
            return Result.Failure("Помилка при очищенні сповіщень");
        }
    }

    /// <summary>
    /// Створити сповіщення про додавання продукту до щоденника
    /// </summary>
    public async Task<Result> CreateFoodAddedNotificationAsync(int userId, string foodName, double grams)
    {
        try
        {
            _logger.LogInformation(
                "Створення сповіщення про додавання продукту для користувача {UserId}. Продукт: {FoodName}, Кількість: {Grams}g",
                userId, foodName, grams);

            var notification = new Notification
            {
                UserId = userId,
                Title = "🍽️ Продукт додано",
                Message = $"Ви додали {foodName} ({grams:F0}g) до щоденника",
                Type = "FoodAdded",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(notification);

            _logger.LogInformation(
                "Сповіщення про додавання продукту успішно створено для користувача {UserId}", userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Помилка при створенні сповіщення про додавання продукту для користувача {UserId}", userId);
            return Result.Failure("Помилка при створенні сповіщення");
        }
    }

    /// <summary>
    /// Створити сповіщення про видалення запису зі щоденника
    /// </summary>
    public async Task<Result> CreateFoodRemovedNotificationAsync(int userId, string foodName)
    {
        try
        {
            _logger.LogInformation(
                "Створення сповіщення про видалення продукту для користувача {UserId}. Продукт: {FoodName}",
                userId, foodName);

            var notification = new Notification
            {
                UserId = userId,
                Title = "🗑️ Продукт видалено",
                Message = $"Продукт {foodName} видалено зі щоденника",
                Type = "FoodRemoved",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(notification);

            _logger.LogInformation(
                "Сповіщення про видалення продукту успішно створено для користувача {UserId}", userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Помилка при створенні сповіщення про видалення продукту для користувача {UserId}", userId);
            return Result.Failure("Помилка при створенні сповіщення");
        }
    }

    /// <summary>
    /// Створити сповіщення про встановлення цілі калорій
    /// </summary>
    public async Task<Result> CreateNutritionGoalSetNotificationAsync(int userId, int calorieGoal)
    {
        try
        {
            _logger.LogInformation(
                "Створення сповіщення про встановлення цілі калорій для користувача {UserId}. Ціль: {CalorieGoal}",
                userId, calorieGoal);

            var notification = new Notification
            {
                UserId = userId,
                Title = "🎯 Ціль встановлена",
                Message = $"Ви встановили денну ціль: {calorieGoal} ккал",
                Type = "GoalSet",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(notification);

            _logger.LogInformation(
                "Сповіщення про встановлення цілі успішно створено для користувача {UserId}", userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Помилка при створенні сповіщення про встановлення цілі для користувача {UserId}", userId);
            return Result.Failure("Помилка при створенні сповіщення");
        }
    }

    /// <summary>
    /// Створити сповіщення про перевищення калорій
    /// </summary>
    public async Task<Result> CreateCalorieThresholdNotificationAsync(int userId, decimal calorieOverage)
    {
        try
        {
            var threshold = _notificationSettings.Value.CalorieThresholdPercent;

            // Не дублювати сповіщення — одне на день
            var alreadySent = await _notificationRepository.HasCalorieThresholdNotificationTodayAsync(userId);
            if (alreadySent)
            {
                _logger.LogInformation(
                    "Сповіщення про перевищення калорій вже надіслано сьогодні для користувача {UserId}", userId);
                return Result.Success();
            }

            _logger.LogInformation(
                "Створення сповіщення про перевищення калорій для користувача {UserId}. " +
                "Поріг: {Threshold}%, перевищено на: {CalorieOverage} ккал",
                userId, threshold, calorieOverage);

            var notification = new Notification
            {
                UserId = userId,
                Title = "Перевищення денної норми калорій",
                Message = $"Ви перевищили денну норму калорій на {calorieOverage:F0} ккал ({threshold}%)",
                Type = "CalorieThreshold",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(notification);

            _logger.LogInformation(
                "Сповіщення про перевищення калорій успішно створено для користувача {UserId}", userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Помилка при створенні сповіщення про перевищення калорій для користувача {UserId}", userId);
            return Result.Failure("Помилка при створенні сповіщення");
        }
    }

    /// <summary>
    /// Мапінг Notification до NotificationDto
    /// </summary>
    private NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            Title = NormalizeLegacyRussianText(notification.Title),
            Message = NormalizeLegacyRussianText(notification.Message),
            Type = notification.Type,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
    }

    private static string NormalizeLegacyRussianText(string text)
    {
        return text
            .Replace("Продукт удален", "Продукт видалено")
            .Replace("Продукт добавлен", "Продукт додано")
            .Replace("Цель установлена", "Ціль встановлена")
            .Replace("Вы добавили", "Ви додали")
            .Replace("Вы установили", "Ви встановили")
            .Replace("удален из дневника", "видалено зі щоденника")
            .Replace("в дневник", "до щоденника")
            .Replace("дневную цель", "денну ціль");
    }
}
