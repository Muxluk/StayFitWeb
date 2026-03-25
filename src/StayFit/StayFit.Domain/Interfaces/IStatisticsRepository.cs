using StayFit.Domain.Entities;

namespace StayFit.Domain.Interfaces;

public interface IStatisticsRepository
{
    Task<NutritionSummary> GetNutritionSummaryAsync(
        int userId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}
