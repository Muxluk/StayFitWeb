using StayFit.Domain.Entities;

namespace StayFit.Application.Interfaces;

public interface IFoodService
{
    Task<IEnumerable<Food>> GetAllFoodsAsync(int userId);
    Task<Food?> GetFoodByIdAsync(int id, int userId);
    Task AddFoodAsync(Food food, int userId);
    Task UpdateFoodAsync(Food food, int userId);
    Task DeleteFoodAsync(int id, int userId);
    Task<IEnumerable<FoodLog>> GetDailyLogAsync(string userEmail, DateTime date);
    Task AddFoodToLogAsync(int foodId, double quantity, string userEmail);
    Task<bool> QuickAddFoodLogEntryAsync(int logId, string userEmail);
    Task RemoveFromLogAsync(int logId);
}
