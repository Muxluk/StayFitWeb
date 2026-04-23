namespace StayFit.Application.DTOs;

/// <summary>
/// DTO для запису журналу безпеки
/// </summary>
public class SecurityLogDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AdditionalInfo { get; set; }
    
    /// <summary>
    /// Людинозрозуміле представлення User Agent (тип пристрою та браузер)
    /// </summary>
    public string DeviceSummary => GetDeviceSummary();
    
    private string GetDeviceSummary()
    {
        if (string.IsNullOrWhiteSpace(UserAgent))
            return "Unknown Device";
        
        // Простий парсинг User Agent для визначення браузера та ОС
        var userAgent = UserAgent.ToLower();
        
        string browser = "Unknown";
        if (userAgent.Contains("chrome")) browser = "Chrome";
        else if (userAgent.Contains("safari")) browser = "Safari";
        else if (userAgent.Contains("firefox")) browser = "Firefox";
        else if (userAgent.Contains("edge")) browser = "Edge";
        else if (userAgent.Contains("opera")) browser = "Opera";
        
        string os = "Unknown";
        if (userAgent.Contains("windows")) os = "Windows";
        else if (userAgent.Contains("macintosh") || userAgent.Contains("mac os")) os = "macOS";
        else if (userAgent.Contains("linux")) os = "Linux";
        else if (userAgent.Contains("android")) os = "Android";
        else if (userAgent.Contains("iphone") || userAgent.Contains("ipad")) os = "iOS";
        
        return $"{browser} on {os}";
    }
}
