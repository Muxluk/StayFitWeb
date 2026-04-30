using Microsoft.EntityFrameworkCore;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Data;
using StayFit.Domain.Enums;

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

    public async Task<IEnumerable<SupportTicket>> GetAllTicketsAsync(SupportStatus? statusFilter, int skip, int take)
    {
        var query = _context.Set<SupportTicket>().Include(t => t.User).AsQueryable();

        if (statusFilter.HasValue)
        {
            var statusString = statusFilter.Value.ToString();
            query = query.Where(t => t.Status == statusString);
        }

        return await query.OrderByDescending(t => t.CreatedAt)
                          .Skip(skip)
                          .Take(take)
                          .ToListAsync();
    }

    public async Task<int> GetTicketsCountAsync(SupportStatus? statusFilter)
    {
        var query = _context.Set<SupportTicket>().AsQueryable();
        
        if (statusFilter.HasValue)
        {
            var statusString = statusFilter.Value.ToString();
            query = query.Where(t => t.Status == statusString);
        }

        return await query.CountAsync();
    }

    public async Task<SupportTicket?> GetTicketWithRepliesByIdAsync(int ticketId)
    {
        return await _context.Set<SupportTicket>()
            .Include(t => t.User)
            .Include(t => t.Replies) // Припускаємо, що зв'язок називається Replies
            .FirstOrDefaultAsync(t => t.Id == ticketId);
    }

    public async Task UpdateTicketAsync(SupportTicket ticket)
    {
        _context.Set<SupportTicket>().Update(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task AddReplyAsync(SupportTicketReply reply)
    {
        await _context.Set<SupportTicketReply>().AddAsync(reply);
        await _context.SaveChangesAsync();
    }
}
