using StayFit.Application.DTOs;

namespace StayFit.Application.Interfaces;

public interface IUsdaFoodDataClient
{
    Task<UsdaSearchResponse?> SearchByBarcodeAsync(string barcode);
}
