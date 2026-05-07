namespace StayFit.Application.Configuration;

/// <summary>
/// Конфігурація для журналу безпеки
/// </summary>
public class SecurityLogSettings
{
    public const string SectionName = "SecurityLog";
    
    /// <summary>
    /// Кількість записів на сторінці за замовчуванням
    /// </summary>
    public int DefaultPageSize { get; set; } = 10;
    
    /// <summary>
    /// Максимальна кількість записів на сторінці
    /// </summary>
    public int MaxPageSize { get; set; } = 50;
    
    /// <summary>
    /// Кількість днів для зберігання записів журналу
    /// </summary>
    public int RetentionDays { get; set; } = 90;
}
