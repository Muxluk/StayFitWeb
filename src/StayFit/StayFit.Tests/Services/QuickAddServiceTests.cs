using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StayFit.Application.Interfaces;
using StayFit.Application.Options;
using StayFit.Application.Services;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using Xunit;

namespace StayFit.Tests.Services;

public class QuickAddServiceTests
{
    private readonly Mock<IMealRepository> _mealRepositoryMock;
    private readonly Mock<IFoodLogRepository> _foodLogRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<INutritionGoalRepository> _nutritionGoalRepositoryMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ILogger<QuickAddService>> _loggerMock;
    private readonly QuickAddService _quickAddService;

    public QuickAddServiceTests()
    {
        _mealRepositoryMock = new Mock<IMealRepository>();
        _foodLogRepositoryMock = new Mock<IFoodLogRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _nutritionGoalRepositoryMock = new Mock<INutritionGoalRepository>();
        _notificationServiceMock = new Mock<INotificationService>();
        _loggerMock = new Mock<ILogger<QuickAddService>>();

        var notificationSettings = Options.Create(new NotificationSettings());

        _quickAddService = new QuickAddService(
            _mealRepositoryMock.Object,
            _foodLogRepositoryMock.Object,
            _userRepositoryMock.Object,
            _nutritionGoalRepositoryMock.Object,
            _notificationServiceMock.Object,
            notificationSettings,
            _loggerMock.Object);
    }

    [Fact]
    public async Task QuickAddMealTodayAsync_SuccessfullyCopesMeal_WhenMealExists()
    {
        // Arrange
        const int mealId = 1;
        const string userEmail = "test@user.com";
        
        var sourceMeal = new MealEntry
        {
            Id = mealId,
            Name = "Сніданок",
            UserEmail = userEmail,
            Time = DateTime.UtcNow.AddHours(-24),
            Note = "Test note"
        };

        var foodLogs = new List<FoodLog>
        {
            new FoodLog { Id = 1, MealEntryId = mealId, FoodId = 10, AmountGrams = 100, UserId = 1 },
            new FoodLog { Id = 2, MealEntryId = mealId, FoodId = 20, AmountGrams = 150, UserId = 1 }
        };

        _mealRepositoryMock
            .Setup(r => r.GetByIdAsync(mealId))
            .ReturnsAsync(sourceMeal);

        _foodLogRepositoryMock
            .Setup(r => r.GetByMealIdAsync(mealId))
            .ReturnsAsync(foodLogs);

        _mealRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<MealEntry>()))
            .Returns(Task.CompletedTask);

        _foodLogRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<FoodLog>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _quickAddService.QuickAddMealTodayAsync(mealId, userEmail);

        // Assert
        Assert.True(result);
        _mealRepositoryMock.Verify(r => r.AddAsync(It.IsAny<MealEntry>()), Times.Once);
        _foodLogRepositoryMock.Verify(r => r.AddAsync(It.IsAny<FoodLog>()), Times.Exactly(2));
    }

    [Fact]
    public async Task QuickAddMealTodayAsync_ReturnsFalse_WhenMealNotFound()
    {
        // Arrange
        const int mealId = 999;
        const string userEmail = "test@user.com";

        _mealRepositoryMock
            .Setup(r => r.GetByIdAsync(mealId))
            .ReturnsAsync((MealEntry?)null);

        // Act
        var result = await _quickAddService.QuickAddMealTodayAsync(mealId, userEmail);

        // Assert
        Assert.False(result);
        _mealRepositoryMock.Verify(r => r.AddAsync(It.IsAny<MealEntry>()), Times.Never);
    }

    [Fact]
    public async Task QuickAddMealTodayAsync_ReturnsFalse_WhenMealBelongsToAnotherUser()
    {
        // Arrange
        const int mealId = 1;
        const string userEmail = "test@user.com";
        const string anotherUserEmail = "another@user.com";

        var meal = new MealEntry
        {
            Id = mealId,
            Name = "Сніданок",
            UserEmail = anotherUserEmail,
            Time = DateTime.UtcNow
        };

        _mealRepositoryMock
            .Setup(r => r.GetByIdAsync(mealId))
            .ReturnsAsync(meal);

        // Act
        var result = await _quickAddService.QuickAddMealTodayAsync(mealId, userEmail);

        // Assert
        Assert.False(result);
        _mealRepositoryMock.Verify(r => r.AddAsync(It.IsAny<MealEntry>()), Times.Never);
    }

    [Fact]
    public async Task QuickAddMealTodayAsync_ReturnsFalse_WhenMealHasNoFoodLogs()
    {
        // Arrange
        const int mealId = 1;
        const string userEmail = "test@user.com";

        var meal = new MealEntry
        {
            Id = mealId,
            Name = "Сніданок",
            UserEmail = userEmail,
            Time = DateTime.UtcNow
        };

        _mealRepositoryMock
            .Setup(r => r.GetByIdAsync(mealId))
            .ReturnsAsync(meal);

        _foodLogRepositoryMock
            .Setup(r => r.GetByMealIdAsync(mealId))
            .ReturnsAsync(Enumerable.Empty<FoodLog>());

        // Act
        var result = await _quickAddService.QuickAddMealTodayAsync(mealId, userEmail);

        // Assert
        Assert.False(result);
        _mealRepositoryMock.Verify(r => r.AddAsync(It.IsAny<MealEntry>()), Times.Never);
    }

    [Fact]
    public async Task QuickAddMealTodayAsync_CopiesMealNote_WhenNoteExists()
    {
        // Arrange
        const int mealId = 1;
        const string userEmail = "test@user.com";
        const string noteText = "Important note about this meal";

        var sourceMeal = new MealEntry
        {
            Id = mealId,
            Name = "Сніданок",
            UserEmail = userEmail,
            Time = DateTime.UtcNow.AddHours(-24),
            Note = noteText
        };

        var foodLogs = new List<FoodLog>
        {
            new FoodLog { Id = 1, MealEntryId = mealId, FoodId = 10, AmountGrams = 100, UserId = 1 }
        };

        MealEntry createdMeal = null!;

        _mealRepositoryMock
            .Setup(r => r.GetByIdAsync(mealId))
            .ReturnsAsync(sourceMeal);

        _foodLogRepositoryMock
            .Setup(r => r.GetByMealIdAsync(mealId))
            .ReturnsAsync(foodLogs);

        _mealRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<MealEntry>()))
            .Callback<MealEntry>(meal => createdMeal = meal)
            .Returns(Task.CompletedTask);

        _foodLogRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<FoodLog>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _quickAddService.QuickAddMealTodayAsync(mealId, userEmail);

        // Assert
        Assert.True(result);
        Assert.Equal(noteText, createdMeal.Note);
    }

    [Fact]
    public async Task QuickAddMealTodayAsync_ReturnsFalse_WhenRepositoryThrowsException()
    {
        // Arrange
        const int mealId = 1;
        const string userEmail = "test@user.com";

        _mealRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _quickAddService.QuickAddMealTodayAsync(mealId, userEmail);

        // Assert
        Assert.False(result);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}