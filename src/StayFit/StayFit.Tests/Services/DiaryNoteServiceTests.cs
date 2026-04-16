using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StayFit.Application.Configuration;
using StayFit.Application.Services;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using Xunit;

namespace StayFit.Tests.Services;

public class DiaryNoteServiceTests
{
    private readonly Mock<IFoodLogRepository> _foodLogRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<DiaryNoteService>> _loggerMock;
    private readonly IOptions<DiaryNoteSettings> _diaryNoteSettings;
    private readonly DiaryNoteService _diaryNoteService;

    public DiaryNoteServiceTests()
    {
        _foodLogRepositoryMock = new Mock<IFoodLogRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<DiaryNoteService>>();
        _diaryNoteSettings = Options.Create(new DiaryNoteSettings { MaxNoteLength = 500 });

        _diaryNoteService = new DiaryNoteService(
            _foodLogRepositoryMock.Object,
            _userRepositoryMock.Object,
            _loggerMock.Object,
            _diaryNoteSettings);
    }

    [Fact]
    public async Task UpdateNoteAsync_SuccessfullyUpdatesNote_WhenValidInput()
    {
        // Arrange
        const int logId = 1;
        const string userEmail = "test@user.com";
        const string noteText = "This is a test note";

        var foodLog = new FoodLog
        {
            Id = logId,
            UserId = 1,
            FoodId = 10,
            AmountGrams = 100,
            LoggedAt = DateTime.UtcNow,
            UserEmail = userEmail
        };

        _foodLogRepositoryMock
            .Setup(r => r.GetByIdAsync(logId))
            .ReturnsAsync(foodLog);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(userEmail))
            .ReturnsAsync(new Domain.Entities.User { Id = 1, Email = userEmail });

        _foodLogRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<FoodLog>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _diaryNoteService.UpdateNoteAsync(logId, userEmail, noteText);

