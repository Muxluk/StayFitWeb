using Microsoft.Extensions.Logging;
using Moq;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Application.Services;

namespace StayFit.Tests.Services;

public sealed class AdminAccountEditServiceTests
{
    #region ValidateUpdateRequest Tests - Позитивні сценарії

    [Fact]
    public void ValidateUpdateRequest_WithValidData_ReturnsSuccess()
    {
        var loggerMock = new Mock<ILogger<AdminAccountEditService>>();
        var sut = new AdminAccountEditService(loggerMock.Object);

        var request = new AdminUpdateUserRequestDto
        {
            UserName = "validuser",
            Email = "user@example.com",
            FullName = "Valid User",
            DateOfBirth = new DateOnly(1990, 5, 15),
            Gender = "Чоловік",
            Weight = 75,
            Height = 180,
        };

        var result = sut.ValidateUpdateRequest(request, 1);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateUpdateRequest_WithMinimalValidData_ReturnsSuccess()
    {
        var loggerMock = new Mock<ILogger<AdminAccountEditService>>();
        var sut = new AdminAccountEditService(loggerMock.Object);

        var request = new AdminUpdateUserRequestDto
        {
            UserName = "usr",
            Email = "u@ex.co",
        };

        var result = sut.ValidateUpdateRequest(request, 1);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateUpdateRequest_WithBoundaryValues_ReturnsSuccess()
    {
        var loggerMock = new Mock<ILogger<AdminAccountEditService>>();
        var sut = new AdminAccountEditService(loggerMock.Object);

        var request = new AdminUpdateUserRequestDto
        {
            UserName = "abc", // Min 3 chars
            Email = "a@b.c",
            FullName = new string('a', 100), // Max 100 chars
            Weight = 20, // Min
            Height = 250, // Max
        };

        var result = sut.ValidateUpdateRequest(request, 1);

        Assert.True(result.IsSuccess);
    }

    #endregion

    #region ValidateUpdateRequest Tests - Негативні сценарії

    [Fact]
    public void ValidateUpdateRequest_WithEmptyUserName_ReturnsFailure()
    {
        var loggerMock = new Mock<ILogger<AdminAccountEditService>>();
        var sut = new AdminAccountEditService(loggerMock.Object);

        var request = new AdminUpdateUserRequestDto
        {
            UserName = "",
            Email = "user@example.com",
        };

        var result = sut.ValidateUpdateRequest(request, 1);

        Assert.True(result.IsFailure);
        Assert.Contains("Ім'я користувача обов'язкове", result.Errors);
    }

    [Fact]
    public void ValidateUpdateRequest_WithNullUserName_ReturnsFailure()
    {
        var loggerMock = new Mock<ILogger<AdminAccountEditService>>();
        var sut = new AdminAccountEditService(loggerMock.Object);

        var request = new AdminUpdateUserRequestDto
        {
            UserName = "   ",  // Whitespace only
            Email = "user@example.com",
        };

        var result = sut.ValidateUpdateRequest(request, 1);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ValidateUpdateRequest_WithUserNameTooShort_ReturnsFailure()
    {
        var loggerMock = new Mock<ILogger<AdminAccountEditService>>();
        var sut = new AdminAccountEditService(loggerMock.Object);

        var request = new AdminUpdateUserRequestDto
        {
            UserName = "ab", // Less than 3 chars
            Email = "user@example.com",
        };

        var result = sut.ValidateUpdateRequest(request, 1);

        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Contains("3", result.Errors[0]);
        Assert.Contains("50", result.Errors[0]);
    }

    [Fact]
    public void ValidateUpdateRequest_WithUserNameTooLong_ReturnsFailure()
    {
        var loggerMock = new Mock<ILogger<AdminAccountEditService>>();
        var sut = new AdminAccountEditService(loggerMock.Object);

        var request = new AdminUpdateUserRequestDto
        {
            UserName = new string('a', 51), // More than 50 chars
            Email = "user@example.com",
        };

        var result = sut.ValidateUpdateRequest(request, 1);

        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Contains("3", result.Errors[0]);
        Assert.Contains("50", result.Errors[0]);
    }

    [Fact]
    public void ValidateUpdateRequest_WithEmptyEmail_ReturnsFailure()
    {
        var loggerMock = new Mock<ILogger<AdminAccountEditService>>();
        var sut = new AdminAccountEditService(loggerMock.Object);

        var request = new AdminUpdateUserRequestDto
        {
            UserName = "validuser",
            Email = "",
        };

        var result = sut.ValidateUpdateRequest(request, 1);

        Assert.True(result.IsFailure);
        Assert.Contains("Email обов'язковий", result.Errors);
    }

    [Fact]
    public void ValidateUpdateRequest_WithInvalidEmailFormat_ReturnsFailure()
    {
        var loggerMock = new Mock<ILogger<AdminAccountEditService>>();
        var sut = new AdminAccountEditService(loggerMock.Object);

        var request = new AdminUpdateUserRequestDto
        {
            UserName = "validuser",
            Email = "notanemail",
        };

        var result = sut.ValidateUpdateRequest(request, 1);

        Assert.True(result.IsFailure);
        Assert.Contains("Некоректний формат email", result.Errors);
    }

    [Fact]
    public void ValidateUpdateRequest_WithFullNameTooLong_ReturnsFailure()
    {
        var loggerMock = new Mock<ILogger<AdminAccountEditService>>();
        var sut = new AdminAccountEditService(loggerMock.Object);

        var request = new AdminUpdateUserRequestDto
        {
            UserName = "validuser",
            Email = "user@example.com",
            FullName = new string('a', 101), // More than 100 chars
        };

        var result = sut.ValidateUpdateRequest(request, 1);

        Assert.Single(result.Errors);
        Assert.Contains("100 символів", result.Errors[0]);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ValidateUpdateRequest_WithGenderTooLong_ReturnsFailure()
    {
        var loggerMock = new Mock<ILogger<AdminAccountEditService>>();
        var sut = new AdminAccountEditService(loggerMock.Object);

        var request = new AdminUpdateUserRequestDto
        {
            UserName = "validuser",
            Email = "user@example.com",
            Gender = new string('a', 21), // More than 20 chars
        };

        var result = sut.ValidateUpdateRequest(request, 1);

        Assert.True(result.IsFailure);
        Assert.Contains("Стать не може перевищувати 20 символів", result.Errors);
    }

    [Fact]
    public void ValidateUpdateRequest_WithWeightTooLow_ReturnsFailure()
    {
        var loggerMock = new Mock<ILogger<AdminAccountEditService>>();
        var sut = new AdminAccountEditService(loggerMock.Object);

        var request = new AdminUpdateUserRequestDto
        {
            UserName = "validuser",
            Email = "user@example.com",
            Weight = 19, // Less than 20
        };

        var result = sut.ValidateUpdateRequest(request, 1);

        Assert.True(result.IsFailure);
        Assert.Contains("Вага має бути від 20 до 300 кг", result.Errors);
    }

    [Fact]
    public void ValidateUpdateRequest_WithWeightTooHigh_ReturnsFailure()
    {
        var loggerMock = new Mock<ILogger<AdminAccountEditService>>();
        var sut = new AdminAccountEditService(loggerMock.Object);

        var request = new AdminUpdateUserRequestDto
        {
            UserName = "validuser",
            Email = "user@example.com",
            Weight = 301, // More than 300
        };

        var result = sut.ValidateUpdateRequest(request, 1);

        Assert.True(result.IsFailure);
        Assert.Contains("Вага має бути від 20 до 300 кг", result.Errors);
    }

    [Fact]
    public void ValidateUpdateRequest_WithHeightTooLow_ReturnsFailure()
    {
        var loggerMock = new Mock<ILogger<AdminAccountEditService>>();
        var sut = new AdminAccountEditService(loggerMock.Object);

        var request = new AdminUpdateUserRequestDto
        {
            UserName = "validuser",
            Email = "user@example.com",
            Height = 99, // Less than 100
        };

        var result = sut.ValidateUpdateRequest(request, 1);

        Assert.True(result.IsFailure);
        Assert.Contains("Зріст має бути від 100 до 250 см", result.Errors);
    }

    [Fact]
    public void ValidateUpdateRequest_WithHeightTooHigh_ReturnsFailure()
    {
        var loggerMock = new Mock<ILogger<AdminAccountEditService>>();
        var sut = new AdminAccountEditService(loggerMock.Object);

        var request = new AdminUpdateUserRequestDto
        {
            UserName = "validuser",
            Email = "user@example.com",
            Height = 251, // More than 250
        };

        var result = sut.ValidateUpdateRequest(request, 1);

        Assert.True(result.IsFailure);
        Assert.Contains("Зріст має бути від 100 до 250 см", result.Errors);
    }

    [Fact]
    public void ValidateUpdateRequest_WithUserAgeTooYoung_ReturnsFailure()
    {
        var loggerMock = new Mock<ILogger<AdminAccountEditService>>();
        var sut = new AdminAccountEditService(loggerMock.Object);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var tooYoung = today.AddYears(-12); // 12 years old

        var request = new AdminUpdateUserRequestDto
        {
            UserName = "validuser",
            Email = "user@example.com",
            DateOfBirth = tooYoung,
        };

        var result = sut.ValidateUpdateRequest(request, 1);

        Assert.Single(result.Errors);
        Assert.Contains("років", result.Errors[0]);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ValidateUpdateRequest_WithUserAgeTooOld_ReturnsFailure()
    {
        var loggerMock = new Mock<ILogger<AdminAccountEditService>>();
        var sut = new AdminAccountEditService(loggerMock.Object);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var tooOld = today.AddYears(-121); // 121 years old

        var request = new AdminUpdateUserRequestDto
        {
            UserName = "validuser",
            Email = "user@example.com",
            DateOfBirth = tooOld,
        };

        var result = sut.ValidateUpdateRequest(request, 1);

        Assert.True(result.IsFailure);
        Assert.Contains("Некоректна дата народження", result.Errors);
    }

    #endregion

    #region IsEmailUniqueAsync Tests

    [Fact]
    public async Task IsEmailUniqueAsync_WhenEmailNotUsed_ReturnsTrue()
    {
        var loggerMock = new Mock<ILogger<AdminAccountEditService>>();
        var repoMock = new Mock<IAdminUserRepository>();

        repoMock
            .Setup(r => r.SearchUsersAsync(null, "newemail@example.com", 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<AdminUserListItemDto> { Items = new List<AdminUserListItemDto>() });

        var sut = new AdminAccountEditService(loggerMock.Object);

        var result = await sut.IsEmailUniqueAsync("newemail@example.com", 1, repoMock.Object);

        Assert.True(result);
    }

    [Fact]
    public async Task IsEmailUniqueAsync_WhenEmailUsedBySameUser_ReturnsTrue()
    {
        var loggerMock = new Mock<ILogger<AdminAccountEditService>>();
        var repoMock = new Mock<IAdminUserRepository>();

        var users = new List<AdminUserListItemDto>
        {
            new() { UserId = 1, UserName = "user", Email = "same@example.com" },
        };

        repoMock
            .Setup(r => r.SearchUsersAsync(null, "same@example.com", 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<AdminUserListItemDto> { Items = users });

        var sut = new AdminAccountEditService(loggerMock.Object);

        var result = await sut.IsEmailUniqueAsync("same@example.com", 1, repoMock.Object);

        Assert.True(result); // Same user, so it's unique
    }

    [Fact]
    public async Task IsEmailUniqueAsync_WhenEmailUsedByDifferentUser_ReturnsFalse()
    {
        var loggerMock = new Mock<ILogger<AdminAccountEditService>>();
        var repoMock = new Mock<IAdminUserRepository>();

        var users = new List<AdminUserListItemDto>
        {
            new() { UserId = 2, UserName = "anotheruser", Email = "taken@example.com" },
        };

        repoMock
            .Setup(r => r.SearchUsersAsync(null, "taken@example.com", 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<AdminUserListItemDto> { Items = users });

        var sut = new AdminAccountEditService(loggerMock.Object);

        var result = await sut.IsEmailUniqueAsync("taken@example.com", 1, repoMock.Object);

        Assert.False(result); // Different user, so it's not unique
    }

    [Fact]
    public async Task IsEmailUniqueAsync_WithEmptyEmail_ReturnsFalse()
    {
        var loggerMock = new Mock<ILogger<AdminAccountEditService>>();
        var repoMock = new Mock<IAdminUserRepository>();

        var sut = new AdminAccountEditService(loggerMock.Object);

        var result = await sut.IsEmailUniqueAsync("", 1, repoMock.Object);

        Assert.False(result);
    }

    #endregion
}
