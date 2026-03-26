using Moq;
using Xunit;
using StayFit.Application.Interfaces;
using StayFit.Application.Services;
using StayFit.Domain.Results;
using StayFit.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace StayFit.Tests.Services;

public class AccountSecurityServiceTests
{
    private readonly Mock<IAccountSecurityRepository> _mockRepository;
    private readonly Mock<ILogger<AccountSecurityService>> _mockLogger;
    private readonly AccountSecurityService _service;

    public AccountSecurityServiceTests()
    {
        _mockRepository = new Mock<IAccountSecurityRepository>();
        _mockLogger = new Mock<ILogger<AccountSecurityService>>();
        _service = new AccountSecurityService(_mockRepository.Object, _mockLogger.Object);
    }

    // ────────────────────────────────────────────────────────────────────────
    // ChangePasswordAsync - Позитивні тести
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ChangePasswordAsync_WithValidPassword_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        const string currentPassword = "OldPassword123";
        const string newPassword = "NewPassword123";
        const string confirmPassword = "NewPassword123";

        _mockRepository
            .Setup(r => r.ChangePasswordAsync(userId, currentPassword, newPassword))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ChangePasswordAsync(userId, currentPassword, newPassword, confirmPassword);

        // Assert
        Assert.IsType<Result<bool>.Success>(result);
        var success = (Result<bool>.Success)result;
        Assert.True(success.Data);
        _mockRepository.Verify(r => r.ChangePasswordAsync(userId, currentPassword, newPassword), Times.Once);
    }

    // ────────────────────────────────────────────────────────────────────────
    // ChangePasswordAsync - Негативні тести
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ChangePasswordAsync_WithShortPassword_ReturnsFail()
    {
        // Arrange
        var userId = 1;
        const string currentPassword = "OldPassword123";
        const string newPassword = "Short1"; // Менше 8 символів
        const string confirmPassword = "Short1";

        // Act
        var result = await _service.ChangePasswordAsync(userId, currentPassword, newPassword, confirmPassword);

        // Assert
        Assert.IsType<Result<bool>.Failure>(result);
        var failure = (Result<bool>.Failure)result;
        Assert.Equal("INVALID_PASSWORD", failure.ErrorCode);
        _mockRepository.Verify(r => r.ChangePasswordAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithSamePassword_ReturnsFail()
    {
        // Arrange
        var userId = 1;
        const string password = "SamePassword123";

        // Act
        var result = await _service.ChangePasswordAsync(userId, password, password, password);

        // Assert
        Assert.IsType<Result<bool>.Failure>(result);
        var failure = (Result<bool>.Failure)result;
        Assert.Equal("PASSWORD_SAME", failure.ErrorCode);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithEmptyPassword_ReturnsFail()
    {
        // Arrange
        var userId = 1;
        const string currentPassword = "OldPassword123";
        const string newPassword = "";
        const string confirmPassword = "";

        // Act
        var result = await _service.ChangePasswordAsync(userId, currentPassword, newPassword, confirmPassword);

        // Assert
        Assert.IsType<Result<bool>.Failure>(result);
        var failure = (Result<bool>.Failure)result;
        Assert.Equal("INVALID_PASSWORD", failure.ErrorCode);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithPasswordMismatch_ReturnsFail()
    {
        // Arrange
        var userId = 1;
        const string currentPassword = "OldPassword123";
        const string newPassword = "NewPassword123";
        const string confirmPassword = "DifferentPassword123";

        // Act
        var result = await _service.ChangePasswordAsync(userId, currentPassword, newPassword, confirmPassword);

        // Assert
        Assert.IsType<Result<bool>.Failure>(result);
        var failure = (Result<bool>.Failure)result;
        Assert.Equal("PASSWORD_MISMATCH", failure.ErrorCode);
        _mockRepository.Verify(r => r.ChangePasswordAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ChangePasswordAsync_RepositoryFails_ReturnsFail()
    {
        // Arrange
        var userId = 1;
        const string currentPassword = "OldPassword123";
        const string newPassword = "NewPassword123";
        const string confirmPassword = "NewPassword123";

        _mockRepository
            .Setup(r => r.ChangePasswordAsync(userId, currentPassword, newPassword))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ChangePasswordAsync(userId, currentPassword, newPassword, confirmPassword);

        // Assert
        Assert.IsType<Result<bool>.Failure>(result);
        var failure = (Result<bool>.Failure)result;
        Assert.Equal("PASSWORD_CHANGE_FAILED", failure.ErrorCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // GetActiveSessionsAsync - Позитивні тести
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetActiveSessionsAsync_WithValidUserId_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var sessions = new List<UserSession>
        {
            new() { Id = 1, UserId = userId, IpAddress = "192.168.1.1" },
            new() { Id = 2, UserId = userId, IpAddress = "192.168.1.2" }
        };

        _mockRepository
            .Setup(r => r.GetActiveSessionsAsync(userId))
            .ReturnsAsync(sessions);

        // Act
        var result = await _service.GetActiveSessionsAsync(userId);

        // Assert
        Assert.IsType<Result<IEnumerable<UserSession>>.Success>(result);
        var success = (Result<IEnumerable<UserSession>>.Success)result;
        Assert.Equal(2, success.Data.Count());
    }

    // ────────────────────────────────────────────────────────────────────────
    // LogoutAllSessionsAsync - Позитивні тести
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LogoutAllSessionsAsync_WithValidUserId_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;

        _mockRepository
            .Setup(r => r.InvalidateAllSessionsAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.LogoutAllSessionsAsync(userId);

        // Assert
        Assert.IsType<Result<bool>.Success>(result);
        _mockRepository.Verify(r => r.InvalidateAllSessionsAsync(userId), Times.Once);
    }

    // ────────────────────────────────────────────────────────────────────────
    // LogoutAllSessionsAsync - Негативні тести
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LogoutAllSessionsAsync_RepositoryFails_ReturnsFail()
    {
        // Arrange
        var userId = 1;

        _mockRepository
            .Setup(r => r.InvalidateAllSessionsAsync(userId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.LogoutAllSessionsAsync(userId);

        // Assert
        Assert.IsType<Result<bool>.Failure>(result);
        var failure = (Result<bool>.Failure)result;
        Assert.Equal("LOGOUT_FAILED", failure.ErrorCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // DeleteAccountAsync - Позитивні тести
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAccountAsync_WithValidConfirmation_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        const string confirmationToken = "valid-token";

        _mockRepository
            .Setup(r => r.UserExistsAsync(userId))
            .ReturnsAsync(true);

        _mockRepository
            .Setup(r => r.DeleteAccountAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAccountAsync(userId, confirmationToken);

        // Assert
        Assert.IsType<Result<bool>.Success>(result);
        _mockRepository.Verify(r => r.DeleteAccountAsync(userId), Times.Once);
    }

    // ────────────────────────────────────────────────────────────────────────
    // DeleteAccountAsync - Негативні тести
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAccountAsync_WithoutConfirmationToken_ReturnsFail()
    {
        // Arrange
        var userId = 1;
        const string confirmationToken = "";

        // Act
        var result = await _service.DeleteAccountAsync(userId, confirmationToken);

        // Assert
        Assert.IsType<Result<bool>.Failure>(result);
        var failure = (Result<bool>.Failure)result;
        Assert.Equal("MISSING_CONFIRMATION", failure.ErrorCode);
    }

    [Fact]
    public async Task DeleteAccountAsync_UserNotFound_ReturnsFail()
    {
        // Arrange
        var userId = 999;
        const string confirmationToken = "valid-token";

        _mockRepository
            .Setup(r => r.UserExistsAsync(userId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteAccountAsync(userId, confirmationToken);

        // Assert
        Assert.IsType<Result<bool>.Failure>(result);
        var failure = (Result<bool>.Failure)result;
        Assert.Equal("USER_NOT_FOUND", failure.ErrorCode);
    }

    [Fact]
    public async Task DeleteAccountAsync_RepositoryFails_ReturnsFail()
    {
        // Arrange
        var userId = 1;
        const string confirmationToken = "valid-token";

        _mockRepository
            .Setup(r => r.UserExistsAsync(userId))
            .ReturnsAsync(true);

        _mockRepository
            .Setup(r => r.DeleteAccountAsync(userId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteAccountAsync(userId, confirmationToken);

        // Assert
        Assert.IsType<Result<bool>.Failure>(result);
        var failure = (Result<bool>.Failure)result;
        Assert.Equal("DELETE_FAILED", failure.ErrorCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Exception handling
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ChangePasswordAsync_WhenExceptionThrown_Throws()
    {
        // Arrange
        var userId = 1;
        const string currentPassword = "OldPassword123";
        const string newPassword = "NewPassword123";
        const string confirmPassword = "NewPassword123";

        _mockRepository
            .Setup(r => r.ChangePasswordAsync(userId, currentPassword, newPassword))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ChangePasswordAsync(userId, currentPassword, newPassword, confirmPassword));
    }
}
