using Microsoft.Extensions.Logging;
using Moq;
using StayFit.Application.DTOs;
using StayFit.Application.Services;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using Xunit;
using System.Linq;

namespace StayFit.Tests.Services;

public class NutritionGoalServiceTests
{
    private readonly Mock<INutritionGoalRepository> _repoMock = new();
    private readonly Mock<ILogger<NutritionGoalService>> _loggerMock = new();
    private readonly NutritionGoalService _service;

    public NutritionGoalServiceTests()
    {
        _service = new NutritionGoalService(_repoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetGoalAsync_WhenGoalExists_ReturnsSuccess()
    {
        var goal = new NutritionGoal { UserId = "user1", CaloriesGoal = 2000 };
        _repoMock.Setup(r => r.GetByUserIdAsync("user1")).ReturnsAsync(goal);

        var result = await _service.GetGoalAsync("user1");

        Assert.True(result.IsSuccess);
        Assert.Equal(2000, result.Value?.CaloriesGoal);
    }

    [Fact]
    public async Task SetGoalAsync_WhenGoalNotExists_CreatesNewGoal()
    {
        _repoMock.Setup(r => r.GetByUserIdAsync("user1")).ReturnsAsync((NutritionGoal?)null);
        var dto = new SetNutritionGoalDto { CaloriesGoal = 2500 };

        var result = await _service.SetGoalAsync("user1", dto);

        Assert.True(result.IsSuccess);
        _repoMock.Verify(r => r.AddAsync(It.Is<NutritionGoal>(g => g.UserId == "user1")), Times.Once);
    }

    [Fact]
    public async Task SetGoalAsync_WhenGoalExists_UpdatesGoal()
    {
        var existing = new NutritionGoal { Id = 1, UserId = "user1" };
        _repoMock.Setup(r => r.GetByUserIdAsync("user1")).ReturnsAsync(existing);
        var dto = new SetNutritionGoalDto { CaloriesGoal = 1800 };

        await _service.SetGoalAsync("user1", dto);

        _repoMock.Verify(r => r.UpdateAsync(It.Is<NutritionGoal>(g => g.CaloriesGoal == 1800)), Times.Once);
    }

    [Fact]
    public async Task GetGoalAsync_WhenGoalNotExists_ReturnsFailure()
    {
        _repoMock.Setup(r => r.GetByUserIdAsync("any")).ReturnsAsync((NutritionGoal?)null);

        var result = await _service.GetGoalAsync("any");

        Assert.False(result.IsSuccess);
        Assert.Equal("Цілі не встановлено", result.Errors.FirstOrDefault());
    }

    [Fact]
    public async Task SetGoalAsync_WhenRepositoryThrows_PropagatesException()
    {
        _repoMock.Setup(r => r.GetByUserIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception("DB Error"));
        
        await Assert.ThrowsAsync<Exception>(() => _service.SetGoalAsync("u1", new SetNutritionGoalDto()));
    }
}