using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using StayFit.Application.Services;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Tests.Services;

public class MealServiceTests
{
    private readonly Mock<IMealRepository> _mealRepoMock;
    private readonly Mock<ILogger<MealService>> _loggerMock;
    private readonly MealService _mealService;

    public MealServiceTests()
    {
        _mealRepoMock = new Mock<IMealRepository>();
        _loggerMock = new Mock<ILogger<MealService>>();
        
        _mealService = new MealService(_mealRepoMock.Object, _loggerMock.Object);
    }

    #region Позитивні сценарії (Positive Tests)

    [Fact]
    public async Task CreateMealAsync_ShouldCallRepository_WhenDataIsValid()
    {
        var meal = new MealEntry 
        { 
            Name = "Сніданок", 
            UserEmail = "test@user.com", 
            Time = DateTime.Now 
        };

        await _mealService.CreateMealAsync(meal);

        _mealRepoMock.Verify(r => r.AddAsync(meal), Times.Once);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Attempting to create")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetUserMealsAsync_ShouldReturnListOfMeals()
    {
        var email = "user@test.com";
        var expectedMeals = new List<MealEntry> 
        { 
            new() { Name = "Обід", UserEmail = email } 
        };
        
        _mealRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedMeals);

        var result = await _mealService.GetUserMealsAsync(email);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Обід", result.First().Name);
    }

    #endregion

    #region Негативні сценарії (Negative Tests)

    [Fact]
    public async Task CreateMealAsync_ShouldLogErrorAndThrow_WhenRepositoryFails()
    {
        var meal = new MealEntry { Name = "Вечеря" };
        var exceptionMessage = "Database connection failed";
        
        _mealRepoMock.Setup(r => r.AddAsync(It.IsAny<MealEntry>()))
                     .ThrowsAsync(new Exception(exceptionMessage));

    
        var exception = await Assert.ThrowsAsync<Exception>(() => _mealService.CreateMealAsync(meal));
        Assert.Equal(exceptionMessage, exception.Message);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error occurred")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion
}