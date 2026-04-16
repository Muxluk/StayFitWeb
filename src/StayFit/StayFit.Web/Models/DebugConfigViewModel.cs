namespace StayFit.Web.Models;

public class DebugConfigViewModel
{
    public string EnvironmentName { get; set; } = string.Empty;
    public int RecentDiaryEntriesCount { get; set; }
    public string ConfiguredBaseUrl { get; set; } = string.Empty;
    public string RuntimeUrl { get; set; } = string.Empty;
    public bool HasDefaultConnectionString { get; set; }
}