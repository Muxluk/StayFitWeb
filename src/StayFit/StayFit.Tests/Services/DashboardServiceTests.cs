using Microsoft.Extensions.Logging;
using Moq;
using StayFit.Application.Services;
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

        foodLogRepositoryMock.Verify(
            r => r.GetByUserIdAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()),
            Times.Never);
    }

    private static DashboardService CreateSut(
        Mock<IFoodLogRepository> foodLogRepositoryMock,
        Mock<INutritionGoalRepository> nutritionGoalRepositoryMock,
        Mock<IUserRepository> userRepositoryMock)
    {
        var loggerMock = new Mock<ILogger<DashboardService>>();
        return new DashboardService(
            foodLogRepositoryMock.Object,
            nutritionGoalRepositoryMock.Object,
            userRepositoryMock.Object,
            loggerMock.Object);
    }
}
