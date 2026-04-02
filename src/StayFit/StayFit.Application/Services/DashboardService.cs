using Microsoft.Extensions.Logging;
using StayFit.Application.Common;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IFoodLogRepository _foodLogRepository;
    private readonly INutritionGoalRepository _nutritionGoalRepository;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IFoodLogRepository foodLogRepository,
        INutritionGoalRepository nutritionGoalRepository,
        ILogger<DashboardService> logger)
    {
        _foodLogRepository = foodLogRepository;
        _nutritionGoalRepository = nutritionGoalRepository;
        _logger = logger;
    }

    public async Task<Result<DashboardDto>> GetTodayDashboardAsync(int userId)
    {
        _logger.LogInformation("Отримання даних дашборду для користувача {UserId}", userId);

        // Отримання записів їжі за сьогодні
        var today = DateTime.Today;
        var foodLogs = await _foodLogRepository.GetByUserIdAndDateAsync(userId, today);

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
        var goal = await _nutritionGoalRepository.GetByUserIdAsync(userId.ToString());

        if (goal == null)
        {
            _logger.LogWarning("Цілі харчування не встановлені для користувача {UserId}", userId);
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
            TargetCarbs = goal.CarbsGoal
        };

        _logger.LogInformation(
            "Дашборд дсклади: Калорії {ActualCal}/{TargetCal}, Білки {ActualPr}/{TargetPr}, Жири {ActualFat}/{TargetFat}, Вуглеводи {ActualCb}/{TargetCb}",
            actualCalories, goal.CaloriesGoal, actualProtein, goal.ProteinGoal,
            actualFat, goal.FatGoal, actualCarbs, goal.CarbsGoal);

        return Result<DashboardDto>.Success(dashboard);
    }
}
