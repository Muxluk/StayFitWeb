namespace StayFit.Application.Options;

/// <summary>
/// Налаштування сеансів користувачів
/// </summary>
public class SessionSettings
{
    public const string SectionName = "SessionSettings";

    /// <summary>Максимальна кількість активних сеансів на одного користувача</summary>
    public int MaxActiveSessions { get; set; } = 5;

    /// <summary>Час життя сеансу в годинах</summary>
    public int SessionLifetimeHours { get; set; } = 24;
}
