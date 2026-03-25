using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Moq;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Infrastructure.Identity;
using StayFit.Infrastructure.Services;
using System.Text;

namespace StayFit.Tests.Services;

public sealed class PasswordResetServiceTests
{
    // ─── SendPasswordResetTokenAsync — позитивні ────────────────────────────

    [Fact]
    public async Task SendPasswordResetTokenAsync_WhenUserExists_GeneratesTokenAndSendsEmail()
    {
        var user = new ApplicationUser { Id = 1, Email = "user@example.com", UserName = "user1" };
        const string rawToken = "raw-reset-token";

        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(m => m.FindByEmailAsync("user@example.com"))
            .ReturnsAsync(user);
        userManagerMock
            .Setup(m => m.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync(rawToken);

        var emailSenderMock = new Mock<IEmailSender>();

        var sut = CreateSut(userManagerMock, emailSenderMock);

        var result = await sut.SendPasswordResetTokenAsync(new ForgotPasswordDto
        {
            Email = "user@example.com",
        });

        Assert.True(result.IsSuccess);

        // Токен має бути згенерований
        userManagerMock.Verify(m => m.GeneratePasswordResetTokenAsync(user), Times.Once);

        // Email має бути відправлений
        emailSenderMock.Verify(
            m => m.SendAsync(
                "user@example.com",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetTokenAsync_WhenUserNotFound_ReturnsTrueWithoutSendingEmail()
    {
        // Захист від email enumeration — повертаємо true навіть якщо юзер не існує
        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var emailSenderMock = new Mock<IEmailSender>();

        var sut = CreateSut(userManagerMock, emailSenderMock);

        var result = await sut.SendPasswordResetTokenAsync(new ForgotPasswordDto
        {
            Email = "ghost@example.com",
        });

        Assert.True(result.IsSuccess);

        // Email не має відправлятись
        emailSenderMock.Verify(
            m => m.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);

        // Токен не має генеруватись
        userManagerMock.Verify(
            m => m.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()),
            Times.Never);
    }

    // ─── SendPasswordResetTokenAsync — негативні ────────────────────────────

    [Fact]
    public async Task SendPasswordResetTokenAsync_WhenEmailSenderThrows_ThrowsException()
    {
        var user = new ApplicationUser { Id = 2, Email = "fail@example.com", UserName = "failuser" };

        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(m => m.FindByEmailAsync("fail@example.com"))
            .ReturnsAsync(user);
        userManagerMock
            .Setup(m => m.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("some-token");

        var emailSenderMock = new Mock<IEmailSender>();
        emailSenderMock
            .Setup(m => m.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP server is down"));

        var sut = CreateSut(userManagerMock, emailSenderMock);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SendPasswordResetTokenAsync(new ForgotPasswordDto
        {
            Email = "fail@example.com",
        }));
    }

    // ─── ResetPasswordAsync — позитивні ─────────────────────────────────────

    [Fact]
    public async Task ResetPasswordAsync_WhenTokenAndUserAreValid_ReturnsEmptyErrors()
    {
        var user = new ApplicationUser { Id = 3, Email = "reset@example.com", UserName = "resetuser" };
        const string rawToken = "valid-token";
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(m => m.FindByEmailAsync("reset@example.com"))
            .ReturnsAsync(user);
        userManagerMock
            .Setup(m => m.ResetPasswordAsync(user, rawToken, "NewPass123!"))
            .ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut(userManagerMock, new Mock<IEmailSender>());

        var result = await sut.ResetPasswordAsync(new ResetPasswordDto
        {
            Email = "reset@example.com",
            Token = encodedToken,
            NewPassword = "NewPass123!",
        });

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
    }

    // ─── ResetPasswordAsync — негативні ─────────────────────────────────────

    [Fact]
    public async Task ResetPasswordAsync_WhenUserNotFound_ReturnsError()
    {
        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var sut = CreateSut(userManagerMock, new Mock<IEmailSender>());

        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("any-token"));

        var result = await sut.ResetPasswordAsync(new ResetPasswordDto
        {
            Email = "nobody@example.com",
            Token = encodedToken,
            NewPassword = "NewPass123!",
        });

        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Errors);

        userManagerMock.Verify(
            m => m.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenTokenIsInvalidBase64_ReturnsError()
    {
        var userManagerMock = CreateUserManagerMock();
        var sut = CreateSut(userManagerMock, new Mock<IEmailSender>());

        var result = await sut.ResetPasswordAsync(new ResetPasswordDto
        {
            Email = "user@example.com",
            Token = "!!!not-valid-base64!!!",
            NewPassword = "NewPass123!",
        });

        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenIdentityRejectsToken_ReturnsIdentityErrors()
    {
        var user = new ApplicationUser { Id = 4, Email = "expired@example.com", UserName = "expireduser" };
        const string rawToken = "expired-token";
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(m => m.FindByEmailAsync("expired@example.com"))
            .ReturnsAsync(user);
        userManagerMock
            .Setup(m => m.ResetPasswordAsync(user, rawToken, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "Invalid token." }));

        var sut = CreateSut(userManagerMock, new Mock<IEmailSender>());

        var result = await sut.ResetPasswordAsync(new ResetPasswordDto
        {
            Email = "expired@example.com",
            Token = encodedToken,
            NewPassword = "AnyPass123!",
        });

        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Contains("Invalid token.", result.Errors);
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenPasswordTooWeak_ReturnsAllIdentityErrors()
    {
        var user = new ApplicationUser { Id = 5, Email = "weak@example.com", UserName = "weakuser" };
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("good-token"));

        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(m => m.FindByEmailAsync("weak@example.com"))
            .ReturnsAsync(user);
        userManagerMock
            .Setup(m => m.ResetPasswordAsync(user, "good-token", "123"))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "Password too short." },
                new IdentityError { Description = "Password requires uppercase." }));

        var sut = CreateSut(userManagerMock, new Mock<IEmailSender>());

        var result = await sut.ResetPasswordAsync(new ResetPasswordDto
        {
            Email = "weak@example.com",
            Token = encodedToken,
            NewPassword = "123",
        });

        Assert.True(result.IsFailure);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains("Password too short.", result.Errors);
        Assert.Contains("Password requires uppercase.", result.Errors);
    }

    // ─── Хелпери ────────────────────────────────────────────────────────────

    private static PasswordResetService CreateSut(
        Mock<UserManager<ApplicationUser>> userManagerMock,
        Mock<IEmailSender> emailSenderMock)
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["App:BaseUrl"]).Returns("http://localhost:5250");

        return new PasswordResetService(
            userManagerMock.Object,
            emailSenderMock.Object,
            configMock.Object,
            new Mock<ILogger<PasswordResetService>>().Object);
    }

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
