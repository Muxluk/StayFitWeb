using Microsoft.Extensions.Logging;
using StayFit.Application.Common;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Domain.Enums;
using StayFit.Domain.Interfaces;

namespace StayFit.Application.Services;

public class ProductSearchService : IProductSearchService
{
    private readonly IFoodRepository _foodRepository;
    private readonly ILogger<ProductSearchService> _logger;

    public ProductSearchService(
        IFoodRepository foodRepository,
        ILogger<ProductSearchService> logger)
    {
        _foodRepository = foodRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<Food>>> SearchAsync(string? searchTerm, FoodCategory? category, int page, int pageSize, int userId)
    {
        _logger.LogInformation("Пошук продуктів. Term: '{Term}', Category: {Category}, Page: {Page}, UserId: {UserId}", 
            searchTerm, category, page, userId);

        if (page < 1)
        {
            _logger.LogWarning("Спроба запиту невалідної сторінки: {Page}", page);
            return Result<PagedResult<Food>>.Failure("Номер сторінки повинен бути більше нуля.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            _logger.LogWarning("Спроба запиту невалідного розміру сторінки: {PageSize}", pageSize);
            return Result<PagedResult<Food>>.Failure("Розмір сторінки має бути від 1 до 100.");
        }

        var (items, totalCount) = await _foodRepository.SearchAsync(searchTerm, category, page, pageSize, userId);

        var pagedResult = new PagedResult<Food>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };

        _logger.LogInformation("Знайдено {Count} продуктів. Сторінка {Page} з {TotalPages}", 
            totalCount, page, pagedResult.TotalPages == 0 ? 1 : pagedResult.TotalPages);

        return pagedResult;
    }
}
