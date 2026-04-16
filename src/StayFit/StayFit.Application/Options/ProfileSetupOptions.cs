namespace StayFit.Application.Options;

public class ProfileSetupOptions
{
    public int MinFullNameLength { get; set; } = 3;
    public int MaxFullNameLength { get; set; } = 100;
    public int MinWeightKg { get; set; } = 20;
    public int MaxWeightKg { get; set; } = 300;
    public int MinHeightCm { get; set; } = 100;
    public int MaxHeightCm { get; set; } = 250;
    public string[] AllowedGenders { get; set; } = Array.Empty<string>();
    public string[] RequiredFields { get; set; } = Array.Empty<string>();
}
