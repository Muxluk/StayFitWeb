using Microsoft.Extensions.Logging;
using StayFit.Application.Common;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Domain.Enums;

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
                Status = SupportStatus.New.ToString(), // Зберігаємо як рядок
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
    public async Task<Result<SupportTicketDto>> GetTicketRepliesAsync(int userId, int ticketId)
    {
        try
        {
            _logger.LogInformation(
                "Отримання деталей звернення {TicketId} для користувача {UserId}",
                ticketId,
                userId);

            // Get ticket first
            var ticket = await _supportRepository.GetTicketByIdAsync(ticketId, userId);
            if (ticket == null)
            {
                _logger.LogWarning(
                    "Звернення {TicketId} не знайдено для користувача {UserId}",
                    ticketId,
                    userId);
                return Result<SupportTicketDto>.Failure("Звернення не знайдено");
            }

            // Get replies
            var replies = await _supportRepository.GetRepliesByTicketIdAsync(ticketId, userId);
            var replyDtos = replies.Select(r => new SupportTicketReplyDto
            {
                Id = r.Id,
                TicketId = r.TicketId,
                Message = r.Message,
                CreatedAt = r.CreatedAt,
                IsAdminReply = r.IsAdminReply
            }).ToList();

            var ticketDto = new SupportTicketDto
            {
                Id = ticket.Id,
                Subject = ticket.Subject,
                Message = ticket.Message,
                Status = ticket.Status,
                CreatedAt = ticket.CreatedAt,
                Replies = replyDtos
            };

            _logger.LogInformation(
                "Отримано {Count} відповідей до звернення {TicketId} для користувача {UserId}",
                replyDtos.Count,
                ticketId,
                userId);

            return Result<SupportTicketDto>.Success(ticketDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Помилка при отриманні деталей звернення {TicketId} для користувача {UserId}",
                ticketId,
                userId);
            return Result<SupportTicketDto>.Failure("Помилка при отриманні деталей звернення");
        }
    }

    // --- АДМІНСЬКІ МЕТОДИ ---

    public async Task<PagedResult<SupportTicketAdminDto>> GetAdminTicketsAsync(SupportStatus? statusFilter, int pageNumber, int pageSize)
    {
        var skip = (pageNumber - 1) * pageSize;
        var totalCount = await _supportRepository.GetTicketsCountAsync(statusFilter);
        var tickets = await _supportRepository.GetAllTicketsAsync(statusFilter, skip, pageSize);

        var dtos = tickets.Select(t => new SupportTicketAdminDto
        {
            Id = t.Id,
            UserEmail = t.User?.Email ?? "Невідомий користувач",
            Subject = t.Subject, 
            // Безпечне перетворення рядка з БД в Enum для DTO
            Status = Enum.TryParse<SupportStatus>(t.Status, true, out var status) ? status : SupportStatus.New,
            CreatedAt = t.CreatedAt
        }).ToList();

        return new PagedResult<SupportTicketAdminDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<SupportTicketAdminDto?> GetAdminTicketByIdAsync(int id)
    {
        var t = await _supportRepository.GetTicketWithRepliesByIdAsync(id);
        if (t == null) return null;

        return new SupportTicketAdminDto
        {
            Id = t.Id,
            UserEmail = t.User?.Email ?? "Невідомий користувач",
            Subject = t.Subject,
            Message = t.Message,
            Status = Enum.TryParse<SupportStatus>(t.Status, true, out var status) ? status : SupportStatus.New,
            CreatedAt = t.CreatedAt,
            Replies = t.Replies?.Select(r => new SupportTicketReplyAdminDto
            {
                Message = r.Message,
                IsAdminReply = r.IsAdminReply, 
                CreatedAt = r.CreatedAt
            }).OrderBy(r => r.CreatedAt).ToList() ?? new List<SupportTicketReplyAdminDto>()
        };
    }

    public async Task<bool> ChangeTicketStatusAsync(int id, SupportStatus newStatus)
    {
        try
        {
            var ticket = await _supportRepository.GetTicketWithRepliesByIdAsync(id);
            if (ticket == null) return false;

            ticket.Status = newStatus.ToString(); // Зберігаємо в БД як рядок
            await _supportRepository.UpdateTicketAsync(ticket);
            
            _logger.LogInformation("Admin changed status for ticket {TicketId} to {Status}", id, newStatus);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing status for ticket {TicketId}", id);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<Result<int>> AddUserReplyAsync(int userId, int ticketId, string message)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return Result<int>.Failure("Текст коментаря обов'язковий");
            }

            var ticket = await _supportRepository.GetTicketByIdAsync(ticketId, userId);
            if (ticket == null)
            {
                _logger.LogWarning(
                    "Користувач {UserId} спробував додати коментар до неіснуючого або чужого звернення {TicketId}",
                    userId, ticketId);
                return Result<int>.Failure("Звернення не знайдено");
            }

            if (ticket.Status == SupportStatus.Closed.ToString())
            {
                return Result<int>.Failure("Неможливо додати коментар до закритого звернення");
            }

            var reply = new SupportTicketReply
            {
                TicketId = ticketId,
                Message = message.Trim(),
                IsAdminReply = false,
                CreatedAt = DateTime.UtcNow
            };

            await _supportRepository.AddReplyAsync(reply);

            _logger.LogInformation(
                "Користувач {UserId} додав коментар до звернення {TicketId}",
                userId, ticketId);

            return Result<int>.Success(reply.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при додаванні коментаря до звернення {TicketId}", ticketId);
            return Result<int>.Failure("Помилка при додаванні коментаря");
        }
    }

    public async Task<bool> ReplyToTicketAsync(SupportReplyDto replyDto)
    {
        try
        {
            var ticket = await _supportRepository.GetTicketWithRepliesByIdAsync(replyDto.TicketId);
            if (ticket == null) return false;

            var reply = new SupportTicketReply
            {
                TicketId = replyDto.TicketId,
                Message = replyDto.ReplyMessage,
                IsAdminReply = true,
                CreatedAt = DateTime.UtcNow
            };
            
            await _supportRepository.AddReplyAsync(reply);

            // Закриваємо тікет після відповіді
            ticket.Status = SupportStatus.Closed.ToString();
            await _supportRepository.UpdateTicketAsync(ticket);

            _logger.LogInformation("Admin replied to ticket {TicketId}", replyDto.TicketId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error replying to ticket {TicketId}", replyDto.TicketId);
            return false;
        }
    }
}