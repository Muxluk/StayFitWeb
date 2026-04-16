using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using StayFit.Application.Common;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Application.Options;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IFoodLogRepository _foodLogRepository;
    private readonly INutritionGoalRepository _nutritionGoalRepository;
    private readonly IUserRepository _userRepository;
    private readonly DashboardSettings _dashboardSettings;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IFoodLogRepository foodLogRepository,
        INutritionGoalRepository nutritionGoalRepository,
        IUserRepository userRepository,
        IOptions<DashboardSettings> dashboardSettings,
        ILogger<DashboardService> logger)
    {
        _foodLogRepository = foodLogRepository;
        _nutritionGoalRepository = nutritionGoalRepository;
        _userRepository = userRepository;
        _dashboardSettings = dashboardSettings.Value;
        _logger = logger;
    }

    public async Task<Result<DashboardDto>> GetTodayDashboardAsync(int authUserId, string userEmail)
    {
        _logger.LogInformation(
            "Отримання даних дашборду для auth-користувача {AuthUserId} та email {Email}",
            authUserId,
            userEmail);

        // Отримання записів їжі за сьогодні
        var today = DateTime.Today;
        var domainUser = await _userRepository.GetByEmailAsync(userEmail);
        var foodLogs = domainUser is null
            ? Enumerable.Empty<FoodLog>()
            : await _foodLogRepository.GetByUserIdAndDateAsync(domainUser.Id, today);

        var recentEntriesLimit = _dashboardSettings.RecentDiaryEntriesCount > 0
            ? _dashboardSettings.RecentDiaryEntriesCount
            : 5;

        var recentFoodLogs = domainUser is null
            ? Enumerable.Empty<FoodLog>()
            : await _foodLogRepository.GetLatestByUserIdAsync(domainUser.Id, recentEntriesLimit);

        // Обчислення фактичних калорій та БЖВ
        float actualCalories = 0;
        float actualProtein = 0;
        float actualFat = 0;
        float actualCarbs = 0;

        foreach (var log in foodLogs)
        {
            var food = log.Food;
            var amount = log.AmountGrams / 100f; // переведення грамів у порції 100г

            actualCalories += food.CaloriesPer100g * amount;
            actualProtein += food.ProteinPer100g * amount;
            actualFat += food.FatPer100g * amount;
            actualCarbs += food.CarbsPer100g * amount;
        }

        // Отримання цільових значень
        var goal = await _nutritionGoalRepository.GetByUserIdAsync(authUserId.ToString());

        if (goal == null)
        {
            _logger.LogWarning("Цілі харчування не встановлені для користувача {UserId}", authUserId);
            return Result<DashboardDto>.Failure("Цілі харчування не встановлені");
        }

        var dashboard = new DashboardDto
        {
            ActualCalories = actualCalories,
            TargetCalories = goal.CaloriesGoal,
            ActualProtein = actualProtein,
            TargetProtein = goal.ProteinGoal,
            ActualFat = actualFat,
            TargetFat = goal.FatGoal,
            ActualCarbs = actualCarbs,
            TargetCarbs = goal.CarbsGoal,
            RecentDiaryEntries = recentFoodLogs
                .Select(log =>
                {
                    var portion = log.AmountGrams / 100f;
                    var food = log.Food;

                    return new RecentDiaryEntryDto
                    {
                        LoggedAt = log.LoggedAt,
                        FoodName = food?.Name ?? "Невідомий продукт",
                        AmountGrams = log.AmountGrams,
                        Calories = (food?.CaloriesPer100g ?? 0f) * portion,
                        Protein = (food?.ProteinPer100g ?? 0f) * portion,
                        Fat = (food?.FatPer100g ?? 0f) * portion,
                        Carbs = (food?.CarbsPer100g ?? 0f) * portion
                    };
                })
                .ToList()
        };

        _logger.LogInformation(
            "Дашборд дсклади: Калорії {ActualCal}/{TargetCal}, Білки {ActualPr}/{TargetPr}, Жири {ActualFat}/{TargetFat}, Вуглеводи {ActualCb}/{TargetCb}",
            actualCalories, goal.CaloriesGoal, actualProtein, goal.ProteinGoal,
            actualFat, goal.FatGoal, actualCarbs, goal.CarbsGoal);

        return dashboard;
    }
}
