using Microsoft.Extensions.Logging;
using Moq;
using StayFit.Application.DTOs;
using StayFit.Application.Services;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using Xunit;

namespace StayFit.Tests.Services;

public class UserProfileServiceTests
{
    private readonly Mock<IUserProfileRepository> _mockRepository;
    private readonly Mock<ILogger<UserProfileService>> _mockLogger;
    private readonly UserProfileService _service;

    public UserProfileServiceTests()
    {
        _mockRepository = new Mock<IUserProfileRepository>();
        _mockLogger = new Mock<ILogger<UserProfileService>>();
        _service = new UserProfileService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetProfileAsync_WithExistingProfile_ReturnsProfileDto()
    {
        // Arrange
        int userId = 1;
        var profile = new UserProfile
        {
            Id = 1,
            UserId = userId,
            FullName = "John Doe",
            DateOfBirth = new DateOnly(1990, 1, 15),
            Gender = "Чоловік",
            Weight = 80m,
            Height = 180m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(profile);

        // Act
        var result = await _service.GetProfileAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John Doe", result.FullName);
        Assert.Equal("Чоловік", result.Gender);
        Assert.Equal(80m, result.Weight);
        Assert.Equal(180m, result.Height);
        _mockRepository.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetProfileAsync_WithNonExistentProfile_ReturnsNull()
    {
        // Arrange
        int userId = 999;
        _mockRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((UserProfile)null);

        // Act
        var result = await _service.GetProfileAsync(userId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task CreateProfileAsync_WithValidData_ReturnsTrue()
    {
        // Arrange
        var dto = new CreateUserProfileDto
        {
            UserId = 2,
            FullName = "Jane Doe",
            DateOfBirth = new DateOnly(1995, 6, 20),
            Gender = "Жінка",
            Weight = 65m,
            Height = 170m,
        };

        _mockRepository.Setup(r => r.ExistsForUserAsync(dto.UserId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<UserProfile>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateProfileAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Jane Doe", result.FullName);
        Assert.Equal("Жінка", result.Gender);
        Assert.Equal(65m, result.Weight);
        Assert.Equal(170m, result.Height);
        _mockRepository.Verify(r => r.ExistsForUserAsync(dto.UserId), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<UserProfile>()), Times.Once);
    }

    [Fact]
    public async Task CreateProfileAsync_WithExistingProfile_ThrowsException()
    {
        // Arrange
        var dto = new CreateUserProfileDto
        {
            UserId = 1,
            FullName = "John Doe",
            DateOfBirth = null,
        };

        _mockRepository.Setup(r => r.ExistsForUserAsync(dto.UserId)).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateProfileAsync(dto));
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<UserProfile>()), Times.Never);
    }

    [Fact]
    public async Task CreateProfileAsync_WhenRepositoryThrows_ThrowsException()
    {
        // Arrange
        var dto = new CreateUserProfileDto
        {
            UserId = 3,
            FullName = "Test",
            DateOfBirth = null,
        };

        _mockRepository.Setup(r => r.ExistsForUserAsync(dto.UserId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<UserProfile>())).ThrowsAsync(new Exception("DB Error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.CreateProfileAsync(dto));
    }

    [Fact]
    public async Task UpdateProfileAsync_WithValidData_ReturnsTrue()
    {
        // Arrange
        int userId = 1;
        var updateDto = new UpdateUserProfileDto
        {
            FullName = "Updated Name",
            DateOfBirth = new DateOnly(1992, 3, 10),
            Gender = "Чоловік",
            Weight = 75m,
            Height = 175m
        };

        var profile = new UserProfile
        {
            Id = 1,
            UserId = userId,
            FullName = "Old Name",
            DateOfBirth = new DateOnly(1990, 1, 15),
            Gender = "Чоловік",
            Weight = 80m,
            Height = 180m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(profile);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<UserProfile>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateProfileAsync(userId, updateDto);

        // Assert
        Assert.True(result);
        Assert.Equal("Updated Name", profile.FullName);
        Assert.Equal(75m, profile.Weight);
        Assert.Equal(175m, profile.Height);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<UserProfile>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithNonExistentProfile_ReturnsFalse()
    {
        // Arrange
        int userId = 999;
        var updateDto = new UpdateUserProfileDto
        {
            FullName = "Test",
            DateOfBirth = null,
        };

        _mockRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((UserProfile)null);

        // Act
        var result = await _service.UpdateProfileAsync(userId, updateDto);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<UserProfile>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfileAsync_WhenRepositoryThrows_ReturnsFalse()
    {
        // Arrange
        int userId = 1;
        var updateDto = new UpdateUserProfileDto
        {
            FullName = "Updated",
            DateOfBirth = new DateOnly(1992, 3, 10),
            Gender = "Чоловік",
            Weight = 75m,
            Height = 175m,
        };

        var profile = new UserProfile
        {
            Id = 1,
            UserId = userId,
            FullName = "Old",
            DateOfBirth = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _mockRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(profile);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<UserProfile>())).ThrowsAsync(new Exception("DB Error"));

        // Act
        var result = await _service.UpdateProfileAsync(userId, updateDto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteProfileAsync_WithExistingProfile_ReturnsTrue()
    {
        // Arrange
        int userId = 1;
        var profile = new UserProfile
        {
            Id = 1,
            UserId = userId,
            FullName = "John Doe",
            DateOfBirth = new DateOnly(1990, 1, 15),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _mockRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(profile);
        _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteProfileAsync(userId);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.DeleteAsync(profile.Id), Times.Once);
    }

    [Fact]
    public async Task DeleteProfileAsync_WithNonExistentProfile_ReturnsFalse()
    {
        // Arrange
        int userId = 999;
        _mockRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((UserProfile)null);

        // Act
        var result = await _service.DeleteProfileAsync(userId);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProfileAsync_WhenRepositoryThrows_ReturnsFalse()
    {
        // Arrange
        int userId = 1;
        var profile = new UserProfile
        {
            Id = 1,
            UserId = userId,
            FullName = "John",
            DateOfBirth = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _mockRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(profile);
        _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<int>())).ThrowsAsync(new Exception("DB Error"));

        // Act
        var result = await _service.DeleteProfileAsync(userId);

        // Assert
        Assert.False(result);
    }
}
