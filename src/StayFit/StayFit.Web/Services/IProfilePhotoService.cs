namespace StayFit.Web.Services;

public interface IProfilePhotoService
{
    Task<ProfilePhotoUploadResult> UploadAsync(int userId, IFormFile? photo, CancellationToken cancellationToken = default);
}

public sealed class ProfilePhotoUploadResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public string? RelativePath { get; init; }

    public static ProfilePhotoUploadResult Success(string relativePath) =>
        new() { IsSuccess = true, RelativePath = relativePath };

    public static ProfilePhotoUploadResult Failure(string errorMessage) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}
