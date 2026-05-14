using Microsoft.EntityFrameworkCore;
using StayFit.Domain.Entities;
using StayFit.Domain.Enums;
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

    // Пошук з пагінацією та фільтрацією за категорією
    public async Task<(IEnumerable<Food> Items, int TotalCount)> SearchAsync(string? searchTerm, FoodCategory? category, int page, int pageSize, int userId)
    {

        var query = DbSet.Where(f => f.IsApproved || f.OwnerUserId == userId).AsQueryable();


        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(f => EF.Functions.ILike(f.Name, $"%{searchTerm}%") ||
                                    (f.Brand != null && EF.Functions.ILike(f.Brand, $"%{searchTerm}%")));
        }


        if (category.HasValue)
        {
            query = query.Where(f => f.Category == category.Value);
        }


        var totalCount = await query.CountAsync();


        var items = await query
            .OrderBy(f => f.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// Отримує всі продукти, які ще не були підтверджені адміністратором.
    public async Task<IEnumerable<Food>> GetPendingProductsAsync()
    {
        return await DbSet
            .Where(f => f.IsApproved == false)
            .OrderByDescending(f => f.SubmittedAt)
            .ThenByDescending(f => f.Id)
            .ToListAsync();
    }

    /// Оновлює статус модерації продукту.
    public async Task UpdateProductStatusAsync(int id, bool isApproved)
    {
        var food = await DbSet.FindAsync(id);
        if (food != null)
        {
            food.IsApproved = isApproved;
            await Context.SaveChangesAsync();
        }
    }
}
