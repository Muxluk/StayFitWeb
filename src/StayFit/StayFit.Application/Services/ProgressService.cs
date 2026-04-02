using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Domain.Interfaces;
using StayFit.Domain.Results;

namespace StayFit.Application.Services;

public class ProgressService : IProgressService
{
    private readonly IFoodLogRepository _foodLogRepository;
    private readonly INutritionGoalRepository _goalRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ProgressService> _logger;

    public ProgressService(
        IFoodLogRepository foodLogRepository,
        INutritionGoalRepository goalRepository,
        IUserRepository userRepository,
        ILogger<ProgressService> logger)
    {
        _foodLogRepository = foodLogRepository;
        _goalRepository = goalRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<ProgressAnalysisDto>> GetProgressAnalysisAsync(int identityUserId, string userEmail, DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation("Отримання прогресу для {UserEmail} з {StartDate} по {EndDate}", userEmail, startDate, endDate);

        if (startDate > endDate)
            return new Result<ProgressAnalysisDto>.Failure("Дата початку не може бути більшою за дату кінця", "INVALID_DATES");

        // Знаходимо DomainUser.Id за email (як GetDailyLogAsync) — для запиту FoodLogs
        var domainUser = await _userRepository.GetByEmailAsync(userEmail);
        if (domainUser == null)
            return new Result<ProgressAnalysisDto>.Failure("Користувача не знайдено", "USER_NOT_FOUND");

        var domainUserId = domainUser.Id;

        // Ціль зберігається з identityUserId (як у NutritionGoalController)
        var goal = await _goalRepository.GetByUserIdAsync(identityUserId.ToString());
        if (goal == null)
            return new Result<ProgressAnalysisDto>.Failure("Цілі харчування не встановлено. Перейдіть у «Мої цілі» та збережіть ціль.", "NO_GOAL");

        // FoodLogs зберігаються з DomainUser.Id
        var allLogs = (await _foodLogRepository.GetByUserIdAndDateRangeAsync(domainUserId, startDate, endDate)).ToList();
        _logger.LogInformation("Знайдено {Count} записів. IdentityUserId={IId}, DomainUserId={DId}", allLogs.Count, identityUserId, domainUserId);

        var dailyProgress = new List<DailyProgressDto>();
        int daysCaloriesMet = 0;
        int totalDays = (endDate.Date - startDate.Date).Days + 1;

        for (int i = 0; i < totalDays; i++)
        {
            var currentDate = startDate.Date.AddDays(i);
            // LoggedAt зберігається в UTC, конвертуємо до локального часу для порівняння дати
            var dayLogs = allLogs.Where(f => f.LoggedAt.ToLocalTime().Date == currentDate).ToList();

            var daily = new DailyProgressDto
            {
                Date = currentDate,
                TotalCalories = dayLogs.Sum(f => f.Food != null ? f.Food.CaloriesPer100g * f.AmountGrams / 100.0 : 0),
                TotalProtein  = dayLogs.Sum(f => f.Food != null ? f.Food.ProteinPer100g  * f.AmountGrams / 100.0 : 0),
                TotalFat      = dayLogs.Sum(f => f.Food != null ? f.Food.FatPer100g      * f.AmountGrams / 100.0 : 0),
                TotalCarbs    = dayLogs.Sum(f => f.Food != null ? f.Food.CarbsPer100g    * f.AmountGrams / 100.0 : 0),
            };

            if (daily.TotalCalories > 0)
                daily.CaloriesGoalMet = Math.Abs(daily.TotalCalories - goal.CaloriesGoal) <= 150;

            if (daily.CaloriesGoalMet) daysCaloriesMet++;
            dailyProgress.Add(daily);
        }

        return new ProgressAnalysisDto
        {
            StartDate = startDate,
            EndDate = endDate,
            CaloriesGoal = goal.CaloriesGoal,
            ProteinGoal  = goal.ProteinGoal,
            FatGoal      = goal.FatGoal,
            CarbsGoal    = goal.CarbsGoal,
            DailyProgress = dailyProgress,
            DaysCaloriesMet = daysCaloriesMet,
            TotalDays = totalDays,
        };
    }
}
