using StayFit.Application.DTOs;

namespace StayFit.Application.Interfaces;

public interface IAdminUserRepository
{
    Task<IReadOnlyList<AdminUserListItemDto>> SearchUsersAsync(
        int? userId,
        string? email,
        CancellationToken cancellationToken = default);

    Task<AdminUserDetailsDto?> GetUserDetailsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, IReadOnlyList<string> Errors)> BlockUserAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, IReadOnlyList<string> Errors)> ResetPasswordAsync(
        int userId,
        string newPassword,
        CancellationToken cancellationToken = default);
}
