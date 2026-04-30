using Microsoft.Extensions.Logging;
using StayFit.Application.Common;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Application.Services;

/// <summary>
/// Сервіс звернень до технічної підтримки.
/// </summary>
public class SupportService : ISupportService
{
    private readonly ISupportRepository _supportRepository;
    private readonly ILogger<SupportService> _logger;

    public SupportService(
        ISupportRepository supportRepository,
        ILogger<SupportService> logger)
    {
        _supportRepository = supportRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<int>> CreateTicketAsync(int userId, CreateSupportTicketRequestDto request)
    {
        try
        {
            if (userId <= 0)
            {
                _logger.LogWarning("Спроба створення звернення з некоректним userId: {UserId}", userId);
                return Result<int>.Failure("Некоректний користувач");
            }

            if (string.IsNullOrWhiteSpace(request.Subject))
            {
                _logger.LogWarning("Спроба створення звернення без теми. UserId: {UserId}", userId);
                return Result<int>.Failure("Тема звернення обов'язкова");
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                _logger.LogWarning("Спроба створення звернення без тексту. UserId: {UserId}", userId);
                return Result<int>.Failure("Текст звернення обов'язковий");
            }

            var ticket = new SupportTicket
            {
                UserId = userId,
                Subject = request.Subject.Trim(),
                Message = request.Message.Trim(),
                Status = "New",
                CreatedAt = DateTime.UtcNow
            };

            await _supportRepository.AddTicketAsync(ticket);

            _logger.LogInformation(
                "Створено звернення до підтримки {TicketId} для користувача {UserId}",
                ticket.Id,
                userId);

            return Result<int>.Success(ticket.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при створенні звернення для користувача {UserId}", userId);
            return Result<int>.Failure("Помилка при створенні звернення");
        }
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<SupportTicketDto>>> GetMyTicketsAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Отримання списку звернень користувача {UserId}", userId);

            var tickets = await _supportRepository.GetTicketsByUserIdAsync(userId);
            var result = tickets.Select(t => new SupportTicketDto
            {
                Id = t.Id,
                Subject = t.Subject,
                Message = t.Message,
                Status = t.Status,
                CreatedAt = t.CreatedAt
            }).ToList();

            _logger.LogInformation(
                "Отримано {Count} звернень користувача {UserId}",
                result.Count,
                userId);

            return Result<IEnumerable<SupportTicketDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні звернень користувача {UserId}", userId);
            return Result<IEnumerable<SupportTicketDto>>.Failure("Помилка при отриманні звернень");
        }
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<SupportTicketReplyDto>>> GetTicketRepliesAsync(int userId, int ticketId)
    {
        try
        {
            _logger.LogInformation(
                "Отримання відповідей до звернення {TicketId} для користувача {UserId}",
                ticketId,
                userId);

            var replies = await _supportRepository.GetRepliesByTicketIdAsync(ticketId, userId);
            var result = replies.Select(r => new SupportTicketReplyDto
            {
                Id = r.Id,
                TicketId = r.TicketId,
                Message = r.Message,
                CreatedAt = r.CreatedAt,
                IsAdminReply = r.IsAdminReply
            }).ToList();

            _logger.LogInformation(
                "Отримано {Count} відповідей до звернення {TicketId} для користувача {UserId}",
                result.Count,
                ticketId,
                userId);

            return Result<IEnumerable<SupportTicketReplyDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Помилка при отриманні відповідей до звернення {TicketId} для користувача {UserId}",
                ticketId,
                userId);
            return Result<IEnumerable<SupportTicketReplyDto>>.Failure("Помилка при отриманні відповідей");
        }
    }
}
