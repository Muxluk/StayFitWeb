using Microsoft.Extensions.Logging;
using Moq;
using StayFit.Application.Services;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using Xunit;

namespace StayFit.Tests.Services;

public class FoodServiceTests
{
    private readonly Mock<IFoodRepository> _mockRepo;
    private readonly Mock<ILogger<FoodService>> _mockLogger;
    private readonly FoodService _foodService;

    public FoodServiceTests()
    {
        _mockRepo = new Mock<IFoodRepository>();
        _mockLogger = new Mock<ILogger<FoodService>>();
        _foodService = new FoodService(_mockRepo.Object, _mockLogger.Object);
    }

    // Позитивні сценарії
    [Fact]
    public async Task AddFoodAsync_ShouldCallRepositoryAddAsync()
    {
        // Arrange
        var food = new Food { Name = "Яблуко", CaloriesPer100g = 52 };
        const int userId = 7;

        // Act
        await _foodService.AddFoodAsync(food, userId);

        // Assert
        Assert.Equal(userId, food.OwnerUserId);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Food>()), Times.Once);
    }

    [Fact]
    public async Task GetFoodByIdAsync_ShouldReturnFood_WhenFoodExists()
    {
        // Arrange
        const int userId = 7;
        var expectedFood = new Food { Id = 1, Name = "Банан" };
        _mockRepo.Setup(r => r.GetByIdAndOwnerAsync(1, userId)).ReturnsAsync(expectedFood);

        // Act
        var result = await _foodService.GetFoodByIdAsync(1, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedFood.Id, result.Id);
        Assert.Equal(expectedFood.Name, result.Name);
    }

    [Fact]
    public async Task DeleteFoodAsync_ShouldCallRepositoryDelete_WhenFoodExists()
    {
        // Arrange
        const int userId = 7;
        var food = new Food { Id = 1, Name = "Курка" };
        _mockRepo.Setup(r => r.GetByIdAndOwnerAsync(1, userId)).ReturnsAsync(food);

        // Act
        await _foodService.DeleteFoodAsync(1, userId);

        // Assert
        _mockRepo.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    // Негативні сценарії
    [Fact]
    public async Task AddFoodAsync_ShouldThrowArgumentNullException_WhenFoodIsNull()
    {
        // Arrange
        Food nullFood = null!;
        const int userId = 7;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _foodService.AddFoodAsync(nullFood, userId));
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Food>()), Times.Never);
    }

    [Fact]
    public async Task GetFoodByIdAsync_ShouldReturnNull_WhenFoodDoesNotExist()
    {
        // Arrange
        const int userId = 7;
        _mockRepo.Setup(r => r.GetByIdAndOwnerAsync(99, userId)).ReturnsAsync((Food?)null);

        // Act
        var result = await _foodService.GetFoodByIdAsync(99, userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteFoodAsync_ShouldThrowKeyNotFoundException_WhenFoodDoesNotExist()
    {
        // Arrange
        const int userId = 7;
        _mockRepo.Setup(r => r.GetByIdAndOwnerAsync(99, userId)).ReturnsAsync((Food?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _foodService.DeleteFoodAsync(99, userId));
        Assert.Equal("Продукт з ID 99 не знайдено.", exception.Message);

        _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
}
