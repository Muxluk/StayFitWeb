namespace StayFit.Application.Options;

/// <summary>
/// Налаштування для системи сповіщень
/// </summary>
public class NotificationSettings
{
    /// <summary>
    /// Назва секції у конфігурації
    /// </summary>
    public const string SectionName = "Notifications";

    /// <summary>
    /// Поріг перевищення денної норми калорій для генерації сповіщення (у відсотках)
    /// Наприклад: 100 означає, що сповіщення буде генеруватися коли калорії = норма * 1.0 (100%)
    /// 110 означає, що сповіщення буде при 110% від норми
    /// </summary>
    public decimal CalorieThresholdPercent { get; set; } = 100;
}
