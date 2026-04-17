using Microsoft.Extensions.Logging;
using Moq;
using StayFit.Application.Interfaces;
using StayFit.Application.Services;

namespace StayFit.Tests.Services;

public sealed class AccountDeletionServiceTests
{
    [Fact]
    public async Task DeleteAccountAsync_WhenPasswordIsEmpty_ReturnsFailure()
    {
        var repoMock = new Mock<IAccountDeletionRepository>();
        var loggerMock = new Mock<ILogger<AccountDeletionService>>();
        var sut = new AccountDeletionService(repoMock.Object, loggerMock.Object);

        var result = await sut.DeleteAccountAsync(1, "   ");

        Assert.True(result.IsFailure);
        Assert.Contains("Потрібно ввести пароль", result.Errors[0]);
        repoMock.Verify(r => r.DeleteUserDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAccountAsync_WhenPasswordIsIncorrect_ReturnsFailure()
    {
        var repoMock = new Mock<IAccountDeletionRepository>();
        repoMock.Setup(r => r.CheckPasswordAsync(1, "wrong_pass")).ReturnsAsync(false);
        
        var loggerMock = new Mock<ILogger<AccountDeletionService>>();
        var sut = new AccountDeletionService(repoMock.Object, loggerMock.Object);

        var result = await sut.DeleteAccountAsync(1, "wrong_pass");

        Assert.True(result.IsFailure);
        Assert.Contains("Невірний пароль", result.Errors[0]);
        repoMock.Verify(r => r.DeleteUserDataAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAccountAsync_WhenRepositoryFails_ReturnsFailure()
    {
        var repoMock = new Mock<IAccountDeletionRepository>();
        repoMock.Setup(r => r.CheckPasswordAsync(1, "correct_pass")).ReturnsAsync(true);
        repoMock.Setup(r => r.DeleteUserDataAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        
        var loggerMock = new Mock<ILogger<AccountDeletionService>>();
        var sut = new AccountDeletionService(repoMock.Object, loggerMock.Object);

        var result = await sut.DeleteAccountAsync(1, "correct_pass");

        Assert.True(result.IsFailure);
        Assert.Contains("внутрішню помилку", result.Errors[0]);
    }

    [Fact]
    public async Task DeleteAccountAsync_WhenSuccessful_ReturnsSuccess()
    {
        var repoMock = new Mock<IAccountDeletionRepository>();
        repoMock.Setup(r => r.CheckPasswordAsync(1, "correct_pass")).ReturnsAsync(true);
        repoMock.Setup(r => r.DeleteUserDataAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        
        var loggerMock = new Mock<ILogger<AccountDeletionService>>();
        var sut = new AccountDeletionService(repoMock.Object, loggerMock.Object);

        var result = await sut.DeleteAccountAsync(1, "correct_pass");

        Assert.True(result.IsSuccess);
    }
}