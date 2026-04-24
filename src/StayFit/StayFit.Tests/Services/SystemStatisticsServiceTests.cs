using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StayFit.Application.Configuration;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Application.Services;
using Xunit;

namespace StayFit.Tests.Services;

public class SystemStatisticsServiceTests
{
    private readonly Mock<ISystemStatisticsRepository> _repoMock;
    private readonly Mock<ILogger<SystemStatisticsService>> _loggerMock;
    private readonly IOptions<SystemStatisticsSettings> _options;

    public SystemStatisticsServiceTests()
    {
        _repoMock = new Mock<ISystemStatisticsRepository>();
        _loggerMock = new Mock<ILogger<SystemStatisticsService>>();
        _options = Options.Create(new SystemStatisticsSettings { CacheDurationMinutes = 10 });
    }

    [Fact]
    public async Task GetSystemStatisticsAsync_WhenCacheIsEmpty_FetchesFromDbAndCaches()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var expectedStats = new SystemStatisticsDto { TotalUsers = 5, TotalProducts = 10, TotalDiaryEntries = 15, ActiveSessions = 2 };
        
        _repoMock.Setup(r => r.GetStatisticsAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync(expectedStats);

        var sut = new SystemStatisticsService(_repoMock.Object, cache, _options, _loggerMock.Object);

        // Act - Перший виклик (йде в БД)
        var result1 = await sut.GetSystemStatisticsAsync();
        
        // Act - Другий виклик (бере з кешу)
        var result2 = await sut.GetSystemStatisticsAsync();

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.Equal(5, result1.Value!.TotalUsers);
        
        Assert.True(result2.IsSuccess);
        Assert.Equal(10, result2.Value!.TotalProducts);

        // Перевіряємо, що репозиторій викликався лише 1 раз!
        _repoMock.Verify(r => r.GetStatisticsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSystemStatisticsAsync_WhenRepositoryThrows_ReturnsFailure()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        _repoMock.Setup(r => r.GetStatisticsAsync(It.IsAny<CancellationToken>()))
                 .ThrowsAsync(new Exception("Database down"));

        var sut = new SystemStatisticsService(_repoMock.Object, cache, _options, _loggerMock.Object);

        // Act
        var result = await sut.GetSystemStatisticsAsync();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("внутрішню помилку", result.Errors[0]);
    }
}