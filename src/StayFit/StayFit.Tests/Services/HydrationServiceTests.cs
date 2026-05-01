using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StayFit.Application.Options;
using StayFit.Application.Services;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using Xunit;

namespace StayFit.Tests.Services;

public class HydrationServiceTests
{
    private readonly Mock<IHydrationRepository> _hydroRepoMock;
    private readonly Mock<IUserProfileRepository> _profileRepoMock;
    private readonly Mock<ILogger<HydrationService>> _loggerMock;
    private readonly HydrationService _service;

    public HydrationServiceTests()
    {
        _hydroRepoMock = new Mock<IHydrationRepository>();
        _profileRepoMock = new Mock<IUserProfileRepository>();
        _loggerMock = new Mock<ILogger<HydrationService>>();
        
        var options = Options.Create(new HydrationSettings 
        { 
            WeightMultiplierMl = 35, 
            QuickAddButtons = new List<int> { 200, 300, 500 }
        });
        
        _service = new HydrationService(_hydroRepoMock.Object, _profileRepoMock.Object, options, _loggerMock.Object);
    }

    [Fact]
    public async Task LogWaterAsync_ValidVolume_ReturnsSuccess()
    {
        // Act
        var result = await _service.LogWaterAsync(1, 250);

        // Assert
        Assert.True(result.IsSuccess);
        _hydroRepoMock.Verify(r => r.AddWaterLogAsync(It.Is<WaterLog>(l => l.VolumeMl == 250 && l.UserId == 1)), Times.Once);
    }

    [Fact]
    public async Task LogWaterAsync_InvalidVolume_ReturnsFailure()
    {
        // Act
        var result = await _service.LogWaterAsync(1, -50);

        // Assert
        Assert.False(result.IsSuccess);
        
        var actualError = result.Errors?.FirstOrDefault();
        Assert.Contains("більшим за 0", actualError); 
        
        _hydroRepoMock.Verify(r => r.AddWaterLogAsync(It.IsAny<WaterLog>()), Times.Never);
    }

    [Fact]
    public async Task CalculateAndSetAutoGoalAsync_UserHasWeight_SetsCalculatedGoal()
    {
        // Arrange
        _profileRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(new UserProfile { Weight = 80 });
        _hydroRepoMock.Setup(r => r.GetGoalByUserIdAsync(1)).ReturnsAsync((HydrationGoal?)null);

        // Act
        var result = await _service.CalculateAndSetAutoGoalAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2800, result.Value);
        _hydroRepoMock.Verify(r => r.AddGoalAsync(It.Is<HydrationGoal>(g => g.DailyGoalMl == 2800)), Times.Once);
    }
}