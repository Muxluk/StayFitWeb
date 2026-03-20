using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StayFit.Application.DTOs;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Identity;
using StayFit.Infrastructure.Services;

namespace StayFit.Tests.Services;

public sealed class RegistrationServiceTests
{
    [Fact]
    public async Task RegisterAsync_WhenIdentitySucceeds_ReturnsSucceededResultWithUserId()
    {
        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationUser, string>((user, _) => user.Id = 123);

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        var loggerMock = new Mock<ILogger<RegistrationService>>();
        var sut = new RegistrationService(userManagerMock.Object, userRepositoryMock.Object, loggerMock.Object);

        var request = new RegisterUserRequestDto
        {
            Email = "test@example.com",
            UserName = "testuser",
            Password = "password123",
        };

        var result = await sut.RegisterAsync(request);

        Assert.True(result.Succeeded);
        Assert.Equal("123", result.UserId);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task RegisterAsync_WhenIdentityFails_ReturnsFailedResultWithErrors()
    {
        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "Email is already taken." },
                new IdentityError { Description = "Password is too weak." }));

        var userRepositoryMock = new Mock<IUserRepository>();
        var loggerMock = new Mock<ILogger<RegistrationService>>();
        var sut = new RegistrationService(userManagerMock.Object, userRepositoryMock.Object, loggerMock.Object);

        var request = new RegisterUserRequestDto
        {
            Email = "taken@example.com",
            UserName = "testuser",
            Password = "weak",
        };

        var result = await sut.RegisterAsync(request);

        Assert.False(result.Succeeded);
        Assert.Null(result.UserId);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains("Email is already taken.", result.Errors);
        Assert.Contains("Password is too weak.", result.Errors);
    }

    [Fact]
    public async Task RegisterAsync_PassesExpectedUserFieldsAndPasswordToIdentity()
    {
        var capturedUser = default(ApplicationUser);
        var capturedPassword = string.Empty;

        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationUser, string>((user, password) =>
            {
                capturedUser = user;
                capturedPassword = password;
                user.Id = 999;
            });

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        var loggerMock = new Mock<ILogger<RegistrationService>>();
        var sut = new RegistrationService(userManagerMock.Object, userRepositoryMock.Object, loggerMock.Object);

        var request = new RegisterUserRequestDto
        {
            Email = "person@stayfit.local",
            UserName = "person1",
            Password = "pass-123",
        };

        await sut.RegisterAsync(request);

        userManagerMock.Verify(
            m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Once);

        Assert.NotNull(capturedUser);
        Assert.Equal(request.Email, capturedUser!.Email);
        Assert.Equal(request.UserName, capturedUser.UserName);
        Assert.Equal(request.Password, capturedPassword);
    }

    [Theory]
    [InlineData("E1")]
    [InlineData("E1|E2")]
    [InlineData("E1|E2|E3")]
    public async Task RegisterAsync_WhenIdentityReturnsErrors_MapsAllErrorDescriptions(string joinedErrors)
    {
        var errors = joinedErrors
            .Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(e => new IdentityError { Description = e })
            .ToArray();

        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(errors));

        var userRepositoryMock = new Mock<IUserRepository>();
        var loggerMock = new Mock<ILogger<RegistrationService>>();
        var sut = new RegistrationService(userManagerMock.Object, userRepositoryMock.Object, loggerMock.Object);

        var request = new RegisterUserRequestDto
        {
            Email = "x@x.local",
            UserName = "x",
            Password = "x",
        };

        var result = await sut.RegisterAsync(request);

        Assert.False(result.Succeeded);
        Assert.Equal(errors.Length, result.Errors.Count);
        foreach (var e in errors)
        {
            Assert.Contains(e.Description, result.Errors);
        }
    }

    [Fact]
    public async Task RegisterAsync_WhenIdentityThrows_PropagatesException()
    {
        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var userRepositoryMock = new Mock<IUserRepository>();
        var loggerMock = new Mock<ILogger<RegistrationService>>();
        var sut = new RegistrationService(userManagerMock.Object, userRepositoryMock.Object, loggerMock.Object);

        var request = new RegisterUserRequestDto
        {
            Email = "t@t.local",
            UserName = "t",
            Password = "t",
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.RegisterAsync(request));
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
