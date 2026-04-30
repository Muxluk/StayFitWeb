using StayFit.Application.Common;
using StayFit.Application.DTOs;

namespace StayFit.Application.Interfaces;

/// <summary>
/// Сервіс для роботи зі зверненнями користувача до технічної підтримки.
/// </summary>
public interface ISupportService
{
    /// <summary>
    /// Створити нове звернення від користувача.
    /// </summary>
    Task<Result<int>> CreateTicketAsync(int userId, CreateSupportTicketRequestDto request);

    /// <summary>
    /// Отримати список звернень поточного користувача.
    /// </summary>
    Task<Result<IEnumerable<SupportTicketDto>>> GetMyTicketsAsync(int userId);

    /// <summary>
    /// Отримати відповіді в межах конкретного звернення користувача.
    /// </summary>
    Task<Result<IEnumerable<SupportTicketReplyDto>>> GetTicketRepliesAsync(int userId, int ticketId);
}
