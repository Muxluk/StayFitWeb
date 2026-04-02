using Microsoft.EntityFrameworkCore;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Infrastructure.Data;

namespace StayFit.Infrastructure.Repositories;

/// <summary>
/// Репозиторій для роботи з категоріями продуктів
/// </summary>
public class FoodCategoryRepository : IFoodCategoryRepository
{
    private readonly AppDbContext _context;

    public FoodCategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Отримати всі категорії
    /// </summary>
    public async Task<IEnumerable<FoodCategoryEntity>> GetAllAsync()
    {
        return await _context.Set<FoodCategoryEntity>()
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Отримати категорію за ID
    /// </summary>
    public async Task<FoodCategoryEntity?> GetByIdAsync(int id)
    {
        return await _context.Set<FoodCategoryEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <summary>
    /// Отримати тільки активні категорії
    /// </summary>
    public async Task<IEnumerable<FoodCategoryEntity>> GetActiveAsync()
    {
        return await _context.Set<FoodCategoryEntity>()
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Додати нову категорію
    /// </summary>
    public async Task<FoodCategoryEntity> AddAsync(FoodCategoryEntity category)
    {
        _context.Set<FoodCategoryEntity>().Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    /// <summary>
    /// Оновити категорію
    /// </summary>
    public async Task<FoodCategoryEntity> UpdateAsync(FoodCategoryEntity category)
    {
        _context.Set<FoodCategoryEntity>().Update(category);
        await _context.SaveChangesAsync();
        return category;
    }

    /// <summary>
    /// Видалити категорію за ID
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.Set<FoodCategoryEntity>()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            return false;

        _context.Set<FoodCategoryEntity>().Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Перевірити, чи існує категорія з таким ID
    /// </summary>
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Set<FoodCategoryEntity>()
            .AnyAsync(c => c.Id == id);
    }

    /// <summary>
    /// Перевірити, чи існує категорія з таким назвою
    /// </summary>
    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.Set<FoodCategoryEntity>()
            .AnyAsync(c => c.Name.ToLower() == name.ToLower());
    }

    /// <summary>
    /// Отримати категорію за назвою
    /// </summary>
    public async Task<FoodCategoryEntity?> GetByNameAsync(string name)
    {
        return await _context.Set<FoodCategoryEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
    }
}
