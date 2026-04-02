using Microsoft.Extensions.Logging;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Domain.Results;

namespace StayFit.Application.Services;

public class ProductModerationService : IProductModerationService
{
    private readonly IFoodRepository _foodRepository;
    private readonly ILogger<ProductModerationService> _logger;

    public ProductModerationService(
        IFoodRepository foodRepository, 
        ILogger<ProductModerationService> logger)
    {
        _foodRepository = foodRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<Food>>> GetPendingProductsAsync()
    {
        var products = await _foodRepository.GetPendingProductsAsync();
        
        return products.ToList();
    }

    public async Task<Result<bool>> ApproveProductAsync(int id)
    {
        var product = await _foodRepository.GetByIdAsync(id);
        if (product == null)
        {
            return new Result<bool>.Failure("Продукт не знайдено", "NOT_FOUND");
        }

        await _foodRepository.UpdateProductStatusAsync(id, true);
        _logger.LogInformation("Адміністратор підтвердив продукт: {ProductName}", product.Name);
        
        return true;
    }

    public async Task<Result<bool>> RejectProductAsync(int id)
    {
        var product = await _foodRepository.GetByIdAsync(id);
        if (product == null)
        {
            return new Result<bool>.Failure("Продукт не знайдено", "NOT_FOUND");
        }

        await _foodRepository.DeleteAsync(id);
        _logger.LogWarning("Адміністратор відхилив продукт: {ProductName}", product.Name);
        
        return true; 
    }
}