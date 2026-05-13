using Microsoft.Extensions.Logging;
using Moq;
using StayFit.Application.Interfaces;
using StayFit.Application.Services;
using StayFit.Domain.Entities;

namespace StayFit.Tests.Services;

public class FoodCategoryServiceTests
{
    private readonly Mock<IFoodCategoryRepository> _repositoryMock = new();
    private readonly Mock<ILogger<FoodCategoryService>> _loggerMock = new();
    private readonly FoodCategoryService _service;

    public FoodCategoryServiceTests()
    {
        _service = new FoodCategoryService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsEmpty_ReturnsFailure_AndDoesNotCallAdd()
    {
        var result = await _service.CreateAsync("   ", "test", null, null);

        Assert.True(result.IsFailure);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<FoodCategoryEntity>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_ReturnsFailure_AndDoesNotCallAdd()
    {
        _repositoryMock
            .Setup(r => r.ExistsByNameAsync("Фрукти"))
            .ReturnsAsync(true);

        var result = await _service.CreateAsync("Фрукти", "Свіжі продукти", null, null);

        Assert.True(result.IsFailure);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<FoodCategoryEntity>()), Times.Never);
    }
    
    [Fact]
    public async Task UpdateAsync_WhenCategoryNotFound_ReturnsFailure()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((FoodCategoryEntity?)null);

        var result = await _service.UpdateAsync(999, "Овочі", "Опис", true, null, null);

        Assert.True(result.IsFailure);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<FoodCategoryEntity>()), Times.Never);
    }
}
