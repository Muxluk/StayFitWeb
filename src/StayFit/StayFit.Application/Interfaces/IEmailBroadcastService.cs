using System.Collections.Generic;
using System.Threading.Tasks;
using StayFit.Domain.Entities;

namespace StayFit.Application.Interfaces
{
    public interface IEmailBroadcastService
    {
        Task SendBroadcastAsync(string adminId, string subject, string body, string audience);
        Task<IEnumerable<EmailBroadcast>> GetHistoryAsync();
    }
}
