namespace StayFit.Domain.Entities;

/// <summary>
/// Повідомлення в межах звернення до технічної підтримки.
/// </summary>
public class SupportTicketReply
{
    public int Id { get; set; }

    /// <summary>
    /// ID звернення, до якого належить відповідь.
    /// </summary>
    public int TicketId { get; set; }

    /// <summary>
    /// Текст відповіді.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Дата створення відповіді.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Чи відповідь залишена адміністратором.
    /// </summary>
    public bool IsAdminReply { get; set; }

    /// <summary>
    /// Навігаційна властивість до звернення.
    /// </summary>
    public SupportTicket? Ticket { get; set; }
}
