using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StayFit.Application.Configuration;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Application.Services;
using StayFit.Domain.Entities;
using Xunit;

namespace StayFit.Tests.Services;

public class SecurityLogServiceTests
{
    private readonly Mock<ISecurityLogRepository> _repositoryMock;
    private readonly Mock<ILogger<SecurityLogService>> _loggerMock;
    private readonly Mock<IOptions<SecurityLogSettings>> _settingsMock;
    private readonly SecurityLogService _service;
    private readonly SecurityLogSettings _settings;

    public SecurityLogServiceTests()
    {
        _repositoryMock = new Mock<ISecurityLogRepository>();
        _loggerMock = new Mock<ILogger<SecurityLogService>>();
        
        _settings = new SecurityLogSettings 
        { 
            DefaultPageSize = 10, 
            MaxPageSize = 50,
            RetentionDays = 90
        };
        
        _settingsMock = new Mock<IOptions<SecurityLogSettings>>();
        _settingsMock.Setup(s => s.Value).Returns(_settings);

        _service = new SecurityLogService(_repositoryMock.Object, _loggerMock.Object, _settingsMock.Object);
    }

    #region LogLoginAsync Tests

    [Fact]
    public async Task LogLoginAsync_SuccessfulLogin_AddsLogEntry()
    {
        // Arrange
        int userId = 1;
        string ipAddress = "192.168.1.1";
        string userAgent = "Mozilla/5.0";
        
        SecurityLogEntry? capturedEntry = null;
        _repositoryMock
            .Setup(r => r.AddLogEntryAsync(It.IsAny<SecurityLogEntry>()))
            .Callback<SecurityLogEntry>(e => capturedEntry = e)
            .ReturnsAsync((SecurityLogEntry e) => e);

        // Act
        await _service.LogLoginAsync(userId, ipAddress, userAgent, true);

        // Assert
        Assert.NotNull(capturedEntry);
        Assert.Equal(userId, capturedEntry.UserId);
        Assert.Equal("Login", capturedEntry.EventType);
        Assert.Equal("Success", capturedEntry.Status);
        Assert.Equal(ipAddress, capturedEntry.IpAddress);
        Assert.Equal(userAgent, capturedEntry.UserAgent);
        _repositoryMock.Verify(r => r.AddLogEntryAsync(It.IsAny<SecurityLogEntry>()), Times.Once);
    }

    [Fact]
    public async Task LogLoginAsync_FailedLogin_AddsFailureEntry()
    {
        // Arrange
        int userId = 1;
        string ipAddress = "192.168.1.1";
        string failureReason = "Invalid credentials";
        
        SecurityLogEntry? capturedEntry = null;
        _repositoryMock
            .Setup(r => r.AddLogEntryAsync(It.IsAny<SecurityLogEntry>()))
            .Callback<SecurityLogEntry>(e => capturedEntry = e)
            .ReturnsAsync((SecurityLogEntry e) => e);

        // Act
        await _service.LogLoginAsync(userId, ipAddress, null, false, failureReason);

        // Assert
        Assert.NotNull(capturedEntry);
        Assert.Equal("Failure", capturedEntry.Status);
        Assert.Equal(failureReason, capturedEntry.AdditionalInfo);
        _repositoryMock.Verify(r => r.AddLogEntryAsync(It.IsAny<SecurityLogEntry>()), Times.Once);
    }

    #endregion

    #region LogPasswordChangeAsync Tests

    [Fact]
    public async Task LogPasswordChangeAsync_SuccessfulChange_AddsLogEntry()
    {
        // Arrange
        int userId = 1;
        string ipAddress = "192.168.1.1";
        
        SecurityLogEntry? capturedEntry = null;
        _repositoryMock
            .Setup(r => r.AddLogEntryAsync(It.IsAny<SecurityLogEntry>()))
            .Callback<SecurityLogEntry>(e => capturedEntry = e)
            .ReturnsAsync((SecurityLogEntry e) => e);

        // Act
        await _service.LogPasswordChangeAsync(userId, ipAddress, null, true);

        // Assert
        Assert.NotNull(capturedEntry);
        Assert.Equal(userId, capturedEntry.UserId);
        Assert.Equal("PasswordChange", capturedEntry.EventType);
        Assert.Equal("Success", capturedEntry.Status);
        _repositoryMock.Verify(r => r.AddLogEntryAsync(It.IsAny<SecurityLogEntry>()), Times.Once);
    }

    [Fact]
    public async Task LogPasswordChangeAsync_FailedChange_AddsFailureEntry()
    {
        // Arrange
        int userId = 1;
        string failureReason = "Current password incorrect";
        
        SecurityLogEntry? capturedEntry = null;
        _repositoryMock
            .Setup(r => r.AddLogEntryAsync(It.IsAny<SecurityLogEntry>()))
            .Callback<SecurityLogEntry>(e => capturedEntry = e)
            .ReturnsAsync((SecurityLogEntry e) => e);

        // Act
        await _service.LogPasswordChangeAsync(userId, null, null, false, failureReason);

        // Assert
        Assert.NotNull(capturedEntry);
        Assert.Equal("Failure", capturedEntry.Status);
        Assert.Equal(failureReason, capturedEntry.AdditionalInfo);
    }

