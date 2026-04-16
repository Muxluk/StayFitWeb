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
    private readonly Mock<IMealRepository> _mealRepositoryMock;
    private readonly Mock<ILogger<DiaryNoteService>> _loggerMock;
    private readonly IOptions<DiaryNoteSettings> _diaryNoteSettings;
    private readonly DiaryNoteService _diaryNoteService;

    public DiaryNoteServiceTests()
    {
        _mealRepositoryMock = new Mock<IMealRepository>();
        _loggerMock = new Mock<ILogger<DiaryNoteService>>();
        
        _diaryNoteSettings = Options.Create(new DiaryNoteSettings { MaxNoteLength = 500 });

        _diaryNoteService = new DiaryNoteService(
            _mealRepositoryMock.Object,
            _loggerMock.Object,
            _diaryNoteSettings);
    }

    [Fact]
    public async Task UpdateNoteAsync_SuccessfullyUpdatesNote_WhenValidInput()
    {
        // Arrange
        const int mealId = 1;
        const string userEmail = "test@user.com";
        const string noteText = "This is a test note";

        var meal = new MealEntry
        {
            Id = mealId,
            Name = "Breakfast",
            UserEmail = userEmail,
            Time = DateTime.UtcNow
        };

        _mealRepositoryMock
            .Setup(r => r.GetByIdAsync(mealId))
            .ReturnsAsync(meal);

        _mealRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<MealEntry>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _diaryNoteService.UpdateNoteAsync(mealId, userEmail, noteText);

        // Assert
        Assert.True(result);
        Assert.Equal(noteText, meal.Note);
        _mealRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<MealEntry>()), Times.Once);
    }

    [Fact]
    public async Task UpdateNoteAsync_ClearsNote_WhenNoteIsEmpty()
    {
        // Arrange
        const int mealId = 1;
        const string userEmail = "test@user.com";

        var meal = new MealEntry
        {
            Id = mealId,
            Name = "Breakfast",
            UserEmail = userEmail,
            Time = DateTime.UtcNow,
            Note = "Old note"
        };

        _mealRepositoryMock
            .Setup(r => r.GetByIdAsync(mealId))
            .ReturnsAsync(meal);

        _mealRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<MealEntry>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _diaryNoteService.UpdateNoteAsync(mealId, userEmail, "");

        // Assert
        Assert.True(result);
        Assert.Null(meal.Note);
        _mealRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<MealEntry>()), Times.Once);
    }

    [Fact]
    public async Task UpdateNoteAsync_ReturnsFalse_WhenNoteExceedsMaxLength()
    {
        // Arrange
        const int mealId = 1;
        const string userEmail = "test@user.com";
        var longNote = new string('x', 501); // Exceeds max of 500

        var meal = new MealEntry
        {
            Id = mealId,
            Name = "Breakfast",
            UserEmail = userEmail,
            Time = DateTime.UtcNow
        };

        _mealRepositoryMock
            .Setup(r => r.GetByIdAsync(mealId))
            .ReturnsAsync(meal);

        // Act
        var result = await _diaryNoteService.UpdateNoteAsync(mealId, userEmail, longNote);

        // Assert
        Assert.False(result);
        _mealRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<MealEntry>()), Times.Never);
    }

    [Fact]
    public async Task UpdateNoteAsync_ReturnsFalse_WhenMealNotFound()
    {
        // Arrange
        const int mealId = 999;
        const string userEmail = "test@user.com";

        _mealRepositoryMock
            .Setup(r => r.GetByIdAsync(mealId))
            .ReturnsAsync((MealEntry?)null);

        // Act
        var result = await _diaryNoteService.UpdateNoteAsync(mealId, userEmail, "Some note");

        // Assert
        Assert.False(result);
        _mealRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<MealEntry>()), Times.Never);
    }

    [Fact]
    public async Task UpdateNoteAsync_ReturnsFalse_WhenMealBelongsToAnotherUser()
    {
        // Arrange
        const int mealId = 1;
        const string userEmail = "test@user.com";
        const string anotherUserEmail = "another@user.com";

        var meal = new MealEntry
        {
            Id = mealId,
            Name = "Breakfast",
            UserEmail = anotherUserEmail,
            Time = DateTime.UtcNow
        };

        _mealRepositoryMock
            .Setup(r => r.GetByIdAsync(mealId))
            .ReturnsAsync(meal);

        // Act
        var result = await _diaryNoteService.UpdateNoteAsync(mealId, userEmail, "Some note");

        // Assert
        Assert.False(result);
        _mealRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<MealEntry>()), Times.Never);
    }

    [Fact]
    public async Task GetNoteAsync_ReturnsNote_WhenValid()
    {
        // Arrange
        const int mealId = 1;
        const string userEmail = "test@user.com";
        const string noteText = "Test note";

        var meal = new MealEntry
        {
            Id = mealId,
            Name = "Breakfast",
            UserEmail = userEmail,
            Time = DateTime.UtcNow,
            Note = noteText
        };

        _mealRepositoryMock
            .Setup(r => r.GetByIdAsync(mealId))
            .ReturnsAsync(meal);

        // Act
        var result = await _diaryNoteService.GetNoteAsync(mealId, userEmail);

        // Assert
        Assert.Equal(noteText, result);
    }

    [Fact]
    public async Task GetNoteAsync_ReturnsNull_WhenMealNotFound()
    {
        // Arrange
        const int mealId = 999;
        const string userEmail = "test@user.com";

        _mealRepositoryMock
            .Setup(r => r.GetByIdAsync(mealId))
            .ReturnsAsync((MealEntry?)null);

        // Act
        var result = await _diaryNoteService.GetNoteAsync(mealId, userEmail);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetNoteAsync_ReturnsNull_WhenMealBelongsToAnotherUser()
    {
        // Arrange
        const int mealId = 1;
        const string userEmail = "test@user.com";
        const string anotherUserEmail = "another@user.com";

        var meal = new MealEntry
        {
            Id = mealId,
            Name = "Breakfast",
            UserEmail = anotherUserEmail,
            Time = DateTime.UtcNow,
            Note = "Note"
        };

        _mealRepositoryMock
            .Setup(r => r.GetByIdAsync(mealId))
            .ReturnsAsync(meal);

        // Act
        var result = await _diaryNoteService.GetNoteAsync(mealId, userEmail);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void IsValidNote_ReturnsTrue_ForEmptyOrNullNotes(string? note)
    {
        // Act & Assert
        Assert.True(_diaryNoteService.IsValidNote(note));
    }

    [Fact]
    public void IsValidNote_ReturnsTrue_ForValidNote()
    {
        // Arrange
        var note = "This is a valid note";

        // Act & Assert
        Assert.True(_diaryNoteService.IsValidNote(note));
    }

    [Fact]
    public void IsValidNote_ReturnsFalse_WhenNoteExceedsMaxLength()
    {
        // Arrange
        var note = new string('x', 501); // Exceeds 500

        // Act & Assert
        Assert.False(_diaryNoteService.IsValidNote(note));
    }

    [Fact]
    public void IsValidNote_ReturnsTrue_WhenNoteEqualsMaxLength()
    {
        // Arrange
        var note = new string('x', 500); // Exactly 500

        // Act & Assert
        Assert.True(_diaryNoteService.IsValidNote(note));
    }

    [Fact]
    public void GetMaxNoteLength_ReturnsConfiguredLength()
    {
        // Act
        var maxLength = _diaryNoteService.GetMaxNoteLength();

        // Assert
        Assert.Equal(500, maxLength);
    }

    [Fact]
    public async Task UpdateNoteAsync_ReturnsFalse_WhenRepositoryThrowsException()
    {
        // Arrange
        const int mealId = 1;
        const string userEmail = "test@user.com";

        _mealRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _diaryNoteService.UpdateNoteAsync(mealId, userEmail, "Some note");

        // Assert
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