        // Assert
        Assert.True(result);
        Assert.Equal(noteText, foodLog.Note);
        _foodLogRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<FoodLog>()), Times.Once);
    }

    [Fact]
    public async Task UpdateNoteAsync_ClearsNote_WhenNoteIsEmpty()
    {
        // Arrange
        const int logId = 1;
        const string userEmail = "test@user.com";

        var foodLog = new FoodLog
        {
            Id = logId,
            UserId = 1,
            FoodId = 10,
            AmountGrams = 100,
            LoggedAt = DateTime.UtcNow,
            UserEmail = userEmail,
            Note = "Old note"
        };

        _foodLogRepositoryMock
            .Setup(r => r.GetByIdAsync(logId))
            .ReturnsAsync(foodLog);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(userEmail))
            .ReturnsAsync(new Domain.Entities.User { Id = 1, Email = userEmail });

        _foodLogRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<FoodLog>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _diaryNoteService.UpdateNoteAsync(logId, userEmail, "");

        // Assert
        Assert.True(result);
        Assert.Null(foodLog.Note);
        _foodLogRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<FoodLog>()), Times.Once);
    }

    [Fact]
    public async Task UpdateNoteAsync_ReturnsFalse_WhenNoteExceedsMaxLength()
    {
        // Arrange
        const int logId = 1;
        const string userEmail = "test@user.com";
        var longNote = new string('x', 501);

        var foodLog = new FoodLog
        {
            Id = logId,
            UserId = 1,
            FoodId = 10,
            AmountGrams = 100,
            LoggedAt = DateTime.UtcNow,
            UserEmail = userEmail
        };

        _foodLogRepositoryMock
            .Setup(r => r.GetByIdAsync(logId))
            .ReturnsAsync(foodLog);

        // Act
        var result = await _diaryNoteService.UpdateNoteAsync(logId, userEmail, longNote);

        // Assert
        Assert.False(result);
        _foodLogRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<FoodLog>()), Times.Never);
    }

    [Fact]
    public async Task UpdateNoteAsync_ReturnsFalse_WhenFoodLogNotFound()
    {
        // Arrange
        const int logId = 999;
        const string userEmail = "test@user.com";

        _foodLogRepositoryMock
            .Setup(r => r.GetByIdAsync(logId))
            .ReturnsAsync((FoodLog?)null);

        // Act
        var result = await _diaryNoteService.UpdateNoteAsync(logId, userEmail, "Some note");

        // Assert
        Assert.False(result);
        _foodLogRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<FoodLog>()), Times.Never);
    }

    [Fact]
    public async Task UpdateNoteAsync_ReturnsFalse_WhenFoodLogBelongsToAnotherUser()
    {
        // Arrange
        const int logId = 1;
        const string userEmail = "test@user.com";
        const string anotherUserEmail = "another@user.com";

        var foodLog = new FoodLog
        {
            Id = logId,
            UserId = 2,
            FoodId = 10,
            AmountGrams = 100,
            LoggedAt = DateTime.UtcNow,
            UserEmail = anotherUserEmail
        };

        _foodLogRepositoryMock
            .Setup(r => r.GetByIdAsync(logId))
            .ReturnsAsync(foodLog);

        // Act
        var result = await _diaryNoteService.UpdateNoteAsync(logId, userEmail, "Some note");

        // Assert
        Assert.False(result);
        _foodLogRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<FoodLog>()), Times.Never);
    }

    [Fact]
    public async Task GetNoteAsync_ReturnsNote_WhenValid()
    {
        // Arrange
        const int logId = 1;
        const string userEmail = "test@user.com";
        const string noteText = "Test note";

        var foodLog = new FoodLog
        {
            Id = logId,
            UserId = 1,
            FoodId = 10,
            AmountGrams = 100,
            LoggedAt = DateTime.UtcNow,
            UserEmail = userEmail,
            Note = noteText
        };

        _foodLogRepositoryMock
            .Setup(r => r.GetByIdAsync(logId))
            .ReturnsAsync(foodLog);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(userEmail))
            .ReturnsAsync(new Domain.Entities.User { Id = 1, Email = userEmail });

        // Act
        var result = await _diaryNoteService.GetNoteAsync(logId, userEmail);

        // Assert
        Assert.Equal(noteText, result);
    }

    [Fact]
    public async Task GetNoteAsync_ReturnsNull_WhenFoodLogNotFound()
    {
        // Arrange
        const int logId = 999;
        const string userEmail = "test@user.com";

        _foodLogRepositoryMock
            .Setup(r => r.GetByIdAsync(logId))
            .ReturnsAsync((FoodLog?)null);

        // Act
        var result = await _diaryNoteService.GetNoteAsync(logId, userEmail);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetNoteAsync_ReturnsNull_WhenFoodLogBelongsToAnotherUser()
    {
        // Arrange
        const int logId = 1;
        const string userEmail = "test@user.com";

        var foodLog = new FoodLog
        {
            Id = logId,
            UserId = 2,
            FoodId = 10,
            AmountGrams = 100,
            LoggedAt = DateTime.UtcNow,
            UserEmail = "another@user.com",
            Note = "Note"
        };

        _foodLogRepositoryMock
            .Setup(r => r.GetByIdAsync(logId))
            .ReturnsAsync(foodLog);

        // Act
        var result = await _diaryNoteService.GetNoteAsync(logId, userEmail);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void IsValidNote_ReturnsTrue_ForEmptyOrNullNotes(string? note)
    {
        Assert.True(_diaryNoteService.IsValidNote(note));
    }

    [Fact]
    public void IsValidNote_ReturnsTrue_ForValidNote()
    {
        var note = "This is a valid note";
        Assert.True(_diaryNoteService.IsValidNote(note));
    }

    [Fact]
    public void IsValidNote_ReturnsFalse_WhenNoteExceedsMaxLength()
    {
        var note = new string('x', 501);
        Assert.False(_diaryNoteService.IsValidNote(note));
    }

    [Fact]
    public void IsValidNote_ReturnsTrue_WhenNoteEqualsMaxLength()
    {
        var note = new string('x', 500);
        Assert.True(_diaryNoteService.IsValidNote(note));
    }

    [Fact]
    public void GetMaxNoteLength_ReturnsConfiguredLength()
    {
        var maxLength = _diaryNoteService.GetMaxNoteLength();
        Assert.Equal(500, maxLength);
    }

    [Fact]
    public async Task UpdateNoteAsync_ReturnsFalse_WhenRepositoryThrowsException()
    {
        const int logId = 1;
        const string userEmail = "test@user.com";

        _foodLogRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _diaryNoteService.UpdateNoteAsync(logId, userEmail, "Some note");

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
