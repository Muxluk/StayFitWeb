using StayFit.Application.Common;

namespace StayFit.Application.Interfaces;

public interface IAccountDeletionService
{
    Task<Result> DeleteAccountAsync(int userId, string password, CancellationToken cancellationToken = default);
}