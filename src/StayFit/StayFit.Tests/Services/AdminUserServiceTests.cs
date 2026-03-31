using Microsoft.Extensions.Logging;
using Moq;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Infrastructure.Services;

namespace StayFit.Tests.Services;

public sealed class AdminUserServiceTests
{
    [Fact]
    public async Task SearchUsersAsync_WhenRepositoryReturnsUsers_ReturnsSuccess()
    {
        var repoMock = new Mock<IAdminUserRepository>();
        var users = new List<AdminUserListItemDto>
        {
            new() { UserId = 1, UserName = "u1", Email = "u1@example.com" },
            new() { UserId = 2, UserName = "u2", Email = "u2@example.com" },
        };

        repoMock
            .Setup(r => r.SearchUsersAsync(1, "u1@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var sut = CreateSut(repoMock);

        var result = await sut.SearchUsersAsync(new AdminUserSearchRequestDto
        {
            UserId = 1,
            Email = "u1@example.com",
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task GetUserDetailsAsync_WhenUserExists_ReturnsSuccess()
    {
        var repoMock = new Mock<IAdminUserRepository>();
        repoMock
            .Setup(r => r.GetUserDetailsAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminUserDetailsDto
            {
                UserId = 42,
                UserName = "admin",
                Email = "admin@example.com",
                AccessFailedCount = 0,
            });

        var sut = CreateSut(repoMock);

        var result = await sut.GetUserDetailsAsync(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value!.UserId);
    }

    [Fact]
    public async Task GetUserDetailsAsync_WhenUserNotFound_ReturnsFailure()
    {
        var repoMock = new Mock<IAdminUserRepository>();
        repoMock
            .Setup(r => r.GetUserDetailsAsync(404, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminUserDetailsDto?)null);

        var sut = CreateSut(repoMock);

        var result = await sut.GetUserDetailsAsync(404);

        Assert.True(result.IsFailure);
        Assert.Contains("Користувача не знайдено", result.Errors);
    }

    [Fact]
    public async Task BlockUserAsync_WhenRepositorySucceeds_ReturnsSuccess()
    {
        var repoMock = new Mock<IAdminUserRepository>();
        repoMock
            .Setup(r => r.BlockUserAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        var sut = CreateSut(repoMock);

        var result = await sut.BlockUserAsync(7);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task BlockUserAsync_WhenRepositoryFails_ReturnsFailure()
    {
        var repoMock = new Mock<IAdminUserRepository>();
        repoMock
            .Setup(r => r.BlockUserAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, (IReadOnlyList<string>)new[] { "Cannot block" }));

        var sut = CreateSut(repoMock);

        var result = await sut.BlockUserAsync(7);

        Assert.True(result.IsFailure);
        Assert.Contains("Cannot block", result.Errors);
    }

    [Fact]
    public async Task UnblockUserAsync_WhenRepositorySucceeds_ReturnsSuccess()
    {
        var repoMock = new Mock<IAdminUserRepository>();
        repoMock
            .Setup(r => r.UnblockUserAsync(8, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        var sut = CreateSut(repoMock);

        var result = await sut.UnblockUserAsync(8);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenPasswordIsEmpty_ReturnsFailureAndSkipsRepository()
    {
        var repoMock = new Mock<IAdminUserRepository>();
        var sut = CreateSut(repoMock);

        var result = await sut.ResetPasswordAsync(3, "   ");

        Assert.True(result.IsFailure);
        Assert.Contains("Новий пароль обов'язковий", result.Errors);

        repoMock.Verify(
            r => r.ResetPasswordAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenRepositoryFails_ReturnsFailureWithErrors()
    {
        var repoMock = new Mock<IAdminUserRepository>();
        repoMock
            .Setup(r => r.ResetPasswordAsync(3, "StrongPass123!", It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, (IReadOnlyList<string>)new[] { "Policy error" }));

        var sut = CreateSut(repoMock);

        var result = await sut.ResetPasswordAsync(3, "StrongPass123!");

        Assert.True(result.IsFailure);
        Assert.Contains("Policy error", result.Errors);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenUserNameIsEmpty_ReturnsFailureAndSkipsRepository()
    {
        var repoMock = new Mock<IAdminUserRepository>();
        var sut = CreateSut(repoMock);

        var result = await sut.UpdateUserAsync(12, new AdminUpdateUserRequestDto
        {
            UserName = " ",
            Email = "user@example.com",
        });

        Assert.True(result.IsFailure);
        Assert.Contains("Ім'я користувача обов'язкове", result.Errors);

        repoMock.Verify(
            r => r.UpdateUserAsync(It.IsAny<int>(), It.IsAny<AdminUpdateUserRequestDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenRepositorySucceeds_ReturnsSuccess()
    {
        var repoMock = new Mock<IAdminUserRepository>();
        var request = new AdminUpdateUserRequestDto
        {
            UserName = "new-name",
            Email = "new@example.com",
            FullName = "New Name",
        };

        repoMock
            .Setup(r => r.UpdateUserAsync(12, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        var sut = CreateSut(repoMock);

        var result = await sut.UpdateUserAsync(12, request);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenRepositoryFails_ReturnsFailure()
    {
        var repoMock = new Mock<IAdminUserRepository>();
        var request = new AdminUpdateUserRequestDto
        {
            UserName = "new-name",
            Email = "new@example.com",
        };

        repoMock
            .Setup(r => r.UpdateUserAsync(12, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, (IReadOnlyList<string>)new[] { "Duplicate email" }));

        var sut = CreateSut(repoMock);

        var result = await sut.UpdateUserAsync(12, request);

        Assert.True(result.IsFailure);
        Assert.Contains("Duplicate email", result.Errors);
    }

    private static AdminUserService CreateSut(Mock<IAdminUserRepository> repoMock) =>
        new(repoMock.Object, new Mock<ILogger<AdminUserService>>().Object);
}
