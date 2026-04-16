namespace StayFit.Application.Configuration;

/// <summary>
/// Конфігурація для пагінації
/// </summary>
public class PaginationSettings
{
    public const string SectionName = "Pagination";
    
    /// <summary>
    /// Кількість елементів на сторінці за замовчуванням
    /// </summary>
    public int DefaultPageSize { get; set; } = 10;
    
    /// <summary>
    /// Максимальна кількість елементів на сторінці
    /// </summary>
    public int MaxPageSize { get; set; } = 100;
}
