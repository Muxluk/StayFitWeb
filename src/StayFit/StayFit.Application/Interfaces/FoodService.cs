using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StayFit.Application.Configuration;
using StayFit.Application.Interfaces;
using StayFit.Application.Options;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Application.Services;

public class FoodService : IFoodService
{
    private readonly IFoodRepository _foodRepository;
    private readonly IFoodLogRepository? _foodLogRepository;
    private readonly IUserRepository? _userRepository;
    private readonly INotificationService? _notificationService;
    private readonly INutritionGoalRepository? _nutritionGoalRepository;
    private readonly NotificationSettings _notificationSettings;
    private readonly ILogger<FoodService> _logger;

    public FoodService(
        IFoodRepository foodRepository,
        ILogger<FoodService> logger,
        IFoodLogRepository? foodLogRepository = null,
        IUserRepository? userRepository = null,
        INotificationService? notificationService = null,
        INutritionGoalRepository? nutritionGoalRepository = null,
        IOptions<NotificationSettings>? notificationSettings = null)
    {
        _foodRepository = foodRepository;
        _foodLogRepository = foodLogRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _nutritionGoalRepository = nutritionGoalRepository;
        _notificationSettings = notificationSettings?.Value ?? new NotificationSettings { CalorieThresholdPercent = 100 };
        _logger = logger;
    }

    public async Task<IEnumerable<Food>> GetAllFoodsAsync(int userId)
    {
        _logger.LogInformation("Отримання списку продуктів для користувача: {UserId}", userId);
        return await _foodRepository.GetAllByOwnerAsync(userId);
    }

    public async Task<Food?> GetFoodByIdAsync(int id, int userId)
    {
        _logger.LogInformation("Пошук продукту з ID: {Id} для користувача: {UserId}", id, userId);
        var food = await _foodRepository.GetByIdAndOwnerAsync(id, userId);

        if (food == null)
        {
            _logger.LogWarning("Продукт з ID: {Id} не знайдено для користувача: {UserId}.", id, userId);
        }

        return food;
    }

    public async Task AddFoodAsync(Food food, int userId)
    {
        if (food == null)
        {
            throw new ArgumentNullException(nameof(food));
        }

        food.OwnerUserId = userId;
        _logger.LogInformation("Додавання нового продукту: {Name}", food.Name);
        await _foodRepository.AddAsync(food);
    }

    public async Task UpdateFoodAsync(Food food, int userId)
    {
        if (food == null)
        {
            throw new ArgumentNullException(nameof(food));
        }

        if (food.OwnerUserId != userId)
        {
            _logger.LogWarning("Спроба оновити чужий продукт. FoodId: {FoodId}, UserId: {UserId}", food.Id, userId);
            throw new KeyNotFoundException($"Продукт з ID {food.Id} не знайдено.");
        }

        _logger.LogInformation("Оновлення продукту з ID: {Id}", food.Id);
        await _foodRepository.UpdateAsync(food);
    }

    public async Task DeleteFoodAsync(int id, int userId)
    {
        _logger.LogInformation("Видалення продукту з ID: {Id} для користувача: {UserId}", id, userId);
        var food = await _foodRepository.GetByIdAndOwnerAsync(id, userId);
        if (food != null)
        {
            // Delete associated FoodLogs first to avoid foreign key constraint violation
            if (_foodLogRepository != null)
            {
                var logs = await _foodLogRepository.GetByFoodIdAsync(id);
                foreach (var log in logs)
                {
                    await _foodLogRepository.DeleteAsync(log.Id);
                }

                _logger.LogInformation("Видалено {LogCount} записів логу для продукту {FoodId}", logs.Count(), id);
            }

            await _foodRepository.DeleteAsync(id);
        }
        else
        {
            _logger.LogWarning("Спроба видалити неіснуючий продукт з ID: {Id}", id);
            throw new KeyNotFoundException($"Продукт з ID {id} не знайдено.");
        }
    }

    public async Task<IEnumerable<FoodLog>> GetDailyLogAsync(string userEmail, DateTime date)
    {
        EnsureFoodLogDependencies();

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            _logger.LogWarning("Спроба отримати щоденний лог без email користувача");
            return Enumerable.Empty<FoodLog>();
        }

