using Microsoft.EntityFrameworkCore;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Data;

namespace StayFit.Infrastructure.Repositories;

public class BarcodeScanRepository : IBarcodeScanRepository
{
    private readonly AppDbContext _context;

    public BarcodeScanRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Food?> GetFoodByBarcodeAsync(string barcode, int userId)
    {
        return await _context.Foods
            .FirstOrDefaultAsync(f => f.Barcode == barcode && (f.OwnerUserId == userId || f.OwnerUserId == 0));
    }

    public async Task<int> ImportFoodAsync(Food food)
    {
        _context.Foods.Add(food);
        await _context.SaveChangesAsync();
        return food.Id;
    }
}
