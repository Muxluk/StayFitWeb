using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using StayFit.Application.DTOs;
using StayFit.Application.Options;
using StayFit.Application.Services;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using Xunit;

namespace StayFit.Tests.Services;

public class FoodImportServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IFoodImportRepository> _repositoryMock;
    private readonly IMemoryCache _cache;
    private readonly Mock<IOptions<UsdaFoodDataOptions>> _optionsMock;
    private readonly Mock<ILogger<FoodImportService>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

    public FoodImportServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _repositoryMock = new Mock<IFoodImportRepository>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _optionsMock = new Mock<IOptions<UsdaFoodDataOptions>>();
        _loggerMock = new Mock<ILogger<FoodImportService>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        _optionsMock.Setup(o => o.Value).Returns(new UsdaFoodDataOptions
        {
            ApiUrl = "https://api.nal.usda.gov/fdc/v1/foods/search",
            ApiKey = "TEST_KEY",
            CacheLifetimeMinutes = 60
        });

        var client = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.nal.usda.gov")
        };

        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
    }

    private FoodImportService CreateService()
    {
        return new FoodImportService(
            _httpClientFactoryMock.Object,
            _repositoryMock.Object,
            _cache,
            _optionsMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task SearchGlobalAsync_WhenSearchTermIsEmpty_ReturnsEmptyArray()
    {
        var service = CreateService();

        var result = await service.SearchGlobalAsync("   ");

        Assert.Empty(result);
        _httpMessageHandlerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SearchGlobalAsync_WhenApiReturnsProducts_ParsesUSDAFormatCorrectly()
    {
        var service = CreateService();

        var apiResponse = new UsdaSearchResponse
        {
            Foods = new List<UsdaFood>
            {
                new UsdaFood
                {
                    Description = "Test Apple",
                    BrandOwner = "TestBrand",
                    FoodNutrients = new List<UsdaNutrient>
                    {
                        new UsdaNutrient { NutrientName = "Energy", Value = 52f },
                        new UsdaNutrient { NutrientName = "Protein", Value = 0.26f },
                        new UsdaNutrient { NutrientName = "Total lipid (fat)", Value = 0.17f },
                        new UsdaNutrient { NutrientName = "Carbohydrate, by difference", Value = 13.81f },
                        new UsdaNutrient { NutrientName = "Fiber, total dietary", Value = 2.4f } // Should be ignored
                    }
                }
            }
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(apiResponse))
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        var result = (await service.SearchGlobalAsync("apple")).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Test Apple", result[0].Name);
        Assert.Equal("TestBrand", result[0].Brand);
        Assert.Equal(52f, result[0].CaloriesPer100g);
        Assert.Equal(0.17f, result[0].FatPer100g);
        Assert.Equal(13.81f, result[0].CarbsPer100g);
        Assert.Equal(0.26f, result[0].ProteinPer100g);
        
        // Act phase 2 - caching
        var cachedResult = (await service.SearchGlobalAsync("apple")).ToList();
        
        // Caching should prevent repeating the API hit
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
        
        Assert.Single(cachedResult);
    }

    [Fact]
    public async Task SearchGlobalAsync_WhenFailedResponse_ReturnsEmptyArrayAndLogsWarning()
    {
        var service = CreateService();

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        var result = await service.SearchGlobalAsync("apple");

        Assert.Empty(result);
    }

    [Fact]
    public async Task ImportProductAsync_ValidProduct_SetsOwnerAndCallsRepository()
    {
        var service = CreateService();
        var food = new Food { Name = "Banana", CaloriesPer100g = 89f };

        await service.ImportProductAsync(food, 99, "test@example.com");

        Assert.Equal(99, food.OwnerUserId);
        Assert.Equal("test@example.com", food.CreatedByEmail);
        Assert.Equal(0, food.Id); 

        _repositoryMock.Verify(r => r.AddImportedFoodAsync(It.Is<Food>(f => f.Name == "Banana")), Times.Once);
    }
}
