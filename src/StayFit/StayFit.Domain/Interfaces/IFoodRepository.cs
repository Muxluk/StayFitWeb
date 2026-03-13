using StayFit.Domain.Entities;

namespace StayFit.Domain.Interfaces;

public interface IFoodRepository : IRepository<Food>
{
    Task<IEnumerable<Food>> SearchByNameAsync(string name);
}
