namespace StayFit.Application.Options;

public class ProfilePhotoOptions
{
    public const string SectionName = "ProfilePhoto";

    public string[] AllowedExtensions { get; set; } = [".jpg", ".png"];

    public int MaxFileSizeMb { get; set; } = 5;

    public int CacheLifetimeMinutes { get; set; } = 30;
}
