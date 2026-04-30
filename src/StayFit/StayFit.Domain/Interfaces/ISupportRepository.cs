using StayFit.Domain.Entities;

namespace StayFit.Domain.Interfaces;

/// <summary>
/// Репозиторій для роботи зі зверненнями до технічної підтримки.
/// </summary>
public interface ISupportRepository
{
    /// <summary>
    /// Створити нове звернення.
    /// </summary>
    Task AddTicketAsync(SupportTicket ticket);

    /// <summary>
    /// Отримати всі звернення конкретного користувача.
    /// </summary>
    Task<IEnumerable<SupportTicket>> GetTicketsByUserIdAsync(int userId);

    /// <summary>
    /// Отримати звернення користувача за ID (разом із відповідями).
    /// </summary>
    Task<SupportTicket?> GetTicketByIdAsync(int ticketId, int userId);

    /// <summary>
    /// Отримати відповіді до конкретного звернення користувача.
    /// </summary>
    Task<IEnumerable<SupportTicketReply>> GetRepliesByTicketIdAsync(int ticketId, int userId);
}
