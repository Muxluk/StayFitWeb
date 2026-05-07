namespace StayFit.Domain.Entities;

/// <summary>
/// Профіль користувача з персональною інформацією
/// </summary>
public class UserProfile
{
    public int Id { get; set; }

    /// <summary>
    /// ID користувача (з таблиці AspNetUsers)
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Повне ім'я користувача
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// День народження
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// Стать (Чоловік, Жінка, Інше)
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Вага користувача (кг)
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Зріст користувача (см)
    /// </summary>
    public decimal? Height { get; set; }

    /// <summary>
    /// Відносний шлях до фото профілю у web root
    /// </summary>
    public string? ProfilePhotoPath { get; set; }

    /// <summary>
    /// Рівень фізичної активності (Low, Medium, High тощо)
    /// </summary>
    public string? ActivityLevel { get; set; }

    /// <summary>
    /// Дата створення профілю
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата останнього оновлення
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
