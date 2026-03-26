using StayFit.Domain.Entities;

namespace StayFit.Domain.Interfaces;

public interface INutritionGoalRepository : IRepository<NutritionGoal>
{
    Task<NutritionGoal?> GetByUserIdAsync(string userId);
    Task<bool> ExistsForUserAsync(string userId);
}