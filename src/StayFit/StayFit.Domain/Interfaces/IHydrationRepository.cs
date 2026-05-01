using StayFit.Domain.Entities;

namespace StayFit.Domain.Interfaces;

public interface IHydrationRepository
{
    Task<HydrationGoal?> GetGoalByUserIdAsync(int userId);
    Task UpdateGoalAsync(HydrationGoal goal);
    Task AddGoalAsync(HydrationGoal goal);

    Task AddWaterLogAsync(WaterLog log);
    Task<IEnumerable<WaterLog>> GetLogsForDateAsync(int userId, DateTime date);
    Task<IEnumerable<WaterLog>> GetLogsForPeriodAsync(int userId, DateTime startDate, DateTime endDate);
}