using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Data;

namespace StayFit.Infrastructure.Repositories;

public class FoodImportRepository : IFoodImportRepository
{
    private readonly AppDbContext _context;

    public FoodImportRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddImportedFoodAsync(Food food)
    {
        await _context.Foods.AddAsync(food);
        await _context.SaveChangesAsync();
    }
}
