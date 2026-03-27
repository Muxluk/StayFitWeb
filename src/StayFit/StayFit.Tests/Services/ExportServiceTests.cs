using Microsoft.Extensions.Logging;
using Moq;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Domain.Results;
using StayFit.Infrastructure.Services;
using Xunit;

namespace StayFit.Tests.Services;

public class ExportServiceTests
{
    private readonly Mock<IFoodLogRepository> _foodLogRepoMock;
    private readonly Mock<IUserRepository>    _userRepoMock;
    private readonly Mock<ILogger<ExportService>> _loggerMock;
    private readonly ExportService _sut;

    private static readonly User SampleUser = new()
    {
        Id    = 42,
        Email = "test@stayfit.ua",
        Name  = "Тестовий Юзер",
    };

    private static readonly Food SampleFood = new()
    {
        Id              = 1,
        Name            = "Гречка варена",
        CaloriesPer100g = 110f,
        ProteinPer100g  = 4.2f,
        FatPer100g      = 0.6f,
        CarbsPer100g    = 21f,
        OwnerUserId     = 42,
    };

    private static List<FoodLog> BuildLogs(int count = 2) =>
        Enumerable.Range(0, count).Select(i => new FoodLog
        {
            Id          = i + 1,
            UserId      = SampleUser.Id,
            FoodId      = SampleFood.Id,
            Food        = SampleFood,
            AmountGrams = 150f,
            LoggedAt    = DateTime.UtcNow.AddDays(-i),
        }).ToList();

    public ExportServiceTests()
    {
        _foodLogRepoMock = new Mock<IFoodLogRepository>();
        _userRepoMock    = new Mock<IUserRepository>();
        _loggerMock      = new Mock<ILogger<ExportService>>();

        _sut = new ExportService(
            _foodLogRepoMock.Object,
            _userRepoMock.Object,
            _loggerMock.Object);
    }

    // ── Позитивні сценарії ───────────────────────────────────────────────────

