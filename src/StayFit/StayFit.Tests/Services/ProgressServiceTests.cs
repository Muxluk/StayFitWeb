using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using StayFit.Application.DTOs;
using StayFit.Application.Services;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Domain.Results;
using Xunit;

namespace StayFit.Tests.Services;

public class ProgressServiceTests
{
    private readonly Mock<IFoodLogRepository> _mockFoodLogRepository;
    private readonly Mock<INutritionGoalRepository> _mockGoalRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ILogger<ProgressService>> _mockLogger;
    private readonly ProgressService _service;

    // Спільний DomainUser для всіх тестів
    private readonly User _domainUser = new User { Id = 7, Email = "test@test.com", Name = "Test" };

    public ProgressServiceTests()
    {
        _mockFoodLogRepository = new Mock<IFoodLogRepository>();
        _mockGoalRepository = new Mock<INutritionGoalRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<ProgressService>>();
        _service = new ProgressService(
            _mockFoodLogRepository.Object,
            _mockGoalRepository.Object,
            _mockUserRepository.Object,
            _mockLogger.Object);
    }

    // ─── INVALID DATES ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetProgressAnalysisAsync_InvalidDates_ReturnsFailure()
    {
        // Arrange: дата початку > дата кінця — перевірка відбувається до запитів до БД
        var result = await _service.GetProgressAnalysisAsync(
            1, "test@test.com",
            new DateTime(2023, 10, 10),
            new DateTime(2023, 10, 5));

        Assert.True(result.IsFailure);
        var failure = Assert.IsType<Result<ProgressAnalysisDto>.Failure>(result);
        Assert.Equal("INVALID_DATES", failure.ErrorCode);
    }

    // ─── MISSING GOAL ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProgressAnalysisAsync_MissingGoal_ReturnsFailure()
    {
        // Arrange: DomainUser знаходиться, але ціль відсутня
        _mockUserRepository
            .Setup(r => r.GetByEmailAsync("test@test.com"))
            .ReturnsAsync(_domainUser);

        _mockGoalRepository
            .Setup(r => r.GetByUserIdAsync("1"))   // identityUserId = 1
            .ReturnsAsync((NutritionGoal?)null);

        var result = await _service.GetProgressAnalysisAsync(
            1, "test@test.com",
            new DateTime(2023, 10, 1),
            new DateTime(2023, 10, 7));

        Assert.True(result.IsFailure);
        var failure = Assert.IsType<Result<ProgressAnalysisDto>.Failure>(result);
        Assert.Equal("NO_GOAL", failure.ErrorCode);
    }

    // ─── USER NOT FOUND ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetProgressAnalysisAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange: DomainUser не знайдено за email
        _mockUserRepository
            .Setup(r => r.GetByEmailAsync("test@test.com"))
            .ReturnsAsync((User?)null);

        var result = await _service.GetProgressAnalysisAsync(
            1, "test@test.com",
            new DateTime(2023, 10, 1),
            new DateTime(2023, 10, 7));

        Assert.True(result.IsFailure);
        var failure = Assert.IsType<Result<ProgressAnalysisDto>.Failure>(result);
        Assert.Equal("USER_NOT_FOUND", failure.ErrorCode);
    }

    // ─── VALID DATA ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProgressAnalysisAsync_ValidData_ReturnsSuccessWithAggregations()
    {
        var startDate = new DateTime(2023, 10, 1);
        var endDate = new DateTime(2023, 10, 2);

        var goal = new NutritionGoal
        {
            UserId = "1", CaloriesGoal = 2000, ProteinGoal = 150, FatGoal = 60, CarbsGoal = 200
        };

        // LoggedAt в UTC: 2023-10-01 08:00 UTC = 2023-10-01 11:00 за UTC+3
        var logs = new List<FoodLog>
        {
            new FoodLog
            {
                UserId = 7, AmountGrams = 100,
                LoggedAt = new DateTime(2023, 10, 1, 8, 0, 0, DateTimeKind.Utc),
                Food = new Food { CaloriesPer100g = 1000, ProteinPer100g = 50, FatPer100g = 20, CarbsPer100g = 50 }
            },
            new FoodLog
            {
                UserId = 7, AmountGrams = 100,
                LoggedAt = new DateTime(2023, 10, 1, 12, 0, 0, DateTimeKind.Utc),
                Food = new Food { CaloriesPer100g = 1000, ProteinPer100g = 50, FatPer100g = 20, CarbsPer100g = 50 }
            },
            new FoodLog
            {
                UserId = 7, AmountGrams = 100,
                LoggedAt = new DateTime(2023, 10, 2, 8, 0, 0, DateTimeKind.Utc),
                Food = new Food { CaloriesPer100g = 3000, ProteinPer100g = 100, FatPer100g = 50, CarbsPer100g = 150 }
            },
        };

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync("test@test.com"))
            .ReturnsAsync(_domainUser);

        _mockGoalRepository
            .Setup(r => r.GetByUserIdAsync("1"))
            .ReturnsAsync(goal);

        // FoodLogs запитуються за domainUser.Id = 7
        _mockFoodLogRepository
            .Setup(r => r.GetByUserIdAndDateRangeAsync(7, startDate, endDate))
            .ReturnsAsync(logs);

        var result = await _service.GetProgressAnalysisAsync(1, "test@test.com", startDate, endDate);

        Assert.True(result.IsSuccess);
        var success = Assert.IsType<Result<ProgressAnalysisDto>.Success>(result);

        Assert.Equal(2, success.Data.TotalDays);
        Assert.Equal(2, success.Data.DailyProgress.Count);

        // День 2 (02.10): 3000 ккал — далеко від цілі 2000
        var day2 = success.Data.DailyProgress.First(d => d.Date.Day == 2);
        Assert.Equal(3000, day2.TotalCalories);
        Assert.False(day2.CaloriesGoalMet);
    }
}
