using Microsoft.EntityFrameworkCore;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Data;

namespace StayFit.Infrastructure.Repositories;

public sealed class StatisticsRepository(AppDbContext context) : IStatisticsRepository
{
    public async Task<NutritionSummary> GetNutritionSummaryAsync(
        int userId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var periodStart = startDate.Date;
        var periodEndExclusive = endDate.Date.AddDays(1);

        var baseQuery = context.FoodLogs
            .AsNoTracking()
            .Where(log =>
                log.UserId == userId &&
                log.LoggedAt >= periodStart &&
                log.LoggedAt < periodEndExclusive);

        var aggregates = await baseQuery
            .GroupBy(_ => 1)
            .Select(group => new
            {
                TotalCalories = group.Sum(log => (double)log.Food.CaloriesPer100g * log.AmountGrams / 100d),
                TotalProtein = group.Sum(log => (double)log.Food.ProteinPer100g * log.AmountGrams / 100d),
                TotalFat = group.Sum(log => (double)log.Food.FatPer100g * log.AmountGrams / 100d),
                TotalCarbs = group.Sum(log => (double)log.Food.CarbsPer100g * log.AmountGrams / 100d),
            })
            .FirstOrDefaultAsync(cancellationToken);

        var daysWithLogs = await baseQuery
            .Select(log => log.LoggedAt.Date)
            .Distinct()
            .CountAsync(cancellationToken);

        return new NutritionSummary
        {
            StartDate = periodStart,
            EndDate = endDate.Date,
            TotalCalories = aggregates is null ? 0m : (decimal)aggregates.TotalCalories,
            TotalProtein = aggregates is null ? 0m : (decimal)aggregates.TotalProtein,
            TotalFat = aggregates is null ? 0m : (decimal)aggregates.TotalFat,
            TotalCarbs = aggregates is null ? 0m : (decimal)aggregates.TotalCarbs,
            DaysWithLogs = daysWithLogs,
        };
    }
}
