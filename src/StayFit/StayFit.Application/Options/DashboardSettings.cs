namespace StayFit.Application.Options;

/// <summary>
/// Налаштування дашборду
/// </summary>
public class DashboardSettings
{
    /// <summary>
    /// Кількість останніх записів щоденника для блоку на дашборді
    /// </summary>
    public int RecentDiaryEntriesCount { get; set; } = 5;
}