namespace StayFit.Domain.Entities;

/// <summary>
/// Активний сеанс користувача у системі
/// </summary>
public class UserSession
{
    public int Id { get; set; }
    public int UserId { get; set; }

    /// <summary>Унікальний токен сеансу (GUID)</summary>
    public string SessionToken { get; set; } = string.Empty;

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    /// <summary>Час створення (входу)</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Час останньої активності</summary>
    public DateTime? LastActivityAt { get; set; }

    /// <summary>Час закінчення дії сеансу</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Чи є сеанс активним</summary>
    public bool IsActive { get; set; }

    // Навігаційна властивість
    public User? User { get; set; }

    /// <summary>Повертає скорочений опис пристрою на основі UserAgent</summary>
    public string GetDeviceSummary()
    {
        if (string.IsNullOrWhiteSpace(UserAgent))
            return "Невідомий пристрій";

        var ua = UserAgent;
        if (ua.Contains("Mobile") || ua.Contains("Android") || ua.Contains("iPhone"))
            return "Мобільний пристрій";
        if (ua.Contains("Tablet") || ua.Contains("iPad"))
            return "Планшет";

        // Браузер
        if (ua.Contains("Chrome") && !ua.Contains("Edg"))
            return "Chrome (ПК)";
        if (ua.Contains("Firefox"))
            return "Firefox (ПК)";
        if (ua.Contains("Safari") && !ua.Contains("Chrome"))
            return "Safari (ПК)";
        if (ua.Contains("Edg"))
            return "Edge (ПК)";

        return "Браузер (ПК)";
    }
}
