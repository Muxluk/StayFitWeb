using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Application.Options;
using StayFit.Application.Services;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using Xunit;

namespace StayFit.Tests.Services;

public class BarcodeScanServiceTests
{
    private readonly Mock<IUsdaFoodDataClient> _usdaClientMock;
    private readonly Mock<IBarcodeScanRepository> _repoMock;
    private readonly Mock<ILogger<BarcodeScanService>> _loggerMock;
    private readonly IMemoryCache _cache;
    private readonly IOptions<UsdaFoodDataOptions> _options;
    private readonly BarcodeScanService _service;

    public BarcodeScanServiceTests()
    {
        _usdaClientMock = new Mock<IUsdaFoodDataClient>();
        _repoMock = new Mock<IBarcodeScanRepository>();
        _loggerMock = new Mock<ILogger<BarcodeScanService>>();
        
        _cache = new MemoryCache(new MemoryCacheOptions());
        _options = Options.Create(new UsdaFoodDataOptions { CacheLifetimeMinutes = 60 });

        _service = new BarcodeScanService(
            _usdaClientMock.Object,
            _repoMock.Object,
            _cache,
            _options,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ScanBarcodeAsync_NullOrEmptyBarcode_ReturnsNull()
    {
        // Act
        var result = await _service.ScanBarcodeAsync("", 1);

        // Assert
        Assert.Null(result);
        _usdaClientMock.Verify(x => x.SearchByBarcodeAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ScanBarcodeAsync_ApiReturnsNull_ReturnsNull()
    {
        // Arrange
        _usdaClientMock.Setup(x => x.SearchByBarcodeAsync("123"))
            .ReturnsAsync((UsdaSearchResponse?)null);

        // Act
        var result = await _service.ScanBarcodeAsync("123", 1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ScanBarcodeAsync_ApiReturnsEmptyFoods_ReturnsNull()
    {
        // Arrange
        _usdaClientMock.Setup(x => x.SearchByBarcodeAsync("123"))
            .ReturnsAsync(new UsdaSearchResponse { Foods = new List<UsdaFood>() });

        // Act
        var result = await _service.ScanBarcodeAsync("123", 1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ScanBarcodeAsync_ProductFoundNotInDb_ReturnsResultWithExistsFalse()
    {
        // Arrange
        var apiResponse = new UsdaSearchResponse
        {
            Foods = new List<UsdaFood>
            {
                new UsdaFood
                {
                    Description = "Test Food",
                    BrandOwner = "Test Brand",
                    FoodNutrients = new List<UsdaNutrient>
                    {
                        new UsdaNutrient { NutrientName = "Energy", Value = 100 },
                        new UsdaNutrient { NutrientName = "Protein", Value = 10 }
                    }
                }
            }
        };

        _usdaClientMock.Setup(x => x.SearchByBarcodeAsync("123"))
            .ReturnsAsync(apiResponse);

        _repoMock.Setup(x => x.GetFoodByBarcodeAsync("123", 1))
            .ReturnsAsync((Food?)null);

        // Act
        var result = await _service.ScanBarcodeAsync("123", 1);

        // Assert
        Assert.NotNull(result);
        Assert.False(result!.ExistsInLocalDb);
        Assert.Equal("Test Food", result.Food.Name);
        Assert.Equal("Test Brand", result.Food.Brand);
        Assert.Equal("123", result.Food.Barcode);
        Assert.Equal(100, result.Food.CaloriesPer100g);
        Assert.Equal(10, result.Food.ProteinPer100g);
        
        // Verify it was cached
        var cached = _cache.TryGetValue("UsdaFoodData_Barcode_123", out Food? cachedFood);
        Assert.True(cached);
        Assert.NotNull(cachedFood);
    }

    [Fact]
    public async Task ScanBarcodeAsync_ProductFoundExistsInDb_ReturnsResultWithExistsTrue()
    {
        // Arrange
        var apiResponse = new UsdaSearchResponse
        {
            Foods = new List<UsdaFood>
            {
                new UsdaFood
                {
                    Description = "Existing Food",
                    FoodNutrients = new List<UsdaNutrient>()
                }
            }
        };

        _usdaClientMock.Setup(x => x.SearchByBarcodeAsync("456"))
            .ReturnsAsync(apiResponse);

        var existingFood = new Food { Id = 5, Name = "Existing Food", Barcode = "456" };
        _repoMock.Setup(x => x.GetFoodByBarcodeAsync("456", 1))
            .ReturnsAsync(existingFood);

        // Act
        var result = await _service.ScanBarcodeAsync("456", 1);

        // Assert
        Assert.NotNull(result);
        Assert.True(result!.ExistsInLocalDb);
        Assert.Equal("Existing Food", result.Food.Name);
    }

    [Fact]
    public async Task ScanBarcodeAsync_ProductInCache_DoesNotCallApi()
    {
        // Arrange
        var cachedFood = new Food { Name = "Cached Food", Barcode = "789" };
        _cache.Set("UsdaFoodData_Barcode_789", cachedFood);
        
        _repoMock.Setup(x => x.GetFoodByBarcodeAsync("789", 1))
            .ReturnsAsync(cachedFood);

        // Act
        var result = await _service.ScanBarcodeAsync("789", 1);

        // Assert
        Assert.NotNull(result);
        Assert.True(result!.ExistsInLocalDb);
        Assert.Equal("Cached Food", result.Food.Name);
        
        _usdaClientMock.Verify(x => x.SearchByBarcodeAsync(It.IsAny<string>()), Times.Never);
    }
}
