using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StayFit.Application.Options;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Application.Services;

public class QuickAddService
{
    private readonly IMealRepository _mealRepository;
    private readonly IFoodLogRepository _foodLogRepository;
    private readonly IUserRepository _userRepository;
    private readonly INutritionGoalRepository _nutritionGoalRepository;
    private readonly INotificationService _notificationService;
    private readonly IOptions<NotificationSettings> _notificationSettings;
    private readonly ILogger<QuickAddService> _logger;

    public QuickAddService(
        IMealRepository mealRepository,
        IFoodLogRepository foodLogRepository,
        IUserRepository userRepository,
        INutritionGoalRepository nutritionGoalRepository,
        INotificationService notificationService,
        IOptions<NotificationSettings> notificationSettings,
        ILogger<QuickAddService> logger)
    {
        _mealRepository = mealRepository;
        _foodLogRepository = foodLogRepository;
        _userRepository = userRepository;
        _nutritionGoalRepository = nutritionGoalRepository;
        _notificationService = notificationService;
        _notificationSettings = notificationSettings;
        _logger = logger;
    }

    /// <summary>
    /// Quickly adds the same food log entry from an existing meal to today's date.
    /// Creates a new meal with the same name on the current date and copies all food logs.
    /// </summary>
    /// <param name="mealId">ID of the meal to copy from</param>
    /// <param name="userEmail">Email of the current user</param>
    /// <returns>True if successful, false otherwise</returns>
    public async Task<bool> QuickAddMealTodayAsync(int mealId, string userEmail)
    {
        try
        {
            _logger.LogInformation("Starting quick add for meal {MealId} for user {UserEmail}", mealId, userEmail);

            // Get the existing meal with its food logs
            var existingMeal = await _mealRepository.GetByIdAsync(mealId);
            if (existingMeal == null || existingMeal.UserEmail != userEmail)
            {
                _logger.LogWarning("Meal {MealId} not found or doesn't belong to user {UserEmail}", mealId, userEmail);
                return false;
            }

            // Get food logs for this meal
            var foodLogs = await _foodLogRepository.GetByMealIdAsync(mealId);
            if (!foodLogs.Any())
            {
                _logger.LogWarning("No food logs found for meal {MealId}", mealId);
                return false;
            }

            // Create new meal for today
            var todayUtc = DateTime.UtcNow;
            var newMeal = new MealEntry
            {
                Name = existingMeal.Name,
                UserEmail = userEmail,
                Time = todayUtc,
                Note = existingMeal.Note // Copy note as well if it exists
            };

            await _mealRepository.AddAsync(newMeal);
            _logger.LogInformation("Created new meal {NewMealId} for user {UserEmail} on {Date}", 
                newMeal.Id, userEmail, todayUtc.Date);

            // Copy all food logs to the new meal
            var copiedLogsCount = 0;
            var totalCaloriesAdded = 0.0;

            foreach (var log in foodLogs)
            {
                var newLog = new FoodLog
                {
                    MealEntryId = newMeal.Id,
                    FoodId = log.FoodId,
                    UserId = log.UserId,
                    AmountGrams = log.AmountGrams,
                    LoggedAt = todayUtc
                };

                await _foodLogRepository.AddAsync(newLog);
                copiedLogsCount++;

                // Для уведомлений: собираем информацию о добавляемых продуктах
                var calories = (log.Food?.CaloriesPer100g ?? 0) * log.AmountGrams / 100;
                totalCaloriesAdded += calories;
            }

            _logger.LogInformation("Successfully copied {LogCount} food logs from meal {SourceMealId} to {NewMealId}",
                copiedLogsCount, mealId, newMeal.Id);

            // Отримати ID користувача для сповіщення
            var user = await _userRepository.GetByEmailAsync(userEmail);
            if (user != null)
            {
                // Створити сповіщення про додавання прийому їжі
                await _notificationService.CreateFoodAddedNotificationAsync(
                    user.Id,
                    $"Повторный прием: {existingMeal.Name}", 
                    totalCaloriesAdded
                );
            }

            // Check and notify if calorie threshold exceeded
            await CheckAndNotifyCalorieThresholdAsync(userEmail);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during quick add for meal {MealId} and user {UserEmail}", mealId, userEmail);
            return false;
        }
    }

    /// <summary>
    /// Checks if the user's daily calorie intake exceeds the threshold and creates a notification.
    /// </summary>
    private async Task CheckAndNotifyCalorieThresholdAsync(string userEmail)
    {
        try
        {
            _logger.LogInformation("🔍 Початок перевірки перевищення калорій для користувача {UserEmail}", userEmail);

            var user = await _userRepository.GetByEmailAsync(userEmail);
            if (user == null)
            {
                _logger.LogWarning("⚠️ Користувача {UserEmail} не знайдено", userEmail);
                return;
            }

            _logger.LogInformation("👤 Користувача {UserEmail} знайдено з ID {UserId}", user.Email, user.Id);

            var today = DateTime.UtcNow.Date;
            var nutritionGoal = await _nutritionGoalRepository.GetByUserIdAsync(userEmail);

            if (nutritionGoal == null)
            {
                _logger.LogWarning("⚠️ Норма калорій НЕ встановлена для користувача {UserEmail}", userEmail);
                return;
            }

            _logger.LogInformation("📊 Норма калорій: {CaloriesGoal} ккал", nutritionGoal.CaloriesGoal);

            var todayLogs = await _foodLogRepository.GetByUserIdAndDateAsync(user.Id, today);
            var totalCalories = todayLogs.Sum(log => (log.Food?.CaloriesPer100g ?? 0) * log.AmountGrams / 100);

            _logger.LogInformation("📈 Всього калорій сьогодні: {TotalCalories} ккал", totalCalories);

            var thresholdPercent = _notificationSettings.Value.CalorieThresholdPercent;
            var threshold = (decimal)nutritionGoal.CaloriesGoal * (thresholdPercent / 100m);

            _logger.LogInformation("🚀 Поріг: {Threshold} ккал ({ThresholdPercent}%)", threshold, thresholdPercent);

            if (totalCalories >= (double)threshold)
            {
                var overage = (decimal)totalCalories - threshold;
                _logger.LogInformation("✅ Перевищено поріг на {Overage:F0} ккал! Створюю сповіщення...", overage);

                await _notificationService.CreateCalorieThresholdNotificationAsync(user.Id, overage);
            }
            else
            {
                _logger.LogInformation("✅ Поріг НЕ перевищено");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Помилка при перевірці порогу калорій для користувача {UserEmail}", userEmail);
        }
    }
}
