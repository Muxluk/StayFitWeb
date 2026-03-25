using Microsoft.Extensions.Logging;
using StayFit.Application.Common;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Infrastructure.Services;

public sealed class StatisticsService(
    IFoodLogRepository foodLogRepository,
    IUserRepository userRepository,
    ILogger<StatisticsService> logger)
    : IStatisticsService
{
    public async Task<Result<NutritionSummary>> GetSummaryAsync(
        string userEmail,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userEmail))
        {
            logger.LogWarning("Statistics request rejected: empty user email");
            return Result<NutritionSummary>.Failure("Email користувача не вказано.");
        }

        if (startDate.Date > endDate.Date)
        {
            logger.LogWarning(
                "Statistics request rejected: invalid date range for {Email}. Start: {StartDate}, End: {EndDate}",
                userEmail,
                startDate,
                endDate);

            return Result<NutritionSummary>.Failure("Початкова дата не може бути пізніше кінцевої.");
        }

        logger.LogInformation(
            "Statistics calculation started for {Email}. Period: {StartDate} - {EndDate}",
            userEmail,
            startDate.Date,
            endDate.Date);

        var user = await userRepository.GetByEmailAsync(userEmail);
        if (user is null)
        {
            logger.LogWarning("Statistics request failed: user not found for email {Email}", userEmail);
            return Result<NutritionSummary>.Failure("Користувача не знайдено.");
        }

        var allLogs = await foodLogRepository.GetByUserIdAsync(user.Id);
        var periodStart = startDate.Date;
        var periodEnd = endDate.Date;

        var periodLogs = allLogs
            .Where(log => log.LoggedAt.Date >= periodStart && log.LoggedAt.Date <= periodEnd)
            .ToList();

        var summary = new NutritionSummary
        {
            StartDate = periodStart,
            EndDate = periodEnd,
            TotalCalories = periodLogs.Sum(log => (decimal)(log.Food?.Calories ?? 0f) * (decimal)log.AmountGrams / 100m),
            TotalProtein = periodLogs.Sum(log => (decimal)(log.Food?.Proteins ?? 0f) * (decimal)log.AmountGrams / 100m),
            TotalFat = periodLogs.Sum(log => (decimal)(log.Food?.Fats ?? 0f) * (decimal)log.AmountGrams / 100m),
            TotalCarbs = periodLogs.Sum(log => (decimal)(log.Food?.Carbohydrates ?? 0f) * (decimal)log.AmountGrams / 100m),
            DaysWithLogs = periodLogs
                .Select(log => log.LoggedAt.Date)
                .Distinct()
                .Count(),
        };

        logger.LogInformation(
            "Statistics calculation completed for {Email}. Logs: {LogCount}, Calories: {Calories}",
            userEmail,
            periodLogs.Count,
            summary.TotalCalories);

        return Result<NutritionSummary>.Success(summary);
    }
}
