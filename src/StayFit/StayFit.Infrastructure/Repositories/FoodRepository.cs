using Microsoft.EntityFrameworkCore;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Data;

namespace StayFit.Infrastructure.Repositories;

public class FoodRepository : Repository<Food>, IFoodRepository
{
    public FoodRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Food>> SearchByNameAsync(string name) =>
        await _dbSet
            .Where(f => f.Name.ToLower().Contains(name.ToLower()))
            .ToListAsync();
}
