using Microsoft.Extensions.Logging;
using StayFit.Application.Common;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Application.Services;

public class NutritionGoalService : INutritionGoalService
{
    private readonly INutritionGoalRepository _repository;
    private readonly ILogger<NutritionGoalService> _logger;

    public NutritionGoalService(INutritionGoalRepository repository, ILogger<NutritionGoalService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<NutritionGoalDto>> GetGoalAsync(string userId)
    {
        _logger.LogInformation("Отримання цілей харчування для користувача {UserId}", userId);
        
        var goal = await _repository.GetByUserIdAsync(userId);
        
        if (goal == null)
        {
            _logger.LogWarning("Цілі для користувача {UserId} не встановлені", userId);
            return Result<NutritionGoalDto>.Failure("Цілі не встановлено");
        }

        return Result<NutritionGoalDto>.Success(MapToDto(goal));
    }

    public async Task<Result<NutritionGoalDto>> SetGoalAsync(string userId, SetNutritionGoalDto dto)
    {
        _logger.LogInformation("Встановлення нових цілей для користувача {UserId}", userId);
        
        var existingGoal = await _repository.GetByUserIdAsync(userId);
        NutritionGoal goal;

        if (existingGoal != null)
        {
            _logger.LogInformation("Оновлення існуючих цілей (ID: {GoalId})", existingGoal.Id);
            existingGoal.CaloriesGoal = dto.CaloriesGoal;
            existingGoal.ProteinGoal = dto.ProteinGoal;
            existingGoal.FatGoal = dto.FatGoal;
            existingGoal.CarbsGoal = dto.CarbsGoal;
            existingGoal.UpdatedAt = DateTime.UtcNow;
            
            await _repository.UpdateAsync(existingGoal);
            goal = existingGoal;
        }
        else
        {
            _logger.LogInformation("Створення нових цілей для користувача");
            goal = new NutritionGoal
            {
                UserId = userId,
                CaloriesGoal = dto.CaloriesGoal,
                ProteinGoal = dto.ProteinGoal,
                FatGoal = dto.FatGoal,
                CarbsGoal = dto.CarbsGoal
            };
            await _repository.AddAsync(goal);
        }

        return Result<NutritionGoalDto>.Success(MapToDto(goal));
    }

    private static NutritionGoalDto MapToDto(NutritionGoal goal) =>
        new(goal.Id, goal.UserId, goal.CaloriesGoal, goal.ProteinGoal, goal.FatGoal, goal.CarbsGoal, goal.CreatedAt, goal.UpdatedAt);
}