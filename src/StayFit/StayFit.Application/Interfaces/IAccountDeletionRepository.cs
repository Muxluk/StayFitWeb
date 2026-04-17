namespace StayFit.Application.Interfaces;

public interface IAccountDeletionRepository
{
    Task<bool> CheckPasswordAsync(int userId, string password);
    Task<bool> DeleteUserDataAsync(int userId, CancellationToken cancellationToken = default);
}