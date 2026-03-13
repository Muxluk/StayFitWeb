using StayFit.Domain.Entities;

namespace StayFit.Domain.Interfaces;

public interface IFoodLogRepository : IRepository<FoodLog>
{
    Task<IEnumerable<FoodLog>> GetByUserIdAsync(int userId);
    Task<IEnumerable<FoodLog>> GetByUserIdAndDateAsync(int userId, DateTime date);
}
