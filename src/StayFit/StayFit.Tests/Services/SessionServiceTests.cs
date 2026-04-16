using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StayFit.Application.Options;
using StayFit.Application.Services;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Domain.Results;
using Xunit;

namespace StayFit.Tests.Services;

public class SessionServiceTests
{
    private readonly Mock<ISessionRepository> _repositoryMock;
    private readonly Mock<IOptions<SessionSettings>> _settingsMock;
    private readonly Mock<ILogger<SessionService>> _loggerMock;
    private readonly SessionService _service;

    public SessionServiceTests()
    {
        _repositoryMock = new Mock<ISessionRepository>();
        _settingsMock = new Mock<IOptions<SessionSettings>>();
        _settingsMock.Setup(s => s.Value).Returns(new SessionSettings
        {
            MaxActiveSessions = 5,
            SessionLifetimeHours = 24
        });
        _loggerMock = new Mock<ILogger<SessionService>>();
        _service = new SessionService(_repositoryMock.Object, _settingsMock.Object, _loggerMock.Object);
    }

    // ── GetActiveSessions ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetActiveSessions_ReturnsSuccess_WithSessions()
    {
        // Arrange
        var sessions = new List<UserSession>
        {
            new() { Id = 1, UserId = 10, IsActive = true, SessionToken = "token1", CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddHours(24) },
            new() { Id = 2, UserId = 10, IsActive = true, SessionToken = "token2", CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddHours(24) },
        };
        _repositoryMock.Setup(r => r.GetActiveByUserIdAsync(10)).ReturnsAsync(sessions);

        // Act
        var result = await _service.GetActiveSessionsAsync(10);

        // Assert
        Assert.True(result.IsSuccess);
        var success = (Result<IList<UserSession>>.Success)result;
        Assert.Equal(2, success.Data.Count);
    }

    [Fact]
    public async Task GetActiveSessions_ReturnsSuccess_WithEmptyList()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetActiveByUserIdAsync(99)).ReturnsAsync(new List<UserSession>());

        // Act
        var result = await _service.GetActiveSessionsAsync(99);

        // Assert
        Assert.True(result.IsSuccess);
        var success = (Result<IList<UserSession>>.Success)result;
        Assert.Empty(success.Data);
    }

    // ── TerminateSession ───────────────────────────────────────────────────────

    [Fact]
    public async Task TerminateSession_Success_WhenSessionBelongsToUser()
    {
        // Arrange
        var sessions = new List<UserSession>
        {
            new() { Id = 5, UserId = 10, IsActive = true, SessionToken = "tok", CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddHours(24) }
        };
        _repositoryMock.Setup(r => r.GetActiveByUserIdAsync(10)).ReturnsAsync(sessions);
        _repositoryMock.Setup(r => r.DeactivateAsync(5)).ReturnsAsync(true);

        // Act
        var result = await _service.TerminateSessionAsync(10, 5);

        // Assert
        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(r => r.DeactivateAsync(5), Times.Once);
    }

    [Fact]
    public async Task TerminateSession_Fails_WhenSessionNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetActiveByUserIdAsync(10)).ReturnsAsync(new List<UserSession>());

        // Act
        var result = await _service.TerminateSessionAsync(10, 999);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("SESSION_NOT_FOUND", ((Result<bool>.Failure)result).ErrorCode);
    }

    [Fact]
    public async Task TerminateSession_Fails_WhenSessionBelongsToAnotherUser()
    {
        // Arrange — сеанс з userId=20, але запит від userId=10
        var sessions = new List<UserSession>
        {
            new() { Id = 7, UserId = 20, IsActive = true, SessionToken = "t", CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddHours(24) }
        };
        // Симулюємо що репозиторій повертає сеанс для userId=10 (але він насправді чужий)
        var hackedSessions = new List<UserSession>
        {
            new() { Id = 7, UserId = 20, IsActive = true, SessionToken = "t", CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddHours(24) }
        };
        _repositoryMock.Setup(r => r.GetActiveByUserIdAsync(10)).ReturnsAsync(hackedSessions);

        // Act
        var result = await _service.TerminateSessionAsync(10, 7);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ACCESS_DENIED", ((Result<bool>.Failure)result).ErrorCode);
        _repositoryMock.Verify(r => r.DeactivateAsync(It.IsAny<int>()), Times.Never);
    }

    // ── TerminateAllExceptCurrent ──────────────────────────────────────────────

    [Fact]
    public async Task TerminateAllExceptCurrent_Success()
    {
        // Arrange
        _repositoryMock.Setup(r => r.DeactivateAllExceptAsync(10, "current-token")).ReturnsAsync(3);

        // Act
        var result = await _service.TerminateAllExceptCurrentAsync(10, "current-token");

        // Assert
        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(r => r.DeactivateAllExceptAsync(10, "current-token"), Times.Once);
    }

    [Fact]
    public async Task TerminateAllExceptCurrent_Fails_WhenTokenIsEmpty()
    {
        // Act
        var result = await _service.TerminateAllExceptCurrentAsync(10, "");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("INVALID_TOKEN", ((Result<bool>.Failure)result).ErrorCode);
        _repositoryMock.Verify(r => r.DeactivateAllExceptAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    // ── CreateSession ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateSession_ReturnsNonEmptyToken()
    {
        // Arrange
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<UserSession>()))
            .ReturnsAsync((UserSession s) => s);

        // Act
        var token = await _service.CreateSessionAsync(10, "127.0.0.1", "Mozilla/5.0");

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal(32, token.Length); // Guid без тире = 32 символи
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<UserSession>()), Times.Once);
    }

    // ── DeactivateSession ──────────────────────────────────────────────────────

    [Fact]
    public async Task DeactivateSession_CallsDeactivate_WhenTokenFound()
    {
        // Arrange
        var session = new UserSession { Id = 3, UserId = 10, SessionToken = "valid-token" };
        _repositoryMock.Setup(r => r.GetByTokenAsync("valid-token")).ReturnsAsync(session);
        _repositoryMock.Setup(r => r.DeactivateAsync(3)).ReturnsAsync(true);

        // Act
        await _service.DeactivateSessionAsync("valid-token");

        // Assert
        _repositoryMock.Verify(r => r.DeactivateAsync(3), Times.Once);
    }

    [Fact]
    public async Task DeactivateSession_DoesNothing_WhenTokenEmpty()
    {
        // Act
        await _service.DeactivateSessionAsync("");

        // Assert
        _repositoryMock.Verify(r => r.GetByTokenAsync(It.IsAny<string>()), Times.Never);
    }
}
