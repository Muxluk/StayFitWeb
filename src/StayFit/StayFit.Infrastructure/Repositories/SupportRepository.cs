using Microsoft.EntityFrameworkCore;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Data;

namespace StayFit.Infrastructure.Repositories;

/// <summary>
/// Репозиторій для роботи зі зверненнями до технічної підтримки.
/// </summary>
public class SupportRepository : ISupportRepository
{
    private readonly AppDbContext _context;

    public SupportRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Створити нове звернення.
    /// </summary>
    public async Task AddTicketAsync(SupportTicket ticket)
    {
        await _context.Set<SupportTicket>().AddAsync(ticket);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Отримати всі звернення конкретного користувача.
    /// </summary>
    public async Task<IEnumerable<SupportTicket>> GetTicketsByUserIdAsync(int userId)
    {
        return await _context.Set<SupportTicket>()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Отримати звернення користувача за ID (разом із відповідями).
    /// </summary>
    public async Task<SupportTicket?> GetTicketByIdAsync(int ticketId, int userId)
    {
        return await _context.Set<SupportTicket>()
            .Include(t => t.Replies)
            .FirstOrDefaultAsync(t => t.Id == ticketId && t.UserId == userId);
    }

    /// <summary>
    /// Отримати відповіді до конкретного звернення користувача.
    /// </summary>
    public async Task<IEnumerable<SupportTicketReply>> GetRepliesByTicketIdAsync(int ticketId, int userId)
    {
        var hasAccess = await _context.Set<SupportTicket>()
            .AnyAsync(t => t.Id == ticketId && t.UserId == userId);

        if (!hasAccess)
        {
            return Array.Empty<SupportTicketReply>();
        }

        return await _context.Set<SupportTicketReply>()
            .Where(r => r.TicketId == ticketId)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    }
}
