using System.Collections.Generic;
using System.Threading.Tasks;
using StayFit.Domain.Entities;

namespace StayFit.Application.Interfaces
{
    public interface IEmailBroadcastRepository
    {
        Task AddAsync(EmailBroadcast broadcast);
        Task<IEnumerable<EmailBroadcast>> GetAllAsync();
    }
}
