using Microsoft.Extensions.Options;
using StayFit.Application.Options;

namespace StayFit.Web.Services;

public sealed class ProfilePhotoService(
    IOptions<ProfilePhotoOptions> profilePhotoOptions,
    IWebHostEnvironment environment,
    ILogger<ProfilePhotoService> logger) : IProfilePhotoService
{
    private readonly ProfilePhotoOptions _profilePhotoOptions = profilePhotoOptions.Value;

    public async Task<ProfilePhotoUploadResult> UploadAsync(
        int userId,
        IFormFile? photo,
        CancellationToken cancellationToken = default)
    {
        if (photo is null || photo.Length == 0)
        {
            logger.LogWarning("Користувач {UserId} не передав файл фото профілю", userId);
            return ProfilePhotoUploadResult.Failure("Оберіть файл для завантаження.");
        }

        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
        var allowedExtensions = (_profilePhotoOptions.AllowedExtensions ?? Array.Empty<string>())
            .Select(e => e.StartsWith('.') ? e.ToLowerInvariant() : $".{e.ToLowerInvariant()}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!allowedExtensions.Contains(extension))
        {
            logger.LogWarning(
                "Користувач {UserId} намагався завантажити файл з недозволеним форматом {Extension}",
                userId,
                extension);

            return ProfilePhotoUploadResult.Failure(
                $"Недозволений формат файлу. Дозволено: {string.Join(", ", allowedExtensions)}");
        }

        var maxFileSizeBytes = _profilePhotoOptions.MaxFileSizeMb * 1024 * 1024;
        if (photo.Length > maxFileSizeBytes)
        {
            logger.LogWarning(
                "Користувач {UserId} намагався завантажити надто великий файл {FileSizeBytes} байтів",
                userId,
                photo.Length);

            return ProfilePhotoUploadResult.Failure(
                $"Файл занадто великий. Максимальний розмір: {_profilePhotoOptions.MaxFileSizeMb} MB.");
        }

        var uploadsRoot = Path.Combine(environment.WebRootPath, "uploads", "profile-photos", userId.ToString());
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadsRoot, fileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await photo.CopyToAsync(stream, cancellationToken);
        }

        var relativePath = $"/uploads/profile-photos/{userId}/{fileName}";

        logger.LogInformation(
            "Користувач {UserId} завантажив фото профілю. RelativePath={RelativePath}",
            userId,
            relativePath);

        return ProfilePhotoUploadResult.Success(relativePath);
    }
}
