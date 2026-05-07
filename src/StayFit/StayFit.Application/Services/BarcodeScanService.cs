using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Application.Options;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Application.Services;

public class BarcodeScanService : IBarcodeScanService
{
    private readonly IUsdaFoodDataClient _usdaClient;
    private readonly IBarcodeScanRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly UsdaFoodDataOptions _options;
    private readonly ILogger<BarcodeScanService> _logger;

    public BarcodeScanService(
        IUsdaFoodDataClient usdaClient,
        IBarcodeScanRepository repository,
        IMemoryCache cache,
        IOptions<UsdaFoodDataOptions> options,
        ILogger<BarcodeScanService> logger)
    {
        _usdaClient = usdaClient;
        _repository = repository;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<BarcodeScanResultDto?> ScanBarcodeAsync(string barcode, int userId)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return null;
        }

        var cacheKey = $"UsdaFoodData_Barcode_{barcode.Trim()}";

        // 1. Перевірка кешу
        if (_cache.TryGetValue(cacheKey, out Food? cachedFood) && cachedFood != null)
        {
            _logger.LogInformation("Returning product from cache for barcode: {Barcode}", barcode);
            var localExists = await _repository.GetFoodByBarcodeAsync(barcode, userId) != null;
            return new BarcodeScanResultDto { Food = cachedFood, ExistsInLocalDb = localExists };
        }

        _logger.LogInformation("Performing USDA FoodData API search for barcode: {Barcode}", barcode);

        // 2. Звернення до API
        var response = await _usdaClient.SearchByBarcodeAsync(barcode);

        if (response?.Foods == null || !response.Foods.Any())
        {
            _logger.LogWarning("API returned no products for barcode: {Barcode}", barcode);
            return null;
        }

        var p = response.Foods.First();

        if (string.IsNullOrEmpty(p.Description))
        {
            return null;
        }

        float TryGetNutrient(string keyWords)
        {
            if (p.FoodNutrients == null) return 0f;
            var nutrient = p.FoodNutrients.FirstOrDefault(n =>
                n.NutrientName != null && n.NutrientName.Contains(keyWords, StringComparison.OrdinalIgnoreCase));
            return nutrient?.Value ?? 0f;
        }

        var food = new Food
        {
            Name = p.Description,
            Brand = p.BrandOwner,
            Barcode = barcode,
            CaloriesPer100g = TryGetNutrient("Energy"),
            ProteinPer100g = TryGetNutrient("Protein"),
            FatPer100g = TryGetNutrient("Total lipid (fat)"),
            CarbsPer100g = TryGetNutrient("Carbohydrate, by difference"),
            IsVerified = true,
            Category = StayFit.Domain.Enums.FoodCategory.General
        };

        // 3. Збереження в кеш
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheLifetimeMinutes)
        };
        _cache.Set(cacheKey, food, cacheOptions);

        // 4. Перевірка чи існує в БД
        var existingFood = await _repository.GetFoodByBarcodeAsync(barcode, userId);

        return new BarcodeScanResultDto
        {
            Food = food,
            ExistsInLocalDb = existingFood != null
        };
    }
}
