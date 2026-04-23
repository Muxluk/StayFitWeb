using StayFit.Domain.Entities;

namespace StayFit.Domain.Interfaces;

public interface IFoodImportRepository
{
    Task AddImportedFoodAsync(Food food);
}
