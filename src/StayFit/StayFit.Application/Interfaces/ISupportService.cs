using StayFit.Application.Common;
using StayFit.Application.DTOs;
using StayFit.Domain.Enums;
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
    /// Отримати деталі конкретного звернення з усіма відповідями.
    /// </summary>
    Task<Result<SupportTicketDto>> GetTicketRepliesAsync(int userId, int ticketId);

    Task<PagedResult<SupportTicketAdminDto>> GetAdminTicketsAsync(SupportStatus? statusFilter, int pageNumber, int pageSize);
    Task<SupportTicketAdminDto?> GetAdminTicketByIdAsync(int id);
    Task<bool> ChangeTicketStatusAsync(int id, SupportStatus newStatus);
    Task<bool> ReplyToTicketAsync(SupportReplyDto replyDto);
}
