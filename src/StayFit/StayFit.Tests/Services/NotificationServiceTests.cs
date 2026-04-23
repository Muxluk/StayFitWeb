using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StayFit.Application.Interfaces;
using StayFit.Application.Options;
using StayFit.Application.Services;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Tests.Services;

public sealed class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly Mock<IOptions<NotificationSettings>> _notificationSettingsMock;
    private readonly Mock<ILogger<NotificationService>> _loggerMock;

    public NotificationServiceTests()
    {
        _notificationRepositoryMock = new Mock<INotificationRepository>();
        _notificationSettingsMock = new Mock<IOptions<NotificationSettings>>();
        _loggerMock = new Mock<ILogger<NotificationService>>();

        _notificationSettingsMock
            .Setup(o => o.Value)
            .Returns(new NotificationSettings { CalorieThresholdPercent = 100 });
    }

    private NotificationService CreateSut()
    {
        return new NotificationService(
            _notificationRepositoryMock.Object,
            _notificationSettingsMock.Object,
            _loggerMock.Object);
    }

    #region GetUnreadNotificationsAsync Tests

    [Fact]
    public async Task GetUnreadNotificationsAsync_WhenNotificationsExist_ReturnsNotificationDtos()
    {
        // Arrange
        var userId = 1;
        var notifications = new List<Notification>
        {
            new()
            {
                Id = 1,
                UserId = userId,
                Title = "Calorie Alert",
                Message = "You exceeded your daily calorie goal",
                Type = "CalorieThreshold",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                UserId = userId,
                Title = "Another Alert",
                Message = "Test message",
                Type = "Test",
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            }
        };

        _notificationRepositoryMock
            .Setup(r => r.GetUnreadNotificationsAsync(userId))
            .ReturnsAsync(notifications);

        var sut = CreateSut();

        // Act
        var result = await sut.GetUnreadNotificationsAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var notificationsList = result.Value.ToList();
        Assert.Equal(2, notificationsList.Count);
        Assert.Equal("Calorie Alert", notificationsList[0].Title);
        _notificationRepositoryMock.Verify(r => r.GetUnreadNotificationsAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUnreadNotificationsAsync_WhenNoNotifications_ReturnsEmptyList()
    {
        // Arrange
        var userId = 1;
        _notificationRepositoryMock
            .Setup(r => r.GetUnreadNotificationsAsync(userId))
            .ReturnsAsync(new List<Notification>());

        var sut = CreateSut();

        // Act
        var result = await sut.GetUnreadNotificationsAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetUnreadNotificationsAsync_WhenRepositoryThrows_ReturnsFailure()
    {
        // Arrange
        var userId = 1;
        _notificationRepositoryMock
            .Setup(r => r.GetUnreadNotificationsAsync(userId))
            .ThrowsAsync(new Exception("Database error"));

        var sut = CreateSut();

        // Act
        var result = await sut.GetUnreadNotificationsAsync(userId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Помилка при отриманні сповіщень", result.Errors);
    }

    #endregion

    #region GetUnreadCountAsync Tests

    [Fact]
    public async Task GetUnreadCountAsync_WhenNotificationsExist_ReturnsCorrectCount()
    {
        // Arrange
        var userId = 1;
        var unreadCount = 5;

        _notificationRepositoryMock
            .Setup(r => r.GetUnreadCountAsync(userId))
            .ReturnsAsync(unreadCount);

        var sut = CreateSut();

        // Act
        var result = await sut.GetUnreadCountAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(unreadCount, result.Value);
        _notificationRepositoryMock.Verify(r => r.GetUnreadCountAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUnreadCountAsync_WhenNoNotifications_ReturnsZero()
    {
        // Arrange
        var userId = 1;
        _notificationRepositoryMock
            .Setup(r => r.GetUnreadCountAsync(userId))
            .ReturnsAsync(0);

        var sut = CreateSut();

        // Act
        var result = await sut.GetUnreadCountAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value);
    }

    [Fact]
    public async Task GetUnreadCountAsync_WhenRepositoryThrows_ReturnsFailure()
    {
        // Arrange
        var userId = 1;
        _notificationRepositoryMock
            .Setup(r => r.GetUnreadCountAsync(userId))
            .ThrowsAsync(new Exception("Database error"));

        var sut = CreateSut();

        // Act
        var result = await sut.GetUnreadCountAsync(userId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Помилка при отриманні кількості сповіщень", result.Errors);
    }

    #endregion

    #region MarkAsReadAsync Tests

    [Fact]
    public async Task MarkAsReadAsync_WhenNotificationExists_MarkesAsRead()
    {
        // Arrange
        var userId = 1;
        var notificationId = 1;
        var notification = new Notification
        {
            Id = notificationId,
            UserId = userId,
            Title = "Test",
            Message = "Test message",
            Type = "Test",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _notificationRepositoryMock
            .Setup(r => r.GetByIdAsync(notificationId))
            .ReturnsAsync(notification);

        _notificationRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        // Act
        var result = await sut.MarkAsReadAsync(notificationId, userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(notification.IsRead);
        Assert.NotNull(notification.ReadAt);
        _notificationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Notification>()), Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_WhenNotificationNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = 1;
        var notificationId = 999;

        _notificationRepositoryMock
            .Setup(r => r.GetByIdAsync(notificationId))
            .ReturnsAsync((Notification?)null);

        var sut = CreateSut();

        // Act
        var result = await sut.MarkAsReadAsync(notificationId, userId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Сповіщення не знайдено", result.Errors);
        _notificationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task MarkAsReadAsync_WhenUserIdMismatch_ReturnsFailure()
    {
        // Arrange
        var userId = 1;
        var differentUserId = 2;
        var notificationId = 1;
        var notification = new Notification
        {
            Id = notificationId,
            UserId = differentUserId,
            Title = "Test",
            Message = "Test message",
            Type = "Test",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _notificationRepositoryMock
            .Setup(r => r.GetByIdAsync(notificationId))
            .ReturnsAsync(notification);

        var sut = CreateSut();

        // Act
        var result = await sut.MarkAsReadAsync(notificationId, userId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Вам не дозволено змінювати це сповіщення", result.Errors);
        _notificationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task MarkAsReadAsync_WhenNotificationAlreadyRead_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var notificationId = 1;
        var notification = new Notification
        {
            Id = notificationId,
            UserId = userId,
            Title = "Test",
            Message = "Test message",
            Type = "Test",
            IsRead = true,
            ReadAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _notificationRepositoryMock
            .Setup(r => r.GetByIdAsync(notificationId))
            .ReturnsAsync(notification);

        var sut = CreateSut();

        // Act
        var result = await sut.MarkAsReadAsync(notificationId, userId);

        // Assert
        Assert.True(result.IsSuccess);
        _notificationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task MarkAsReadAsync_WhenRepositoryThrows_ReturnsFailure()
    {
        // Arrange
        var userId = 1;
        var notificationId = 1;
        var notification = new Notification
        {
            Id = notificationId,
            UserId = userId,
            Title = "Test",
            Message = "Test message",
            Type = "Test",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _notificationRepositoryMock
            .Setup(r => r.GetByIdAsync(notificationId))
            .ReturnsAsync(notification);

        _notificationRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Notification>()))
            .ThrowsAsync(new Exception("Database error"));

        var sut = CreateSut();

        // Act
        var result = await sut.MarkAsReadAsync(notificationId, userId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Помилка при позначенні сповіщення як прочитаного", result.Errors);
    }

    #endregion

    #region MarkAllAsReadAsync Tests

    [Fact]
    public async Task MarkAllAsReadAsync_WhenUnreadNotificationsExist_MarkesAllAsRead()
    {
        // Arrange
        var userId = 1;
        var unreadNotifications = new List<Notification>
        {
            new()
            {
                Id = 1,
                UserId = userId,
                Title = "Test 1",
                Message = "Test message 1",
                Type = "Test",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                UserId = userId,
                Title = "Test 2",
                Message = "Test message 2",
                Type = "Test",
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            }
        };

        _notificationRepositoryMock
            .Setup(r => r.GetUnreadNotificationsAsync(userId))
            .ReturnsAsync(unreadNotifications);

        _notificationRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        // Act
        var result = await sut.MarkAllAsReadAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.All(unreadNotifications, n => Assert.True(n.IsRead));
        Assert.All(unreadNotifications, n => Assert.NotNull(n.ReadAt));
        _notificationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Notification>()), Times.Exactly(2));
    }

    [Fact]
    public async Task MarkAllAsReadAsync_WhenNoUnreadNotifications_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        _notificationRepositoryMock
            .Setup(r => r.GetUnreadNotificationsAsync(userId))
            .ReturnsAsync(new List<Notification>());

        var sut = CreateSut();

        // Act
        var result = await sut.MarkAllAsReadAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        _notificationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_WhenRepositoryThrows_ReturnsFailure()
    {
        // Arrange
        var userId = 1;
        _notificationRepositoryMock
            .Setup(r => r.GetUnreadNotificationsAsync(userId))
            .ThrowsAsync(new Exception("Database error"));

        var sut = CreateSut();

        // Act
        var result = await sut.MarkAllAsReadAsync(userId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Помилка при позначенні всіх сповіщень як прочитаних", result.Errors);
    }

    #endregion

    #region ClearAllNotificationsAsync Tests

    [Fact]
    public async Task ClearAllNotificationsAsync_WhenCalled_DeletesAllNotifications()
    {
        // Arrange
        var userId = 1;
        _notificationRepositoryMock
            .Setup(r => r.DeleteAllByUserIdAsync(userId))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        // Act
        var result = await sut.ClearAllNotificationsAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        _notificationRepositoryMock.Verify(r => r.DeleteAllByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task ClearAllNotificationsAsync_WhenRepositoryThrows_ReturnsFailure()
    {
        // Arrange
        var userId = 1;
        _notificationRepositoryMock
            .Setup(r => r.DeleteAllByUserIdAsync(userId))
            .ThrowsAsync(new Exception("Database error"));

        var sut = CreateSut();

        // Act
        var result = await sut.ClearAllNotificationsAsync(userId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Помилка при очищенні сповіщень", result.Errors);
    }

    #endregion

    #region CreateCalorieThresholdNotificationAsync Tests

    [Fact]
    public async Task CreateCalorieThresholdNotificationAsync_WhenCalled_CreatesNotification()
    {
        // Arrange
        var userId = 1;
        var calorieOverage = 150.5m;
        
        _notificationRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        // Act
        var result = await sut.CreateCalorieThresholdNotificationAsync(userId, calorieOverage);

        // Assert
        Assert.True(result.IsSuccess);
        _notificationRepositoryMock.Verify(
            r => r.AddAsync(It.Is<Notification>(n =>
                n.UserId == userId &&
                n.Type == "CalorieThreshold" &&
                n.IsRead == false &&
                n.Message.Contains(calorieOverage.ToString("F0"))
            )),
            Times.Once);
    }

    [Fact]
    public async Task CreateCalorieThresholdNotificationAsync_WhenRepositoryThrows_ReturnsFailure()
    {
        // Arrange
        var userId = 1;
        var calorieOverage = 150.5m;
        
        _notificationRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .ThrowsAsync(new Exception("Database error"));

        var sut = CreateSut();

        // Act
        var result = await sut.CreateCalorieThresholdNotificationAsync(userId, calorieOverage);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Помилка при створенні сповіщення", result.Errors);
    }

    [Fact]
    public async Task CreateCalorieThresholdNotificationAsync_WithDifferentThresholds_CreatesCorrectMessage()
    {
        // Arrange
        var userId = 1;
        var calorieOverage = 250.75m;
        var threshold = 120m;

        _notificationSettingsMock
            .Setup(o => o.Value)
            .Returns(new NotificationSettings { CalorieThresholdPercent = threshold });

        _notificationRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        // Act
        var result = await sut.CreateCalorieThresholdNotificationAsync(userId, calorieOverage);

        // Assert
        Assert.True(result.IsSuccess);
        _notificationRepositoryMock.Verify(
            r => r.AddAsync(It.Is<Notification>(n =>
                n.Message.Contains("251") && 
                n.Message.Contains("120%")
            )),
            Times.Once);
    }

    #endregion
}
