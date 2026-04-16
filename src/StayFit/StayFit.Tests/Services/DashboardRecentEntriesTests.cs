using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StayFit.Application.Options;
using StayFit.Application.Services;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Tests.Services;

public sealed class DashboardRecentEntriesTests
{
    [Fact]
    public async Task GetTodayDashboardAsync_WhenConfiguredCountIsTwo_LoadsExactlyTwoRecentEntries()
    {
        var user = CreateUser(42, "user@example.com");
        var latestLogs = CreateRecentLogs(user.Id, 2);

        var (sut, foodLogRepositoryMock) = CreateSutWithRecentLogs(
            user,
            latestLogs,
            recentEntriesCount: 2);

        var result = await sut.GetTodayDashboardAsync(42, "user@example.com");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.RecentDiaryEntries.Count);

        foodLogRepositoryMock.Verify(r => r.GetLatestByUserIdAsync(user.Id, 2), Times.Once);
    }

    [Fact]
    public async Task GetTodayDashboardAsync_WhenFoodMissing_UsesFallbackFoodName()
    {
        var user = CreateUser(7, "user@example.com");
        var now = DateTime.UtcNow;

        var latestLogs = new List<FoodLog>
        {
            new()
            {
                UserId = user.Id,
                AmountGrams = 100,
                LoggedAt = now,
                Food = null!
            }
        };

        var (sut, _) = CreateSutWithRecentLogs(user, latestLogs, recentEntriesCount: 1);

        var result = await sut.GetTodayDashboardAsync(7, "user@example.com");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value!.RecentDiaryEntries);
        Assert.Equal("Невідомий продукт", result.Value.RecentDiaryEntries[0].FoodName);
    }

    [Fact]
    public async Task GetTodayDashboardAsync_WhenRecentEntryMapped_CalculatesMacrosByAmount()
    {
        var user = CreateUser(9, "user@example.com");
        var now = DateTime.UtcNow;

        var latestLogs = new List<FoodLog>
        {
            new()
            {
                UserId = user.Id,
                AmountGrams = 150,
                LoggedAt = now,
                Food = new Food
                {
                    Name = "Тест продукт",
                    CaloriesPer100g = 200,
                    ProteinPer100g = 10,
                    FatPer100g = 6,
                    CarbsPer100g = 20
                }
            }
        };

        var (sut, _) = CreateSutWithRecentLogs(user, latestLogs, recentEntriesCount: 1);

        var result = await sut.GetTodayDashboardAsync(9, "user@example.com");

        Assert.True(result.IsSuccess);
        var entry = Assert.Single(result.Value!.RecentDiaryEntries);
        Assert.Equal(300f, entry.Calories);
        Assert.Equal(15f, entry.Protein);
        Assert.Equal(9f, entry.Fat);
        Assert.Equal(30f, entry.Carbs);
    }

    [Fact]
    public async Task GetTodayDashboardAsync_WhenConfiguredCountInvalid_UsesDefaultFive()
    {
        var user = CreateUser(11, "user@example.com");

        var (sut, foodLogRepositoryMock) = CreateSutWithRecentLogs(
            user,
            Array.Empty<FoodLog>(),
            recentEntriesCount: -3);

        var result = await sut.GetTodayDashboardAsync(11, "user@example.com");

        Assert.True(result.IsSuccess);
        foodLogRepositoryMock.Verify(r => r.GetLatestByUserIdAsync(user.Id, 5), Times.Once);
    }

    private static (DashboardService sut, Mock<IFoodLogRepository> foodLogRepositoryMock) CreateSutWithRecentLogs(
        User user,
        IEnumerable<FoodLog> latestLogs,
        int recentEntriesCount)
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);

        var foodLogRepositoryMock = new Mock<IFoodLogRepository>();
        foodLogRepositoryMock
            .Setup(r => r.GetByUserIdAndDateAsync(user.Id, It.IsAny<DateTime>()))
            .ReturnsAsync(Array.Empty<FoodLog>());
        foodLogRepositoryMock
            .Setup(r => r.GetLatestByUserIdAsync(user.Id, It.IsAny<int>()))
            .ReturnsAsync(latestLogs);

        var nutritionGoalRepositoryMock = new Mock<INutritionGoalRepository>();
        nutritionGoalRepositoryMock
            .Setup(r => r.GetByUserIdAsync(user.Id.ToString()))
            .ReturnsAsync(new NutritionGoal
            {
                UserId = user.Id.ToString(),
                CaloriesGoal = 2000,
                ProteinGoal = 120,
                FatGoal = 70,
                CarbsGoal = 250
            });

        var options = Options.Create(new DashboardSettings
        {
            RecentDiaryEntriesCount = recentEntriesCount
        });

        var loggerMock = new Mock<ILogger<DashboardService>>();
        var sut = new DashboardService(
            foodLogRepositoryMock.Object,
            nutritionGoalRepositoryMock.Object,
            userRepositoryMock.Object,
            options,
            loggerMock.Object);

        return (sut, foodLogRepositoryMock);
    }

    private static User CreateUser(int id, string email) => new()
    {
        Id = id,
        Email = email,
        Name = "Test User"
    };

    private static List<FoodLog> CreateRecentLogs(int userId, int count)
    {
        var now = DateTime.UtcNow;
        var logs = new List<FoodLog>();

        for (var i = 0; i < count; i++)
        {
            logs.Add(new FoodLog
            {
                UserId = userId,
                AmountGrams = 100,
                LoggedAt = now.AddMinutes(-i),
                Food = new Food
                {
                    Name = $"Food {i + 1}",
                    CaloriesPer100g = 100,
                    ProteinPer100g = 10,
                    FatPer100g = 5,
                    CarbsPer100g = 15
                }
            });
        }

        return logs;
    }
}