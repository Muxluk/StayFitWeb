using Microsoft.Extensions.Logging;
using Moq;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Services;

namespace StayFit.Tests.Services;

public sealed class StatisticsServiceTests
{
    [Fact]
    public async Task GetSummaryAsync_WhenEmailIsEmpty_ReturnsFailure()
    {
        var statisticsRepositoryMock = new Mock<IStatisticsRepository>();
        var userRepositoryMock = new Mock<IUserRepository>();
        var sut = CreateSut(statisticsRepositoryMock, userRepositoryMock);

        var result = await sut.GetSummaryAsync(string.Empty, DateTime.Today, DateTime.Today);

        Assert.True(result.IsFailure);
        Assert.Contains("Email користувача не вказано.", result.Errors);

        userRepositoryMock.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        statisticsRepositoryMock.Verify(
            r => r.GetNutritionSummaryAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetSummaryAsync_WhenDateRangeIsInvalid_ReturnsFailure()
    {
        var statisticsRepositoryMock = new Mock<IStatisticsRepository>();
        var userRepositoryMock = new Mock<IUserRepository>();
        var sut = CreateSut(statisticsRepositoryMock, userRepositoryMock);

        var result = await sut.GetSummaryAsync("user@example.com", new DateTime(2026, 3, 26), new DateTime(2026, 3, 25));

        Assert.True(result.IsFailure);
        Assert.Contains("Початкова дата не може бути пізніше кінцевої.", result.Errors);

        userRepositoryMock.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        statisticsRepositoryMock.Verify(
            r => r.GetNutritionSummaryAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetSummaryAsync_WhenUserNotFound_ReturnsFailure()
    {
        var statisticsRepositoryMock = new Mock<IStatisticsRepository>();
        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock.Setup(r => r.GetByEmailAsync("missing@example.com")).ReturnsAsync((User?)null);

        var sut = CreateSut(statisticsRepositoryMock, userRepositoryMock);

        var result = await sut.GetSummaryAsync("missing@example.com", new DateTime(2026, 3, 25), new DateTime(2026, 3, 25));

        Assert.True(result.IsFailure);
        Assert.Contains("Користувача не знайдено.", result.Errors);

        statisticsRepositoryMock.Verify(
            r => r.GetNutritionSummaryAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetSummaryAsync_WhenInputIsValid_ReturnsSuccessWithSummary()
    {
        var periodStart = new DateTime(2026, 3, 20);
        var periodEnd = new DateTime(2026, 3, 25);

        var user = new User { Id = 42, Email = "user@example.com", Name = "user" };
        var summary = new NutritionSummary
        {
            StartDate = periodStart,
            EndDate = periodEnd,
            TotalCalories = 1500m,
            TotalProtein = 100m,
            TotalFat = 60m,
            TotalCarbs = 170m,
            DaysWithLogs = 4,
        };

        var statisticsRepositoryMock = new Mock<IStatisticsRepository>();
        statisticsRepositoryMock
            .Setup(r => r.GetNutritionSummaryAsync(42, periodStart, periodEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock.Setup(r => r.GetByEmailAsync("user@example.com")).ReturnsAsync(user);

        var sut = CreateSut(statisticsRepositoryMock, userRepositoryMock);

        var result = await sut.GetSummaryAsync("user@example.com", periodStart, periodEnd);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1500m, result.Value!.TotalCalories);
        Assert.Equal(4, result.Value.DaysWithLogs);
        Assert.Empty(result.Errors);

        statisticsRepositoryMock.Verify(
            r => r.GetNutritionSummaryAsync(42, periodStart, periodEnd, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSummaryAsync_NormalizesTimePart_WhenCallingRepository()
    {
        var user = new User { Id = 7, Email = "user@example.com", Name = "name" };

        var fromWithTime = new DateTime(2026, 3, 25, 16, 12, 10);
        var toWithTime = new DateTime(2026, 3, 26, 23, 59, 59);
        DateTime capturedFrom = default;
        DateTime capturedTo = default;

        var statisticsRepositoryMock = new Mock<IStatisticsRepository>();
        statisticsRepositoryMock
            .Setup(r => r.GetNutritionSummaryAsync(7, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Callback<int, DateTime, DateTime, CancellationToken>((_, from, to, _) =>
            {
                capturedFrom = from;
                capturedTo = to;
            })
            .ReturnsAsync(new NutritionSummary());

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock.Setup(r => r.GetByEmailAsync("user@example.com")).ReturnsAsync(user);

        var sut = CreateSut(statisticsRepositoryMock, userRepositoryMock);

        var result = await sut.GetSummaryAsync("user@example.com", fromWithTime, toWithTime);

        Assert.True(result.IsSuccess);
        Assert.Equal(fromWithTime.Date, capturedFrom);
        Assert.Equal(toWithTime.Date, capturedTo);
    }

    [Fact]
    public async Task GetSummaryAsync_WhenRepositoryThrows_PropagatesException()
    {
        var user = new User { Id = 3, Email = "user@example.com", Name = "user" };

        var statisticsRepositoryMock = new Mock<IStatisticsRepository>();
        statisticsRepositoryMock
            .Setup(r => r.GetNutritionSummaryAsync(3, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db failure"));

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock.Setup(r => r.GetByEmailAsync("user@example.com")).ReturnsAsync(user);

        var sut = CreateSut(statisticsRepositoryMock, userRepositoryMock);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.GetSummaryAsync("user@example.com", new DateTime(2026, 3, 25), new DateTime(2026, 3, 26)));
    }

    private static StatisticsService CreateSut(
        Mock<IStatisticsRepository> statisticsRepositoryMock,
        Mock<IUserRepository> userRepositoryMock)
    {
        var loggerMock = new Mock<ILogger<StatisticsService>>();
        return new StatisticsService(statisticsRepositoryMock.Object, userRepositoryMock.Object, loggerMock.Object);
    }
}
