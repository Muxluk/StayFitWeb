using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StayFit.Application.Common;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Application.Options;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Application.Services;

public class HydrationService : IHydrationService
{
    private readonly IHydrationRepository _hydrationRepository;
    private readonly IUserProfileRepository _userProfileRepository; // Для отримання ваги
    private readonly HydrationSettings _settings;
    private readonly ILogger<HydrationService> _logger;

    public HydrationService(
        IHydrationRepository hydrationRepository,
        IUserProfileRepository userProfileRepository,
        IOptions<HydrationSettings> options,
        ILogger<HydrationService> logger)
    {
        _hydrationRepository = hydrationRepository;
        _userProfileRepository = userProfileRepository;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<Result<HydrationProgressDto>> GetTodayProgressAsync(int userId)
    {
        var goal = await _hydrationRepository.GetGoalByUserIdAsync(userId);
        int goalMl = goal?.DailyGoalMl ?? 0;

        // Якщо цілі ще немає, спробуємо авторозрахувати
        if (goalMl == 0)
        {
            var autoGoalResult = await CalculateAndSetAutoGoalAsync(userId);
            if (autoGoalResult.IsSuccess) goalMl = autoGoalResult.Value;
        }

        var logs = await _hydrationRepository.GetLogsForDateAsync(userId, DateTime.UtcNow);

        var dto = new HydrationProgressDto
        {
            DailyGoalMl = goalMl,
            ConsumedMl = logs.Sum(l => l.VolumeMl),
            QuickAddOptions = _settings.QuickAddButtons,
            TodayLogs = logs.Select(l => new WaterLogDto
            {
                Id = l.Id,
                VolumeMl = l.VolumeMl,
                LoggedAt = l.LoggedAt.ToLocalTime() // Показуємо локальний час
            })
        };

        return Result<HydrationProgressDto>.Success(dto);
    }

    public async Task<Result<bool>> LogWaterAsync(int userId, int volumeMl)
    {
        if (volumeMl <= 0) return Result<bool>.Failure("Об'єм має бути більшим за 0");

        try
        {
            var log = new WaterLog
            {
                UserId = userId,
                VolumeMl = volumeMl,
                LoggedAt = DateTime.UtcNow
            };
            await _hydrationRepository.AddWaterLogAsync(log);
            _logger.LogInformation("User {UserId} logged {Volume} ml of water", userId, volumeMl);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging water for user {UserId}", userId);
            return Result<bool>.Failure("Помилка збереження даних");
        }
    }

    public async Task<Result<bool>> SetGoalAsync(int userId, int goalMl)
    {
        if (goalMl < 500 || goalMl > 10000) return Result<bool>.Failure("Некоректна ціль (500-10000 мл)");

        var goal = await _hydrationRepository.GetGoalByUserIdAsync(userId);
        if (goal == null)
        {
            await _hydrationRepository.AddGoalAsync(new HydrationGoal { UserId = userId, DailyGoalMl = goalMl });
        }
        else
        {
            goal.DailyGoalMl = goalMl;
            await _hydrationRepository.UpdateGoalAsync(goal);
        }
        
        _logger.LogInformation("User {UserId} set custom hydration goal: {Goal} ml", userId, goalMl);
        return Result<bool>.Success(true);
    }

    public async Task<Result<int>> CalculateAndSetAutoGoalAsync(int userId)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(userId);
        
        if (profile == null || !profile.Weight.HasValue || profile.Weight.Value <= 0) 
            return Result<int>.Failure("Не вказана вага профілю");

        int calculatedGoal = (int)(profile.Weight.Value * _settings.WeightMultiplierMl);
        await SetGoalAsync(userId, calculatedGoal);
        
        _logger.LogInformation("Calculated auto goal for User {UserId}: {Goal} ml based on weight", userId, calculatedGoal);
        return Result<int>.Success(calculatedGoal);
    }
    public async Task<IEnumerable<WaterLog>> GetLogsForPeriodAsync(int userId, DateTime start, DateTime end)
    {
        return await _hydrationRepository.GetLogsForPeriodAsync(userId, start, end);
    }
}