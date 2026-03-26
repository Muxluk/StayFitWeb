using StayFit.Application.Common;
using StayFit.Application.DTOs;

namespace StayFit.Application.Interfaces;

public interface INutritionGoalService
{
    Task<Result<NutritionGoalDto>> GetGoalAsync(string userId);
    Task<Result<NutritionGoalDto>> SetGoalAsync(string userId, SetNutritionGoalDto dto);
}