    [Fact]
    public async Task ExportFoodLogsAsync_Csv_ReturnsSuccessWithCsvFile()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(SampleUser.Email))
                     .ReturnsAsync(SampleUser);
        _foodLogRepoMock
            .Setup(r => r.GetByUserIdAndDateRangeAsync(SampleUser.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(BuildLogs());

        var result = await _sut.ExportFoodLogsAsync(
            SampleUser.Email, DateTime.Today.AddDays(-7), DateTime.Today, ExportFormat.Csv);

        Assert.True(result.IsSuccess);
        var data = ((Result<ExportResult>.Success)result).Data;
        Assert.Equal("text/csv", data.ContentType);
        Assert.EndsWith(".csv", data.FileName);
        Assert.NotEmpty(data.FileBytes);
    }

    [Fact]
    public async Task ExportFoodLogsAsync_Pdf_ReturnsSuccessWithPdfFile()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(SampleUser.Email))
                     .ReturnsAsync(SampleUser);
        _foodLogRepoMock
            .Setup(r => r.GetByUserIdAndDateRangeAsync(SampleUser.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(BuildLogs());

        var result = await _sut.ExportFoodLogsAsync(
            SampleUser.Email, DateTime.Today.AddDays(-7), DateTime.Today, ExportFormat.Pdf);

        Assert.True(result.IsSuccess);
        var data = ((Result<ExportResult>.Success)result).Data;
        Assert.NotEmpty(data.FileBytes);
        Assert.NotEmpty(data.FileName);
    }

    [Fact]
    public async Task ExportFoodLogsAsync_Csv_FileContainsHeaderAndRows()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(SampleUser.Email))
                     .ReturnsAsync(SampleUser);
        _foodLogRepoMock
            .Setup(r => r.GetByUserIdAndDateRangeAsync(SampleUser.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(BuildLogs(3));

        var result = await _sut.ExportFoodLogsAsync(
            SampleUser.Email, DateTime.Today.AddDays(-7), DateTime.Today, ExportFormat.Csv);

        var success = (Result<ExportResult>.Success)result;
        var text = System.Text.Encoding.UTF8.GetString(success.Data.FileBytes).TrimStart('\uFEFF');

        Assert.Contains("Дата", text);
        Assert.Contains("Продукт", text);
        Assert.Contains("Гречка варена", text);

        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        // 1 заголовок + 3 рядки
        Assert.Equal(4, lines.Length);
    }

    [Fact]
    public async Task ExportFoodLogsAsync_FileName_ContainsDateRange()
    {
        var from = new DateTime(2025, 1, 1);
        var to   = new DateTime(2025, 1, 31);

        _userRepoMock.Setup(r => r.GetByEmailAsync(SampleUser.Email))
                     .ReturnsAsync(SampleUser);
        _foodLogRepoMock
            .Setup(r => r.GetByUserIdAndDateRangeAsync(SampleUser.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(BuildLogs());

        var result = await _sut.ExportFoodLogsAsync(SampleUser.Email, from, to, ExportFormat.Csv);

        var data = ((Result<ExportResult>.Success)result).Data;
        Assert.Contains("20250101", data.FileName);
        Assert.Contains("20250131", data.FileName);
    }

    // ── Негативні сценарії ───────────────────────────────────────────────────

    [Fact]
    public async Task ExportFoodLogsAsync_EmptyEmail_ReturnsFailure()
    {
        var result = await _sut.ExportFoodLogsAsync(
            string.Empty, DateTime.Today.AddDays(-1), DateTime.Today, ExportFormat.Csv);

        Assert.True(result.IsFailure);
        var failure = (Result<ExportResult>.Failure)result;
        Assert.False(string.IsNullOrWhiteSpace(failure.ErrorMessage));
    }

    [Fact]
    public async Task ExportFoodLogsAsync_FromAfterTo_ReturnsFailure()
    {
        var result = await _sut.ExportFoodLogsAsync(
            SampleUser.Email,
            from: DateTime.Today,
            to:   DateTime.Today.AddDays(-5),
            ExportFormat.Csv);

        Assert.True(result.IsFailure);
        var failure = (Result<ExportResult>.Failure)result;
        Assert.Contains("початку", failure.ErrorMessage);
    }

    [Fact]
    public async Task ExportFoodLogsAsync_UserNotFound_ReturnsFailure()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                     .ReturnsAsync((User?)null);

        var result = await _sut.ExportFoodLogsAsync(
            "unknown@example.com", DateTime.Today.AddDays(-1), DateTime.Today, ExportFormat.Csv);

        Assert.True(result.IsFailure);
        var failure = (Result<ExportResult>.Failure)result;
        Assert.Contains("не знайдено", failure.ErrorMessage);
    }

    [Fact]
    public async Task ExportFoodLogsAsync_NoLogsInRange_ReturnsFailure()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(SampleUser.Email))
                     .ReturnsAsync(SampleUser);
        _foodLogRepoMock
            .Setup(r => r.GetByUserIdAndDateRangeAsync(SampleUser.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<FoodLog>());

        var result = await _sut.ExportFoodLogsAsync(
            SampleUser.Email, DateTime.Today.AddDays(-7), DateTime.Today, ExportFormat.Csv);

        Assert.True(result.IsFailure);
        var failure = (Result<ExportResult>.Failure)result;
        Assert.Contains("не знайдено", failure.ErrorMessage);
    }

    [Fact]
    public async Task ExportFoodLogsAsync_NullWhitespaceEmail_ReturnsFailure()
    {
        var result = await _sut.ExportFoodLogsAsync(
            "   ", DateTime.Today.AddDays(-1), DateTime.Today, ExportFormat.Pdf);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task ExportFoodLogsAsync_SameDayRange_ReturnsSuccess()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(SampleUser.Email))
                     .ReturnsAsync(SampleUser);
        _foodLogRepoMock
            .Setup(r => r.GetByUserIdAndDateRangeAsync(SampleUser.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(BuildLogs(1));

        var result = await _sut.ExportFoodLogsAsync(
            SampleUser.Email, DateTime.Today, DateTime.Today, ExportFormat.Csv);

        Assert.True(result.IsSuccess);
    }
}
