namespace StayFit.Domain.Entities;

/// <summary>
/// Сутність для зберігання сповіщень користувачів
/// </summary>
public class Notification
{
    /// <summary>
    /// ID сповіщення
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID користувача, якому призначено сповіщення
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Заголовок сповіщення
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Текст сповіщення
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Тип сповіщення (CalorieThreshold, etc.)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Чи було прочитано сповіщення
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// Дата створення сповіщення
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата прочитання (null якщо не прочитано)
    /// </summary>
    public DateTime? ReadAt { get; set; }
}
