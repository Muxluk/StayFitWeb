using StayFit.Application.Common;
using StayFit.Application.DTOs;

namespace StayFit.Application.Interfaces;

public interface IAdminUserService
{
    Task<Result<IReadOnlyList<AdminUserListItemDto>>> SearchUsersAsync(
        AdminUserSearchRequestDto request,
        CancellationToken cancellationToken = default);

    Task<Result<AdminUserDetailsDto>> GetUserDetailsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<Result> BlockUserAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<Result> UnblockUserAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<Result> ResetPasswordAsync(
        int userId,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task<Result> UpdateUserAsync(
        int userId,
        AdminUpdateUserRequestDto request,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteUserAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
