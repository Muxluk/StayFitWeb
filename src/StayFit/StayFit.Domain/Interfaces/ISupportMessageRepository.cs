using StayFit.Domain.Entities;

namespace StayFit.Domain.Interfaces;

public interface ISupportMessageRepository
{
    Task<IEnumerable<SupportMessage>> GetUnnotifiedAsync();
    Task AddAsync(SupportMessage message);
    Task MarkAsNotifiedAsync(IEnumerable<int> ids);
}
