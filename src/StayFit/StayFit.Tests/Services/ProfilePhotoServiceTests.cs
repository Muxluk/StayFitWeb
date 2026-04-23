using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StayFit.Application.Options;
using StayFit.Domain.Interfaces;
using StayFit.Web.Services;

namespace StayFit.Tests.Services;

public class ProfilePhotoServiceTests : IDisposable
{
    private readonly Mock<IUserProfileRepository> _userProfileRepositoryMock;
    private readonly Mock<ILogger<ProfilePhotoService>> _loggerMock;
    private readonly Mock<IWebHostEnvironment> _webHostEnvironmentMock;
    private readonly IMemoryCache _memoryCache;
    private readonly ProfilePhotoService _service;
    private readonly string _webRootPath;

    public ProfilePhotoServiceTests()
    {
        _userProfileRepositoryMock = new Mock<IUserProfileRepository>();
        _loggerMock = new Mock<ILogger<ProfilePhotoService>>();
        _webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _webRootPath = Path.Combine(Path.GetTempPath(), $"stayfit-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_webRootPath);

        _webHostEnvironmentMock
            .SetupGet(e => e.WebRootPath)
            .Returns(_webRootPath);

        var options = Options.Create(new ProfilePhotoOptions
        {
            AllowedExtensions = [".jpg", ".png"],
            MaxFileSizeMb = 1,
            CacheLifetimeMinutes = 30,
        });

        _service = new ProfilePhotoService(
            options,
            _webHostEnvironmentMock.Object,
            _loggerMock.Object,
            _userProfileRepositoryMock.Object,
            _memoryCache);
    }

    [Fact]
    public async Task UploadAsync_NullFile_ReturnsFailure()
    {
        var result = await _service.UploadAsync(userId: 10, photo: null);

        Assert.False(result.IsSuccess);
        Assert.Equal("Оберіть файл для завантаження.", result.ErrorMessage);
        _userProfileRepositoryMock.Verify(
            r => r.UpdateProfilePhotoPathAsync(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task UploadAsync_UnsupportedExtension_ReturnsFailure()
    {
        var file = CreateFormFile("avatar.gif", new byte[] { 1, 2, 3 }, "image/gif");

        var result = await _service.UploadAsync(userId: 10, photo: file);

        Assert.False(result.IsSuccess);
        Assert.Contains("Недозволений формат файлу", result.ErrorMessage);
        _userProfileRepositoryMock.Verify(
            r => r.UpdateProfilePhotoPathAsync(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task UploadAsync_TooLargeFile_ReturnsFailure()
    {
        var tooLargeContent = new byte[2 * 1024 * 1024];
        var file = CreateFormFile("avatar.jpg", tooLargeContent, "image/jpeg");

        var result = await _service.UploadAsync(userId: 10, photo: file);

        Assert.False(result.IsSuccess);
        Assert.Contains("Файл занадто великий", result.ErrorMessage);
        _userProfileRepositoryMock.Verify(
            r => r.UpdateProfilePhotoPathAsync(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task UploadAsync_ProfileNotFound_ReturnsFailure()
    {
        var file = CreateFormFile("avatar.png", new byte[] { 1, 2, 3, 4 }, "image/png");
        _userProfileRepositoryMock
            .Setup(r => r.UpdateProfilePhotoPathAsync(10, It.IsAny<string>()))
            .ReturnsAsync(false);

        var result = await _service.UploadAsync(userId: 10, photo: file);

        Assert.False(result.IsSuccess);
        Assert.Equal("Профіль користувача не знайдено.", result.ErrorMessage);
        _userProfileRepositoryMock.Verify(
            r => r.UpdateProfilePhotoPathAsync(10, It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task UploadAsync_ValidFile_SavesAndUpdatesProfileAndClearsCache()
    {
        const int userId = 42;
        _memoryCache.Set("profile-photo:42", "cached-profile");

        var file = CreateFormFile("avatar.PNG", new byte[] { 10, 20, 30, 40, 50 }, "image/png");
        _userProfileRepositoryMock
            .Setup(r => r.UpdateProfilePhotoPathAsync(userId, It.IsAny<string>()))
            .ReturnsAsync(true);

        var result = await _service.UploadAsync(userId, file);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.RelativePath);
        Assert.StartsWith($"/uploads/profile-photos/{userId}/", result.RelativePath);

        _userProfileRepositoryMock.Verify(
            r => r.UpdateProfilePhotoPathAsync(userId, result.RelativePath!),
            Times.Once);

        var physicalPath = Path.Combine(
            _webRootPath,
            result.RelativePath!.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(physicalPath));

        var hasCache = _memoryCache.TryGetValue("profile-photo:42", out _);
        Assert.False(hasCache);
    }

    public void Dispose()
    {
        if (Directory.Exists(_webRootPath))
        {
            Directory.Delete(_webRootPath, recursive: true);
        }

        _memoryCache.Dispose();
    }

    private static IFormFile CreateFormFile(string fileName, byte[] content, string contentType)
    {
        var stream = new MemoryStream(content);
        var formFile = new FormFile(stream, 0, content.Length, "photo", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType,
        };

        return formFile;
    }
}
