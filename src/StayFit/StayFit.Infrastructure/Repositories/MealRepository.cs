using Microsoft.EntityFrameworkCore;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Data;

namespace StayFit.Infrastructure.Repositories;

public class MealRepository : Repository<MealEntry>, IMealRepository
{
    private readonly AppDBContext _context;

    public MealRepository(AppDBContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MealEntry>> GetMealsWithFoodsAsync(string userEmail, DateTime date)
    {
        return await _context.Set<MealEntry>()
            .Include(m => m.FoodLogs)
                .ThenInclude(fl => fl.Food)
            .Where(m => m.UserEmail == userEmail && m.Time.Date == date.Date)
            .OrderBy(m => m.Time)
            .ToListAsync();
    }
}