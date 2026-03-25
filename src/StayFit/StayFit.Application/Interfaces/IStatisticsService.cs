using StayFit.Application.Common;
using StayFit.Domain.Entities;

namespace StayFit.Application.Interfaces;

public interface IStatisticsService
{
    Task<Result<NutritionSummary>> GetSummaryAsync(
        string userEmail,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}
