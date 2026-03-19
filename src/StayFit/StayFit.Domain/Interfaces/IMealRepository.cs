using StayFit.Domain.Entities;

namespace StayFit.Domain.Interfaces;

public interface IMealRepository : IRepository<MealEntry>
{
    Task<IEnumerable<MealEntry>> GetMealsWithFoodsAsync(string userEmail, DateTime date);
}