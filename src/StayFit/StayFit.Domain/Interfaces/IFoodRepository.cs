using StayFit.Domain.Entities;

namespace StayFit.Domain.Interfaces;

public interface IFoodRepository : IRepository<Food>
{
    Task<IEnumerable<Food>> GetAllByOwnerAsync(int ownerUserId);
    Task<Food?> GetByIdAndOwnerAsync(int id, int ownerUserId);
    Task<IEnumerable<Food>> SearchByNameAsync(string name);
}
