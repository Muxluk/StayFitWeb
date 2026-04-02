using Microsoft.Extensions.Logging;
using Moq;
using StayFit.Application.Services;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using Xunit;

namespace StayFit.Tests.Services;

public class ProductModerationServiceTests
{
    private readonly Mock<IFoodRepository> _foodRepositoryMock;
    private readonly Mock<ILogger<ProductModerationService>> _loggerMock;
    private readonly ProductModerationService _service;

    public ProductModerationServiceTests()
    {
        _foodRepositoryMock = new Mock<IFoodRepository>();
        _loggerMock = new Mock<ILogger<ProductModerationService>>();
        
        _service = new ProductModerationService(
            _foodRepositoryMock.Object, 
            _loggerMock.Object);
    }

    [Fact]
    public async Task ApproveProductAsync_ShouldReturnSuccess_WhenProductExists()
    {
        // Arrange
        var productId = 1;
        var food = new Food { Id = productId, Name = "Test", IsApproved = false };
        _foodRepositoryMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(food);

        // Act
        var result = await _service.ApproveProductAsync(productId);

        // Assert
        Assert.True(result.IsSuccess); // Замість .Should().BeTrue()
        _foodRepositoryMock.Verify(r => r.UpdateProductStatusAsync(productId, true), Times.Once);
    }

    [Fact]
    public async Task ApproveProductAsync_ShouldReturnFailure_WhenProductDoesNotExist()
    {
        // Arrange
        _foodRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Food?)null);

        // Act
        var result = await _service.ApproveProductAsync(999);

        // Assert
        Assert.True(result.IsFailure);
    }
    // --- ТЕСТИ ДЛЯ ТАСКИ #35 (МОДЕРАЦІЯ) ---

    [Fact]
    public async Task ApproveProductAsync_ShouldCallRepository_WhenProductExists()
    {
        // Тест на пункт "Написати сервіс (Approve)"
        var productId = 1;
        var food = new Food { Id = productId, Name = "Test", IsVerified = false };
        _foodRepositoryMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(food);

        var result = await _service.ApproveProductAsync(productId);

        Assert.True(result.IsSuccess);
        _foodRepositoryMock.Verify(r => r.UpdateProductStatusAsync(productId, true), Times.Once);
    }

    [Fact]
    public async Task RejectProductAsync_ShouldDelete_WhenProductExists()
    {
        // Тест на пункт "Написати сервіс (Reject)"
        var productId = 2;
        var food = new Food { Id = productId, Name = "To Delete" };
        _foodRepositoryMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(food);

        var result = await _service.RejectProductAsync(productId);

        Assert.True(result.IsSuccess);
        _foodRepositoryMock.Verify(r => r.DeleteAsync(productId), Times.Once);
    }

    [Fact]
    public async Task ApproveProductAsync_ShouldReturnFailure_WhenNotFound()
    {
        // Негативний сценарій (пункт про юніт-тести)
        _foodRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Food?)null);

        var result = await _service.ApproveProductAsync(999);

        Assert.True(result.IsFailure); // Перевірка implicit operator та Result.Failure
    }
}