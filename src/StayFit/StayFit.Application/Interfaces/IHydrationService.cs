using StayFit.Application.Common;
using StayFit.Application.DTOs;
using StayFit.Domain.Entities;

namespace StayFit.Application.Interfaces;

public interface IHydrationService
{
    Task<Result<HydrationProgressDto>> GetTodayProgressAsync(int userId);
    Task<Result<bool>> LogWaterAsync(int userId, int volumeMl);
    Task<Result<bool>> SetGoalAsync(int userId, int goalMl);
    Task<Result<int>> CalculateAndSetAutoGoalAsync(int userId);
    Task<IEnumerable<WaterLog>> GetLogsForPeriodAsync(int userId, DateTime startDate, DateTime endDate);
}