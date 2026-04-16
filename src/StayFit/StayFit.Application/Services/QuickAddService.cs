using Microsoft.Extensions.Logging;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Application.Services;

public class QuickAddService
{
    private readonly IMealRepository _mealRepository;
    private readonly IFoodLogRepository _foodLogRepository;
    private readonly ILogger<QuickAddService> _logger;

    public QuickAddService(
        IMealRepository mealRepository,
        IFoodLogRepository foodLogRepository,
        ILogger<QuickAddService> logger)
    {
        _mealRepository = mealRepository;
        _foodLogRepository = foodLogRepository;
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
            }

            _logger.LogInformation("Successfully copied {LogCount} food logs from meal {SourceMealId} to {NewMealId}",
                copiedLogsCount, mealId, newMeal.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during quick add for meal {MealId} and user {UserEmail}", mealId, userEmail);
            return false;
        }
    }
}
