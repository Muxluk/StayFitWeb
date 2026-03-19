using Microsoft.EntityFrameworkCore;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Data;

namespace StayFit.Infrastructure.Repositories;

public class FoodRepository : Repository<Food>, IFoodRepository
{
    public FoodRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<IEnumerable<Food>> GetAllByOwnerAsync(int ownerUserId) =>
        await DbSet
            .Where(f => f.OwnerUserId == ownerUserId)
            .OrderBy(f => f.Name)
            .ToListAsync();

    public async Task<Food?> GetByIdAndOwnerAsync(int id, int ownerUserId) =>
        await DbSet
            .FirstOrDefaultAsync(f => f.Id == id && f.OwnerUserId == ownerUserId);

    public async Task<IEnumerable<Food>> SearchByNameAsync(string name) =>
        await DbSet
            .Where(f => f.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToListAsync();
}