    #endregion

    #region LogLogoutAsync Tests

    [Fact]
    public async Task LogLogoutAsync_ValidInput_AddsLogEntry()
    {
        // Arrange
        int userId = 1;
        string ipAddress = "192.168.1.1";
        
        SecurityLogEntry? capturedEntry = null;
        _repositoryMock
            .Setup(r => r.AddLogEntryAsync(It.IsAny<SecurityLogEntry>()))
            .Callback<SecurityLogEntry>(e => capturedEntry = e)
            .ReturnsAsync((SecurityLogEntry e) => e);

        // Act
        await _service.LogLogoutAsync(userId, ipAddress);

        // Assert
        Assert.NotNull(capturedEntry);
        Assert.Equal(userId, capturedEntry.UserId);
        Assert.Equal("Logout", capturedEntry.EventType);
        Assert.Equal("Success", capturedEntry.Status);
        _repositoryMock.Verify(r => r.AddLogEntryAsync(It.IsAny<SecurityLogEntry>()), Times.Once);
    }

    #endregion

    #region LogSessionTerminatedAsync Tests

    [Fact]
    public async Task LogSessionTerminatedAsync_ValidInput_AddsLogEntry()
    {
        // Arrange
        int userId = 1;
        string ipAddress = "192.168.1.1";
        
        SecurityLogEntry? capturedEntry = null;
        _repositoryMock
            .Setup(r => r.AddLogEntryAsync(It.IsAny<SecurityLogEntry>()))
            .Callback<SecurityLogEntry>(e => capturedEntry = e)
            .ReturnsAsync((SecurityLogEntry e) => e);

        // Act
        await _service.LogSessionTerminatedAsync(userId, ipAddress);

        // Assert
        Assert.NotNull(capturedEntry);
        Assert.Equal(userId, capturedEntry.UserId);
        Assert.Equal("SessionTerminated", capturedEntry.EventType);
        _repositoryMock.Verify(r => r.AddLogEntryAsync(It.IsAny<SecurityLogEntry>()), Times.Once);
    }

    #endregion

    #region GetUserSecurityLogsAsync Tests

