using StayFit.Domain.Entities;

namespace StayFit.Domain.Interfaces;

public interface IBarcodeScanRepository
{
    Task<Food?> GetFoodByBarcodeAsync(string barcode, int userId);
    Task<int> ImportFoodAsync(Food food);
}
