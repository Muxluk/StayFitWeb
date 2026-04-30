namespace StayFit.Domain.Entities;

/// <summary>
/// Звернення користувача до технічної підтримки.
/// </summary>
public class SupportTicket
{
    public int Id { get; set; }

    /// <summary>
    /// ID користувача, який створив звернення.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Тема звернення.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Текст звернення.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Поточний статус звернення (New, InProgress, Closed).
    /// </summary>
    public string Status { get; set; } = "New";

    /// <summary>
    /// Дата створення звернення.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Відповіді в межах звернення.
    /// </summary>
    public ICollection<SupportTicketReply> Replies { get; set; } = new List<SupportTicketReply>();
}
