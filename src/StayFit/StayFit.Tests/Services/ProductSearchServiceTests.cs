using Microsoft.Extensions.Logging;
using Moq;
using StayFit.Application.Services;
using StayFit.Domain.Entities;
using StayFit.Domain.Enums;
using StayFit.Domain.Interfaces;
using System.Linq;
using Xunit;

namespace StayFit.Tests.Services;

public class ProductSearchServiceTests
{
    private readonly Mock<IFoodRepository> _mockFoodRepository;
    private readonly Mock<ILogger<ProductSearchService>> _mockLogger;
    private readonly ProductSearchService _productSearchService;

    public ProductSearchServiceTests()
    {
        _mockFoodRepository = new Mock<IFoodRepository>();
        _mockLogger = new Mock<ILogger<ProductSearchService>>();
        
        _productSearchService = new ProductSearchService(
            _mockFoodRepository.Object, 
            _mockLogger.Object);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnSuccess_WhenParametersAreValid()
    {
        // Arrange
        int userId = 1;
        string searchTerm = "Apple";
        FoodCategory category = FoodCategory.Fruits;
        int page = 1;
        int pageSize = 10;
        
        var foods = new List<Food> 
        { 
            new Food { Id = 1, Name = "Green Apple", Category = FoodCategory.Fruits },
            new Food { Id = 2, Name = "Red Apple", Category = FoodCategory.Fruits }
        };

        _mockFoodRepository
            .Setup(repo => repo.SearchAsync(searchTerm, category, page, pageSize, userId))
            .ReturnsAsync((foods, 2));

        // Act
        var result = await _productSearchService.SearchAsync(searchTerm, category, page, pageSize, userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Items.Count());
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Equal(1, result.Value.PageNumber);
        Assert.Equal(10, result.Value.PageSize);
        Assert.Equal(1, result.Value.TotalPages);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnFailure_WhenPageIsLessThanOne()
    {
        // Arrange
        int invalidPage = 0;

        // Act
        var result = await _productSearchService.SearchAsync("Test", null, invalidPage, 10, 1);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Номер сторінки повинен бути більше нуля.", result.Errors);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnFailure_WhenPageSizeIsLessThanOne()
    {
        // Arrange
        int invalidPageSize = 0;

        // Act
        var result = await _productSearchService.SearchAsync("Test", null, 1, invalidPageSize, 1);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Розмір сторінки має бути від 1 до 100.", result.Errors);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnFailure_WhenPageSizeIsMoreThanOneHundred()
    {
        // Arrange
        int invalidPageSize = 101;

        // Act
        var result = await _productSearchService.SearchAsync("Test", null, 1, invalidPageSize, 1);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Розмір сторінки має бути від 1 до 100.", result.Errors);
    }

    [Fact]
    public async Task SearchAsync_ShouldCalculateTotalPagesCorrectly()
    {
        // Arrange
        var foods = new List<Food>();
        // 25 items, pageSize = 10 -> 3 pages
        _mockFoodRepository
            .Setup(repo => repo.SearchAsync(null, null, 2, 10, 1))
            .ReturnsAsync((foods, 25));

        // Act
        var result = await _productSearchService.SearchAsync(null, null, 2, 10, 1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value!.TotalPages);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnEmptyItems_WhenNoMatches()
    {
        // Arrange
        _mockFoodRepository
            .Setup(repo => repo.SearchAsync("NonExistent", null, 1, 10, 1))
            .ReturnsAsync((new List<Food>(), 0));

        // Act
        var result = await _productSearchService.SearchAsync("NonExistent", null, 1, 10, 1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(0, result.Value!.TotalCount);
        Assert.Equal(0, result.Value!.TotalPages);
    }
}
