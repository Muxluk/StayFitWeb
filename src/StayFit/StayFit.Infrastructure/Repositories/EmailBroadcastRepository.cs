using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Infrastructure.Data;

namespace StayFit.Infrastructure.Repositories
{
    public class EmailBroadcastRepository : IEmailBroadcastRepository
    {
        private readonly AppDbContext _context;

        public EmailBroadcastRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(EmailBroadcast broadcast)
        {
            _context.EmailBroadcasts.Add(broadcast);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<EmailBroadcast>> GetAllAsync()
        {
            return await _context.EmailBroadcasts.OrderByDescending(e => e.SentAt).ToListAsync();
        }
    }
}
