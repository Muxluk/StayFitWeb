using StayFit.Domain.Entities;

namespace StayFit.Application.Interfaces;

public interface IFoodImportService
{
    Task<IEnumerable<Food>> SearchGlobalAsync(string searchTerm);
    Task ImportProductAsync(Food product, int ownerUserId, string userEmail);
}