        var user = await _userRepository!.GetByEmailAsync(userEmail);
        if (user == null)
        {
            _logger.LogWarning("Користувача з email {Email} не знайдено, буде створено запис", userEmail);

            // Lazy creation: create user on-the-fly for legacy users
            user = new User
            {
                Email = userEmail,
                Name = userEmail,
                CreatedAt = DateTime.UtcNow,
            };

            await _userRepository!.AddAsync(user);
            _logger.LogInformation("Запис користувача з email {Email} створено з ID {UserId}", userEmail, user.Id);
        }

        _logger.LogInformation("Отримання щоденного логу для користувача: {UserId}, дата: {Date}", user.Id, date.Date);
        return await _foodLogRepository!.GetByUserIdAndDateAsync(user.Id, date.Date);
    }

    public async Task AddFoodToLogAsync(int foodId, double quantity, string userEmail)
    {
        EnsureFoodLogDependencies();

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            throw new ArgumentNullException(nameof(userEmail));
        }

        var user = await _userRepository!.GetByEmailAsync(userEmail);

        if (user == null)
        {
            user = new User
            {
                Email = userEmail,
                Name = userEmail,
                CreatedAt = DateTime.UtcNow,
            };

            await _userRepository.AddAsync(user);
            _logger.LogInformation("Новий користувач створено: {Email}, ID: {UserId}", userEmail, user.Id);
        }

        var log = new FoodLog
        {
            UserId = user.Id,
            FoodId = foodId,
            AmountGrams = (float)quantity,
            Quantity = quantity,
            LoggedAt = DateTime.UtcNow,
            UserEmail = userEmail,
        };

        _logger.LogInformation("Додавання запису в лог їжі. UserId: {UserId}, FoodId: {FoodId}", log.UserId, log.FoodId);
        await _foodLogRepository!.AddAsync(log);
        _logger.LogInformation("✅ Запис в БД успішно додано. LogId буде присвоєно БД");

        // Отримати назву продукту для сповіщення
        _logger.LogInformation("🔍 Отримання інформації про продукт FoodId={FoodId}", foodId);
        var food = await _foodRepository.GetByIdAsync(foodId);
        var foodName = food?.Name ?? "Продукт";
        _logger.LogInformation("📝 Назва продукту: {FoodName}", foodName);

        // Створити сповіщення про додавання продукту
        _logger.LogInformation("📢 Спроба створити сповіщення FoodAdded. UserId={UserId}, FoodName={FoodName}, Quantity={Quantity}g", user.Id, foodName, quantity);
        if (_notificationService == null)
        {
            _logger.LogError("❌ ПОМИЛКА: _notificationService == null!");
        }
        else
        {
            var notificationResult = await _notificationService.CreateFoodAddedNotificationAsync(user.Id, foodName, quantity);
            if (notificationResult.IsSuccess)
            {
                _logger.LogInformation("✅ Сповіщення FoodAdded успішно створено");
            }
            else
            {
                _logger.LogError("❌ Помилка при створенні сповіщення: {Errors}", string.Join(", ", notificationResult.Errors));
            }
        }

        // Перевірка перевищення норми калорій та генерація сповіщення
        _logger.LogInformation("🔍 Перевірка перевищення норми калорій");
        await CheckAndNotifyCalorieThresholdAsync(user.Id);
        _logger.LogInformation("✅ Перевірка калорій завершена");
    }

    public async Task<bool> QuickAddFoodLogEntryAsync(int logId, string userEmail)
    {
        EnsureFoodLogDependencies();

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            throw new ArgumentNullException(nameof(userEmail));
        }

        var user = await _userRepository!.GetByEmailAsync(userEmail);
        if (user == null)
        {
            user = new User
            {
                Email = userEmail,
                Name = userEmail,
                CreatedAt = DateTime.UtcNow,
            };

            await _userRepository!.AddAsync(user);
            _logger.LogInformation("Новий користувач створено: {Email}, ID: {UserId}", userEmail, user.Id);
        }

        var existingLog = await _foodLogRepository!.GetByIdAsync(logId);
        if (existingLog == null || existingLog.UserId != user.Id)
        {
            _logger.LogWarning("Запис логу {LogId} не знайдено або він не належить користувачу {Email}", logId, userEmail);
            return false;
        }

        await AddFoodToLogAsync(existingLog.FoodId, existingLog.Quantity, userEmail);
        _logger.LogInformation("Швидке додавання виконано для log {LogId} користувача {Email}", logId, userEmail);
        return true;
    }

    public async Task RemoveFromLogAsync(int logId)
    {
        EnsureFoodLogDependencies();

        _logger.LogInformation("Видалення запису логу їжі з ID: {LogId}", logId);
        await _foodLogRepository!.DeleteAsync(logId);
    }

    private void EnsureFoodLogDependencies()
    {
        if (_foodLogRepository == null || _userRepository == null)
        {
            throw new InvalidOperationException(
                "Food log operations require IFoodLogRepository and IUserRepository dependencies.");
        }
    }

    private async Task CheckAndNotifyCalorieThresholdAsync(int userId)
    {
        try
        {
            _logger.LogInformation("🔍 Початок перевірки перевищення калорій для користувача {UserId}", userId);

            // Перевірка, чи активована автоматична генерація сповіщень
            if (_nutritionGoalRepository == null || _notificationService == null || _foodLogRepository == null || _userRepository == null)
            {
                _logger.LogWarning("⚠️ Сповіщення про калорії НЕ активовані - залежності відсутні");
                return;
            }

            // Отримати користувача для отримання email
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("❌ Користувач {UserId} не знайдено", userId);
                return;
            }

            _logger.LogInformation("👤 Користувач знайдено: {Email}", user.Email);

            // Отримати норму калорій користувача
            var goal = await _nutritionGoalRepository.GetByUserIdAsync(user.Email);
            if (goal == null)
            {
                _logger.LogWarning("⚠️ Норма калорій НЕ встановлена для користувача {UserId}. Сповіщення не пересилаються!", userId);
                return;
            }

            _logger.LogInformation("📊 Норма калорій: {CaloriesGoal} ккал", goal.CaloriesGoal);

            // Отримати сумарні калорії за допис
            var todayLogs = await _foodLogRepository!.GetByUserIdAndDateAsync(userId, DateTime.Today);
            var totalCalories = todayLogs.Sum(log => (log.Food?.CaloriesPer100g ?? 0) * log.AmountGrams / 100);

            // Обчислити поріг (за замовчуванням 100% = норма)
            var threshold = (decimal)goal.CaloriesGoal * (_notificationSettings.CalorieThresholdPercent / 100m);

            _logger.LogInformation(
                "📈 Перевірка калорій: {TotalCalories:F0} ккал / {Threshold:F0} ккал (поріг {Percent}%)",
                totalCalories, threshold, _notificationSettings.CalorieThresholdPercent);

            // Якщо перевищено норму - генерувати сповіщення
            if (totalCalories > (double)threshold)
            {
                var overage = (decimal)totalCalories - threshold;
                _logger.LogInformation("🚀 Перевищення виявлено на {Overage:F0} ккал. Генеруємо сповіщення...", overage);
                
                var result = await _notificationService.CreateCalorieThresholdNotificationAsync(userId, overage);

                if (result.IsSuccess)
                {
                    _logger.LogInformation(
                        "✅ Сповіщення про перевищення калорій успішно створено для користувача {UserId} (+{Overage:F0} ккал)",
                        userId, overage);
                }
                else
                {
                    _logger.LogWarning(
                        "❌ Помилка при генерації сповіщення для користувача {UserId}: {Error}",
                        userId, string.Join(", ", result.Errors));
                }
            }
            else
            {
                _logger.LogInformation("✓ Норма калорій не перевищена. Сповіщення не потрібне.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Помилка при перевірці перевищення калорій для користувача {UserId}", userId);
            // Не вихлюпуємо помилку - це не критично для основної операції
        }
    }
}

