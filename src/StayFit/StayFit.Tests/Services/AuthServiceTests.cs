using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StayFit.Application.DTOs;
using StayFit.Infrastructure.Identity;
using StayFit.Infrastructure.Services;

namespace StayFit.Tests.Services;

public sealed class AuthServiceTests
{
    // ─── Позитивні сценарії ──────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ReturnsSuccess()
    {
        var user = new ApplicationUser { Id = 1, UserName = "testuser", Email = "test@example.com" };

        var userManagerMock = CreateUserManagerMock();
        userManagerMock.Setup(m => m.FindByEmailAsync("test@example.com")).ReturnsAsync(user);
        userManagerMock.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(false);
        userManagerMock.Setup(m => m.CheckPasswordAsync(user, "password123")).ReturnsAsync(true);
        userManagerMock.Setup(m => m.ResetAccessFailedCountAsync(user)).ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut(userManagerMock);

        var result = await sut.LoginAsync(new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "password123",
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("testuser", result.Value);
        Assert.Empty(result.Errors);
    }

    // ─── Негативні сценарії ──────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ReturnsFailure()
    {
        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var sut = CreateSut(userManagerMock);

        var result = await sut.LoginAsync(new LoginRequestDto
        {
            Email = "nobody@example.com",
            Password = "password123",
        });

        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Errors);

        // CheckPassword не має викликатись якщо юзер не знайдений
        userManagerMock.Verify(
            m => m.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsWrong_ReturnsFailure()
    {
        var user = new ApplicationUser { Id = 2, UserName = "user2", Email = "user2@example.com" };

        var userManagerMock = CreateUserManagerMock();
        userManagerMock.Setup(m => m.FindByEmailAsync("user2@example.com")).ReturnsAsync(user);
        userManagerMock.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(false);
        userManagerMock.Setup(m => m.CheckPasswordAsync(user, "wrongpass")).ReturnsAsync(false);
        userManagerMock.Setup(m => m.AccessFailedAsync(user)).ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut(userManagerMock);

        var result = await sut.LoginAsync(new LoginRequestDto
        {
            Email = "user2@example.com",
            Password = "wrongpass",
        });

        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Errors);

        // Лічильник невдалих спроб має збільшитись
        userManagerMock.Verify(m => m.AccessFailedAsync(user), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenAccountIsLockedOut_ReturnsFailureWithLockoutMessage()
    {
        var user = new ApplicationUser { Id = 3, UserName = "lockeduser", Email = "locked@example.com" };

        var userManagerMock = CreateUserManagerMock();
        userManagerMock.Setup(m => m.FindByEmailAsync("locked@example.com")).ReturnsAsync(user);
        userManagerMock.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(true);

        var sut = CreateSut(userManagerMock);

        var result = await sut.LoginAsync(new LoginRequestDto
        {
            Email = "locked@example.com",
            Password = "anypass",
        });

        Assert.True(result.IsFailure);
        Assert.Contains("Акаунт заблоковано", result.Errors[0]);

        // CheckPassword не має викликатись для заблокованого акаунту
        userManagerMock.Verify(
            m => m.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsValid_ResetsAccessFailedCount()
    {
        var user = new ApplicationUser { Id = 4, UserName = "gooduser", Email = "good@example.com" };

        var userManagerMock = CreateUserManagerMock();
        userManagerMock.Setup(m => m.FindByEmailAsync("good@example.com")).ReturnsAsync(user);
        userManagerMock.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(false);
        userManagerMock.Setup(m => m.CheckPasswordAsync(user, "correctpass")).ReturnsAsync(true);
        userManagerMock.Setup(m => m.ResetAccessFailedCountAsync(user)).ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut(userManagerMock);

        await sut.LoginAsync(new LoginRequestDto
        {
            Email = "good@example.com",
            Password = "correctpass",
        });

        userManagerMock.Verify(m => m.ResetAccessFailedCountAsync(user), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ErrorMessage_DoesNotRevealWhetherUserExists()
    {
        var user = new ApplicationUser { Id = 5, UserName = "u", Email = "u@x.com" };

        var userManagerMock = CreateUserManagerMock();
        userManagerMock.Setup(m => m.FindByEmailAsync("notexist@x.com")).ReturnsAsync((ApplicationUser?)null);
        userManagerMock.Setup(m => m.FindByEmailAsync("u@x.com")).ReturnsAsync(user);
        userManagerMock.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(false);
        userManagerMock.Setup(m => m.CheckPasswordAsync(user, "wrongpass")).ReturnsAsync(false);
        userManagerMock.Setup(m => m.AccessFailedAsync(user)).ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut(userManagerMock);

        var notFoundResult = await sut.LoginAsync(new LoginRequestDto { Email = "notexist@x.com", Password = "x" });
        var wrongPassResult = await sut.LoginAsync(new LoginRequestDto { Email = "u@x.com", Password = "wrongpass" });

        // Однакові повідомлення — не розкриваємо причину
        Assert.Equal(notFoundResult.Errors[0], wrongPassResult.Errors[0]);
    }

    // ─── Хелпери ────────────────────────────────────────────────────────────

    private static AuthService CreateSut(Mock<UserManager<ApplicationUser>> userManagerMock) =>
        new(userManagerMock.Object, new Mock<ILogger<AuthService>>().Object);

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            Options.Create(new IdentityOptions()),
            new Mock<IPasswordHasher<ApplicationUser>>().Object,
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new Mock<ILookupNormalizer>().Object,
            new IdentityErrorDescriber(),
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<ApplicationUser>>>().Object);
    }
}
