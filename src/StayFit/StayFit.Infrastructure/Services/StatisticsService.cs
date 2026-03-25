using Microsoft.Extensions.Logging;
using StayFit.Application.Common;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Infrastructure.Services;

public sealed class StatisticsService(
    IStatisticsRepository statisticsRepository,
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

        var periodStart = startDate.Date;
        var periodEnd = endDate.Date;
        var summary = await statisticsRepository.GetNutritionSummaryAsync(
            user.Id,
            periodStart,
            periodEnd,
            cancellationToken);

        logger.LogInformation(
            "Statistics calculation completed for {Email}. DaysWithLogs: {DaysWithLogs}, Calories: {Calories}",
            userEmail,
            summary.DaysWithLogs,
            summary.TotalCalories);

        return Result<NutritionSummary>.Success(summary);
    }
}
