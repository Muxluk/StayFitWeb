namespace StayFit.Domain.Entities;

/// <summary>
/// Сесія користувача
/// </summary>
public class UserSession
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? SessionToken { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public bool IsActive { get; set; }
}
