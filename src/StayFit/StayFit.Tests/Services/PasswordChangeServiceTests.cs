using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StayFit.Application.Options;
using StayFit.Infrastructure.Services;
using StayFit.Infrastructure.Identity;
using StayFit.Domain.Results;
using Xunit;

namespace StayFit.Tests.Services;

public class PasswordChangeServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IOptions<PasswordSettings>> _passwordSettingsMock;
    private readonly Mock<ILogger<PasswordChangeService>> _loggerMock;
    private readonly PasswordChangeService _service;

    public PasswordChangeServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _passwordSettingsMock = new Mock<IOptions<PasswordSettings>>();
        _passwordSettingsMock.Setup(s => s.Value).Returns(new PasswordSettings { MinLength = 8 });
        
        _loggerMock = new Mock<ILogger<PasswordChangeService>>();

        _service = new PasswordChangeService(_userManagerMock.Object, _passwordSettingsMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ChangePasswordAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null!);

        // Act
        var result = await _service.ChangePasswordAsync(1, "oldPass", "newPass123", "newPass123");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("USER_NOT_FOUND", ((Result<bool>.Failure)result).ErrorCode);
    }

    [Fact]
    public async Task ChangePasswordAsync_InvalidCurrentPassword_ReturnsFailure()
    {
        // Arrange
        var user = new ApplicationUser { Id = 1, UserName = "test" };
        _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "wrongOld")).ReturnsAsync(false);

        // Act
        var result = await _service.ChangePasswordAsync(1, "wrongOld", "newPass123", "newPass123");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("INVALID_CURRENT_PASSWORD", ((Result<bool>.Failure)result).ErrorCode);
    }

    [Fact]
    public async Task ChangePasswordAsync_SamePassword_ReturnsFailure()
    {
        // Arrange
        var user = new ApplicationUser { Id = 1, UserName = "test" };
        _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "oldPass")).ReturnsAsync(true);

        // Act
        var result = await _service.ChangePasswordAsync(1, "oldPass", "oldPass", "oldPass");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("SAME_PASSWORD_ERROR", ((Result<bool>.Failure)result).ErrorCode);
    }

    [Fact]
    public async Task ChangePasswordAsync_PasswordsDoNotMatch_ReturnsFailure()
    {
        // Arrange
        var user = new ApplicationUser { Id = 1, UserName = "test" };
        _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "oldPass")).ReturnsAsync(true);

        // Act
        var result = await _service.ChangePasswordAsync(1, "oldPass", "newPass123", "differentPass");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("PASSWORD_MISMATCH", ((Result<bool>.Failure)result).ErrorCode);
    }

    [Fact]
    public async Task ChangePasswordAsync_PasswordTooShort_ReturnsFailure()
    {
        // Arrange
        var user = new ApplicationUser { Id = 1, UserName = "test" };
        _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "oldPass")).ReturnsAsync(true);

        // Act
        var result = await _service.ChangePasswordAsync(1, "oldPass", "short", "short");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("PASSWORD_TOO_SHORT", ((Result<bool>.Failure)result).ErrorCode);
    }

    [Fact]
    public async Task ChangePasswordAsync_ValidRequest_Success()
    {
        // Arrange
        var user = new ApplicationUser { Id = 1, UserName = "test" };
        _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "oldPass")).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.ChangePasswordAsync(user, "oldPass", "newPass123!"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _service.ChangePasswordAsync(1, "oldPass", "newPass123!", "newPass123!");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(((Result<bool>.Success)result).Data);
    }

    [Fact]
    public async Task ChangePasswordAsync_ChangeFailsInIdentity_ReturnsFailure()
    {
        // Arrange
        var user = new ApplicationUser { Id = 1, UserName = "test" };
        _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "oldPass")).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.ChangePasswordAsync(user, "oldPass", "newPass123!"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Identity error msg" }));

        // Act
        var result = await _service.ChangePasswordAsync(1, "oldPass", "newPass123!", "newPass123!");

        // Assert
        Assert.True(result.IsFailure);
        var failure = (Result<bool>.Failure)result;
        Assert.Equal("PASSWORD_CHANGE_FAILED", failure.ErrorCode);
        Assert.Contains("Identity error msg", failure.ErrorMessage);
    }
}
