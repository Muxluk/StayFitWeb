using StayFit.Domain.Entities;

namespace StayFit.Domain.Interfaces;

public interface IFoodLogRepository : IRepository<FoodLog>
{
    Task<IEnumerable<FoodLog>> GetByUserIdAsync(int userId);
    Task<IEnumerable<FoodLog>> GetByUserIdAndDateAsync(int userId, DateTime date);
    Task<IEnumerable<FoodLog>> GetLatestByUserIdAsync(int userId, int count);
    Task<IEnumerable<FoodLog>> GetByFoodIdAsync(int foodId);
    Task<IEnumerable<FoodLog>> GetByUserIdAndDateRangeAsync(int userId, DateTime from, DateTime to);
    Task<IEnumerable<FoodLog>> GetByMealIdAsync(int mealId);
}
