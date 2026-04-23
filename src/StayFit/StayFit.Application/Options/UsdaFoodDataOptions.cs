namespace StayFit.Application.Options;

public class UsdaFoodDataOptions
{
    public string ApiUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = "DEMO_KEY";
    public int CacheLifetimeMinutes { get; set; } = 60;
}
