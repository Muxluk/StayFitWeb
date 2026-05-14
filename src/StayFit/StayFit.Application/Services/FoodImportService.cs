using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Application.Options;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Application.Services;

public class FoodImportService : IFoodImportService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IFoodImportRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly UsdaFoodDataOptions _options;
    private readonly ILogger<FoodImportService> _logger;

    public FoodImportService(
        IHttpClientFactory httpClientFactory,
        IFoodImportRepository repository,
        IMemoryCache cache,
        IOptions<UsdaFoodDataOptions> options,
        ILogger<FoodImportService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _repository = repository;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IEnumerable<Food>> SearchGlobalAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return Array.Empty<Food>();
        }

        var cacheKey = $"UsdaFoodData_Search_{searchTerm.ToLowerInvariant()}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<Food>? cachedProducts))
        {
            _logger.LogInformation("Returning products from cache for term: {SearchTerm}", searchTerm);
            return cachedProducts ?? Array.Empty<Food>();
        }

        try
        {
            _logger.LogInformation("Performing USDA FoodData API search for term: {SearchTerm}", searchTerm);
            
            var client = _httpClientFactory.CreateClient();
            var url = $"{_options.ApiUrl}?query={Uri.EscapeDataString(searchTerm)}&api_key={_options.ApiKey}&pageSize=20";
            
            var response = await client.GetFromJsonAsync<UsdaSearchResponse>(url);
            
            if (response?.Foods == null)
            {
                _logger.LogWarning("API returned no products for term: {SearchTerm}", searchTerm);
                return Array.Empty<Food>();
            }

            var result = response.Foods
                .Where(p => !string.IsNullOrEmpty(p.Description))
                .Select(p => 
                {
                    float TryGetNutrient(string keyWords)
                    {
                        if (p.FoodNutrients == null) return 0f;
                        var nutrient = p.FoodNutrients.FirstOrDefault(n => 
                            n.NutrientName != null && n.NutrientName.Contains(keyWords, StringComparison.OrdinalIgnoreCase));
                        return nutrient?.Value ?? 0f;
                    }

                    return new Food
                    {
                        Name = p.Description ?? "Unknown",
                        Brand = p.BrandOwner,
                        CaloriesPer100g = TryGetNutrient("Energy"),
                        ProteinPer100g = TryGetNutrient("Protein"),
                        FatPer100g = TryGetNutrient("Total lipid (fat)"),
                        CarbsPer100g = TryGetNutrient("Carbohydrate, by difference"),
                        IsVerified = true,
                        Category = StayFit.Domain.Enums.FoodCategory.General
                    };
                })
                .ToList();

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheLifetimeMinutes)
            };

            _cache.Set(cacheKey, result, cacheOptions);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while searching external API for term: {SearchTerm}", searchTerm);
            return Array.Empty<Food>();
        }
    }

    public async Task ImportProductAsync(Food product, int ownerUserId, string userEmail)
    {
        _logger.LogInformation("Importing product {ProductName} for user {UserId}", product.Name, ownerUserId);
        
        product.OwnerUserId = ownerUserId;
        product.CreatedByEmail = userEmail;
        product.Id = 0; // Ensure it's treated as a new entity
        product.SubmittedAt ??= DateTime.UtcNow;

        await _repository.AddImportedFoodAsync(product);
        
        _logger.LogInformation("Product {ProductName} imported successfully into local DB", product.Name);
    }

    public Task<HashSet<string>> GetExistingNamesAsync(IEnumerable<string> names)
        => _repository.GetMatchingNamesAsync(names);
}
