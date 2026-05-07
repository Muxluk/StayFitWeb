namespace StayFit.Domain.Entities;

/// <summary>
/// Запис журналу безпеки акаунта - історія входів, змін пароля тощо
/// </summary>
public class SecurityLogEntry
{
    public int Id { get; set; }
    
    /// <summary>
    /// ID користувача
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Тип події (Login, PasswordChange, LogOut, SessionTerminated)
    /// </summary>
    public string EventType { get; set; } = string.Empty;
    
    /// <summary>
    /// Опис події
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// IP адреса
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User Agent - інформація про пристрій та браузер
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Час подієї
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Статус подієї (Success, Failure)
    /// </summary>
    public string Status { get; set; } = "Success"; // Success, Failure
    
    /// <summary>
    /// Комунікаційна глава
    /// </summary>
    public string? AdditionalInfo { get; set; }
    
    /// <summary>
    /// Навігаційне властивість на користувача
    /// </summary>
    public User? User { get; set; }
}
