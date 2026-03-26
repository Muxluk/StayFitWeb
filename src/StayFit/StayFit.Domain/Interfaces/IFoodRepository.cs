using StayFit.Domain.Entities;
using StayFit.Domain.Enums;
namespace StayFit.Domain.Interfaces;

public interface IFoodRepository : IRepository<Food>
{
    Task<IEnumerable<Food>> GetAllByOwnerAsync(int ownerUserId);
    Task<Food?> GetByIdAndOwnerAsync(int id, int ownerUserId);


    Task<(IEnumerable<Food> Items, int TotalCount)> SearchAsync(string? searchTerm, FoodCategory? category, int page, int pageSize, int userId);
}
