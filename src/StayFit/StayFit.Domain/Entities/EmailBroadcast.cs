namespace StayFit.Domain.Entities;

/// <summary>
/// Представляє запис про розсилку email-повідомлень адміністратором.
/// </summary>
public class EmailBroadcast
{
    public int Id { get; set; }
    
    /// <summary>
    /// ID адміністратора, який відправив розсилку.
    /// </summary>
    public int AdminUserId { get; set; }
    public User? AdminUser { get; set; }
    
    /// <summary>
    /// Тема листа.
    /// </summary>
    public required string Subject { get; set; }
    
    /// <summary>
    /// HTML-текст листа.
    /// </summary>
    public required string HtmlBody { get; set; }
    
    /// <summary>
    /// Кількість отримувачів.
    /// </summary>
    public int RecipientCount { get; set; }
    
    /// <summary>
    /// Дата відправлення.
    /// </summary>
    public DateTime SentAt { get; set; }
    
    /// <summary>
    /// Статус розсилки (Pending, Sent, Failed).
    /// </summary>
    public string Status { get; set; } = "Pending";
}
