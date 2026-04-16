using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Moq;
using StayFit.Application.Services;
using StayFit.Application.Options;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Tests.Services;

public sealed class DashboardServiceTests
{
    [Fact]
    public async Task GetTodayDashboardAsync_WhenGoalMissing_ReturnsFailure()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.GetByEmailAsync("user@example.com"))
            .ReturnsAsync(new User { Id = 10, Email = "user@example.com", Name = "Test User" });

        var foodLogRepositoryMock = new Mock<IFoodLogRepository>();
        foodLogRepositoryMock
            .Setup(r => r.GetByUserIdAndDateAsync(10, It.IsAny<DateTime>()))
            .ReturnsAsync(Array.Empty<FoodLog>());
        foodLogRepositoryMock
            .Setup(r => r.GetLatestByUserIdAsync(10, It.IsAny<int>()))
            .ReturnsAsync(Array.Empty<FoodLog>());

        var nutritionGoalRepositoryMock = new Mock<INutritionGoalRepository>();
        nutritionGoalRepositoryMock
            .Setup(r => r.GetByUserIdAsync("123"))
            .ReturnsAsync((NutritionGoal?)null);

        var sut = CreateSut(foodLogRepositoryMock, nutritionGoalRepositoryMock, userRepositoryMock);

        var result = await sut.GetTodayDashboardAsync(123, "user@example.com");

        Assert.True(result.IsFailure);
        Assert.Contains("Цілі харчування не встановлені", result.Errors);
    }

    [Fact]
    public async Task GetTodayDashboardAsync_WhenInputValid_ReturnsCalculatedDashboard()
    {
        var today = DateTime.Today;

        var user = new User
        {
            Id = 7,
            Email = "user@example.com",
            Name = "Test User"
        };

        var foodLogs = new List<FoodLog>
        {
            new()
            {
                UserId = user.Id,
                AmountGrams = 150,
                LoggedAt = today,
                Food = new Food
                {
                    Name = "Food 1",
                    CaloriesPer100g = 200,
                    ProteinPer100g = 10,
                    FatPer100g = 5,
                    CarbsPer100g = 30
                }
            },
            new()
            {
                UserId = user.Id,
                AmountGrams = 50,
                LoggedAt = today,
                Food = new Food
                {
                    Name = "Food 2",
                    CaloriesPer100g = 100,
                    ProteinPer100g = 20,
                    FatPer100g = 10,
                    CarbsPer100g = 0
                }
            }
        };

        var goal = new NutritionGoal
        {
            UserId = "77",
            CaloriesGoal = 2200,
            ProteinGoal = 120,
            FatGoal = 70,
            CarbsGoal = 250
        };

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.GetByEmailAsync("user@example.com"))
            .ReturnsAsync(user);

        var foodLogRepositoryMock = new Mock<IFoodLogRepository>();
        foodLogRepositoryMock
            .Setup(r => r.GetByUserIdAndDateAsync(user.Id, today))
            .ReturnsAsync(foodLogs);
        foodLogRepositoryMock
            .Setup(r => r.GetLatestByUserIdAsync(user.Id, It.IsAny<int>()))
            .ReturnsAsync(foodLogs);

        var nutritionGoalRepositoryMock = new Mock<INutritionGoalRepository>();
        nutritionGoalRepositoryMock
            .Setup(r => r.GetByUserIdAsync("77"))
            .ReturnsAsync(goal);

        var sut = CreateSut(foodLogRepositoryMock, nutritionGoalRepositoryMock, userRepositoryMock);

        var result = await sut.GetTodayDashboardAsync(77, "user@example.com");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(350f, result.Value!.ActualCalories);
        Assert.Equal(25f, result.Value.ActualProtein);
        Assert.Equal(12.5f, result.Value.ActualFat);
        Assert.Equal(45f, result.Value.ActualCarbs);

        Assert.Equal(2200f, result.Value.TargetCalories);
        Assert.Equal(120f, result.Value.TargetProtein);
        Assert.Equal(70f, result.Value.TargetFat);
        Assert.Equal(250f, result.Value.TargetCarbs);
        Assert.Equal(2, result.Value.RecentDiaryEntries.Count);
    }

    [Fact]
    public async Task GetTodayDashboardAsync_WhenUserNotFound_UsesEmptyLogsAndReturnsSuccess()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.GetByEmailAsync("missing@example.com"))
            .ReturnsAsync((User?)null);

        var foodLogRepositoryMock = new Mock<IFoodLogRepository>();

        var nutritionGoalRepositoryMock = new Mock<INutritionGoalRepository>();
        nutritionGoalRepositoryMock
            .Setup(r => r.GetByUserIdAsync("5"))
            .ReturnsAsync(new NutritionGoal
            {
                UserId = "5",
                CaloriesGoal = 1800,
                ProteinGoal = 100,
                FatGoal = 60,
                CarbsGoal = 200
            });

        var sut = CreateSut(foodLogRepositoryMock, nutritionGoalRepositoryMock, userRepositoryMock);

        var result = await sut.GetTodayDashboardAsync(5, "missing@example.com");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(0f, result.Value!.ActualCalories);
        Assert.Equal(0f, result.Value.ActualProtein);
        Assert.Equal(0f, result.Value.ActualFat);
        Assert.Equal(0f, result.Value.ActualCarbs);
        Assert.Empty(result.Value.RecentDiaryEntries);

        foodLogRepositoryMock.Verify(
            r => r.GetByUserIdAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()),
            Times.Never);

        foodLogRepositoryMock.Verify(
            r => r.GetLatestByUserIdAsync(It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task GetTodayDashboardAsync_WhenRecentCountConfigured_UsesConfiguredValueAndMapsEntries()
    {
        var user = new User { Id = 12, Email = "user@example.com", Name = "User" };
        var now = DateTime.UtcNow;

        var latestLogs = new List<FoodLog>
        {
            new()
            {
                UserId = user.Id,
                AmountGrams = 120,
                LoggedAt = now,
                Food = new Food
                {
                    Name = "Вівсянка",
                    CaloriesPer100g = 300,
                    ProteinPer100g = 12,
                    FatPer100g = 5,
                    CarbsPer100g = 55
                }
            }
        };

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.GetByEmailAsync("user@example.com"))
            .ReturnsAsync(user);

        var foodLogRepositoryMock = new Mock<IFoodLogRepository>();
        foodLogRepositoryMock
            .Setup(r => r.GetByUserIdAndDateAsync(user.Id, It.IsAny<DateTime>()))
            .ReturnsAsync(Array.Empty<FoodLog>());
        foodLogRepositoryMock
            .Setup(r => r.GetLatestByUserIdAsync(user.Id, 3))
            .ReturnsAsync(latestLogs);

        var nutritionGoalRepositoryMock = new Mock<INutritionGoalRepository>();
        nutritionGoalRepositoryMock
            .Setup(r => r.GetByUserIdAsync("12"))
            .ReturnsAsync(new NutritionGoal
            {
                UserId = "12",
                CaloriesGoal = 2000,
                ProteinGoal = 110,
                FatGoal = 65,
                CarbsGoal = 240
            });

        var sut = CreateSut(foodLogRepositoryMock, nutritionGoalRepositoryMock, userRepositoryMock, 3);

        var result = await sut.GetTodayDashboardAsync(12, "user@example.com");

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.RecentDiaryEntries);
        Assert.Equal("Вівсянка", result.Value.RecentDiaryEntries[0].FoodName);
        Assert.Equal(360f, result.Value.RecentDiaryEntries[0].Calories);

        foodLogRepositoryMock.Verify(r => r.GetLatestByUserIdAsync(user.Id, 3), Times.Once);
    }

    [Fact]
    public async Task GetTodayDashboardAsync_WhenRecentCountInvalid_FallsBackToDefaultValue()
    {
        var user = new User { Id = 20, Email = "user@example.com", Name = "User" };

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.GetByEmailAsync("user@example.com"))
            .ReturnsAsync(user);

        var foodLogRepositoryMock = new Mock<IFoodLogRepository>();
        foodLogRepositoryMock
            .Setup(r => r.GetByUserIdAndDateAsync(user.Id, It.IsAny<DateTime>()))
            .ReturnsAsync(Array.Empty<FoodLog>());
        foodLogRepositoryMock
            .Setup(r => r.GetLatestByUserIdAsync(user.Id, 5))
            .ReturnsAsync(Array.Empty<FoodLog>());

        var nutritionGoalRepositoryMock = new Mock<INutritionGoalRepository>();
        nutritionGoalRepositoryMock
            .Setup(r => r.GetByUserIdAsync("20"))
            .ReturnsAsync(new NutritionGoal
            {
                UserId = "20",
                CaloriesGoal = 1800,
                ProteinGoal = 100,
                FatGoal = 60,
                CarbsGoal = 200
            });

        var sut = CreateSut(foodLogRepositoryMock, nutritionGoalRepositoryMock, userRepositoryMock, 0);

        var result = await sut.GetTodayDashboardAsync(20, "user@example.com");

        Assert.True(result.IsSuccess);
        foodLogRepositoryMock.Verify(r => r.GetLatestByUserIdAsync(user.Id, 5), Times.Once);
    }

    private static DashboardService CreateSut(
        Mock<IFoodLogRepository> foodLogRepositoryMock,
        Mock<INutritionGoalRepository> nutritionGoalRepositoryMock,
        Mock<IUserRepository> userRepositoryMock,
        int recentDiaryEntriesCount = 5)
    {
        var loggerMock = new Mock<ILogger<DashboardService>>();
        var options = Options.Create(new DashboardSettings
        {
            RecentDiaryEntriesCount = recentDiaryEntriesCount
        });

        return new DashboardService(
            foodLogRepositoryMock.Object,
            nutritionGoalRepositoryMock.Object,
            userRepositoryMock.Object,
            options,
            loggerMock.Object);
    }
}
