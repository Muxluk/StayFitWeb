using StayFit.Domain.Entities;

namespace StayFit.Domain.Interfaces;

public interface IFoodImportRepository
{
    Task AddImportedFoodAsync(Food food);
    Task<HashSet<string>> GetMatchingNamesAsync(IEnumerable<string> names);
}