    [Fact]
    public async Task GetUserSecurityLogsAsync_ValidPage_ReturnsPagedResult()
    {
        // Arrange
        int userId = 1;
        int pageNumber = 1;
        
        var entries = new List<SecurityLogEntry>
        {
            new SecurityLogEntry 
            { 
                Id = 1, 
                UserId = userId, 
                EventType = "Login", 
                Description = "Успішний вхід",
                CreatedAt = DateTime.UtcNow,
                Status = "Success"
            },
            new SecurityLogEntry 
            { 
                Id = 2, 
                UserId = userId, 
                EventType = "PasswordChange", 
                Description = "Пароль змінено",
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                Status = "Success"
            }
        };

        _repositoryMock
            .Setup(r => r.GetUserLogsAsync(userId, pageNumber, _settings.DefaultPageSize, null))
            .ReturnsAsync((entries, 2));

        // Act
        var result = await _service.GetUserSecurityLogsAsync(userId, pageNumber);

        // Assert
        Assert.True(result.IsSuccess);
        var successResult = result as StayFit.Domain.Results.Result<PagedResult<SecurityLogDto>>.Success;
        Assert.NotNull(successResult);
        Assert.Equal(2, successResult.Data.Items.Count());
        Assert.Equal(2, successResult.Data.TotalCount);
        Assert.Equal(pageNumber, successResult.Data.PageNumber);
        _repositoryMock.Verify(r => r.GetUserLogsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task GetUserSecurityLogsAsync_InvalidPageNumber_UsesPageOne()
    {
        // Arrange
        int userId = 1;
        int invalidPageNumber = -1;
        
        var entries = new List<SecurityLogEntry>();
        _repositoryMock
            .Setup(r => r.GetUserLogsAsync(userId, 1, _settings.DefaultPageSize, null))
            .ReturnsAsync((entries, 0));

        // Act
        var result = await _service.GetUserSecurityLogsAsync(userId, invalidPageNumber);

        // Assert
        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(r => r.GetUserLogsAsync(userId, 1, _settings.DefaultPageSize, null), Times.Once);
    }

    [Fact]
    public async Task GetUserSecurityLogsAsync_RepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        int userId = 1;
        _repositoryMock
            .Setup(r => r.GetUserLogsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.GetUserSecurityLogsAsync(userId, 1);

        // Assert
        Assert.True(result.IsFailure);
        var failureResult = result as StayFit.Domain.Results.Result<PagedResult<SecurityLogDto>>.Failure;
        Assert.NotNull(failureResult);
        Assert.Equal("ERROR", failureResult.ErrorCode);
    }

    [Fact]
    public async Task GetUserSecurityLogsAsync_WithEventType_UsesFilterInRepository()
    {
        // Arrange
        const int userId = 1;
        const int pageNumber = 1;
        const string eventType = "PasswordChange";

        _repositoryMock
            .Setup(r => r.GetUserLogsAsync(userId, pageNumber, _settings.DefaultPageSize, eventType))
            .ReturnsAsync((new List<SecurityLogEntry>(), 0));

        // Act
        var result = await _service.GetUserSecurityLogsAsync(userId, pageNumber, eventType);

        // Assert
        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(r => r.GetUserLogsAsync(userId, pageNumber, _settings.DefaultPageSize, eventType), Times.Once);
    }

    #endregion

    #region GetRecentLogsAsync Tests

    [Fact]
    public async Task GetRecentLogsAsync_ValidInput_ReturnsRecentLogs()
    {
        // Arrange
        int userId = 1;
        int count = 5;
        
        var entries = new List<SecurityLogEntry>
        {
            new SecurityLogEntry { Id = 1, UserId = userId, EventType = "Login", CreatedAt = DateTime.UtcNow, Status = "Success" },
            new SecurityLogEntry { Id = 2, UserId = userId, EventType = "Logout", CreatedAt = DateTime.UtcNow.AddHours(-1), Status = "Success" }
        };

        _repositoryMock
            .Setup(r => r.GetRecentLogsAsync(userId, count))
            .ReturnsAsync(entries);

        // Act
        var result = await _service.GetRecentLogsAsync(userId, count);

        // Assert
        Assert.True(result.IsSuccess);
        var successResult = result as StayFit.Domain.Results.Result<System.Collections.Generic.IEnumerable<SecurityLogDto>>.Success;
        Assert.NotNull(successResult);
        Assert.Equal(2, successResult.Data.Count());
        _repositoryMock.Verify(r => r.GetRecentLogsAsync(userId, count), Times.Once);
    }

    [Fact]
    public async Task GetRecentLogsAsync_InvalidCount_UsesDefaultCount()
    {
        // Arrange
        int userId = 1;
        int invalidCount = -1;
        
        var entries = new List<SecurityLogEntry>();
        _repositoryMock
            .Setup(r => r.GetRecentLogsAsync(userId, 5))
            .ReturnsAsync(entries);

        // Act
        var result = await _service.GetRecentLogsAsync(userId, invalidCount);

        // Assert
        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(r => r.GetRecentLogsAsync(userId, 5), Times.Once);
    }

    #endregion

    #region CleanupOldLogsAsync Tests

    [Fact]
    public async Task CleanupOldLogsAsync_SuccessfulDeletion_ReturnsDeletedCount()
    {
        // Arrange
        int deletedCount = 50;
        _repositoryMock
            .Setup(r => r.DeleteOldLogsAsync(_settings.RetentionDays))
            .ReturnsAsync(deletedCount);

        // Act
        var result = await _service.CleanupOldLogsAsync();

        // Assert
        Assert.True(result.IsSuccess);
        var successResult = result as StayFit.Domain.Results.Result<int>.Success;
        Assert.NotNull(successResult);
        Assert.Equal(deletedCount, successResult.Data);
        _repositoryMock.Verify(r => r.DeleteOldLogsAsync(_settings.RetentionDays), Times.Once);
    }

    [Fact]
    public async Task CleanupOldLogsAsync_RepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.DeleteOldLogsAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.CleanupOldLogsAsync();

        // Assert
        Assert.True(result.IsFailure);
        var failureResult = result as StayFit.Domain.Results.Result<int>.Failure;
        Assert.NotNull(failureResult);
        Assert.Equal("ERROR", failureResult.ErrorCode);
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task LogLoginAsync_RepositoryThrowsException_DoesNotThrow()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.AddLogEntryAsync(It.IsAny<SecurityLogEntry>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert - should not throw
        await _service.LogLoginAsync(1, "192.168.1.1", "UserAgent", true);
        
        _repositoryMock.Verify(r => r.AddLogEntryAsync(It.IsAny<SecurityLogEntry>()), Times.Once);
    }

    [Fact]
    public async Task LogPasswordChangeAsync_RepositoryThrowsException_DoesNotThrow()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.AddLogEntryAsync(It.IsAny<SecurityLogEntry>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert - should not throw
        await _service.LogPasswordChangeAsync(1, "192.168.1.1", null, true);
        
        _repositoryMock.Verify(r => r.AddLogEntryAsync(It.IsAny<SecurityLogEntry>()), Times.Once);
    }

    #endregion
}
