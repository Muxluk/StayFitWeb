using Microsoft.EntityFrameworkCore;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Data;

namespace StayFit.Infrastructure.Repositories;

public class SupportMessageRepository : ISupportMessageRepository
{
    private readonly AppDbContext _context;

    public SupportMessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SupportMessage>> GetUnnotifiedAsync()
    {
        return await _context.SupportMessages
            .Where(m => !m.IsAdminNotified)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(SupportMessage message)
    {
        _context.SupportMessages.Add(message);
        await _context.SaveChangesAsync();
    }

    public async Task MarkAsNotifiedAsync(IEnumerable<int> ids)
    {
        var idSet = ids.ToHashSet();
        var messages = await _context.SupportMessages
            .Where(m => idSet.Contains(m.Id))
            .ToListAsync();

        foreach (var message in messages)
        {
            message.IsAdminNotified = true;
        }

        await _context.SaveChangesAsync();
    }
}
