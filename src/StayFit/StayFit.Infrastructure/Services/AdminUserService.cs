using Microsoft.Extensions.Logging;
using StayFit.Application.Common;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;

namespace StayFit.Infrastructure.Services;

/// <summary>
/// Адмін-сервіс для керування обліковими записами користувачів.
/// </summary>
public sealed class AdminUserService(
    IAdminUserRepository adminUserRepository,
    ILogger<AdminUserService> logger)
    : IAdminUserService
{
    public async Task<Result<IReadOnlyList<AdminUserListItemDto>>> SearchUsersAsync(
        AdminUserSearchRequestDto request,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Admin user search requested. UserId={UserId}, Email={Email}",
            request.UserId,
            request.Email);

        var users = await adminUserRepository.SearchUsersAsync(
            request.UserId,
            request.Email,
            cancellationToken);

        logger.LogInformation("Admin user search completed. Found {Count} users", users.Count);
        return Result<IReadOnlyList<AdminUserListItemDto>>.Success(users);
    }

    public async Task<Result<AdminUserDetailsDto>> GetUserDetailsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Admin requested user details for userId={UserId}", userId);

        var user = await adminUserRepository.GetUserDetailsAsync(userId, cancellationToken);

        if (user is null)
        {
            logger.LogWarning("Admin user details lookup failed: userId={UserId} not found", userId);
            return Result<AdminUserDetailsDto>.Failure("Користувача не знайдено");
        }

        return user;
    }

    public async Task<Result> BlockUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Admin requested block for userId={UserId}", userId);

        var (succeeded, errors) = await adminUserRepository.BlockUserAsync(userId, cancellationToken);
        if (!succeeded)
        {
            logger.LogWarning(
                "Admin block failed for userId={UserId}. Errors: {Errors}",
                userId,
                string.Join("; ", errors));
            return Result.Failure(errors);
        }

        logger.LogInformation("Admin blocked user userId={UserId}", userId);
        return Result.Success();
    }

    public async Task<Result> UnblockUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Admin requested unblock for userId={UserId}", userId);

        var (succeeded, errors) = await adminUserRepository.UnblockUserAsync(userId, cancellationToken);
        if (!succeeded)
        {
            logger.LogWarning(
                "Admin unblock failed for userId={UserId}. Errors: {Errors}",
                userId,
                string.Join("; ", errors));
            return Result.Failure(errors);
        }

        logger.LogInformation("Admin unblocked user userId={UserId}", userId);
        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(
        int userId,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Admin requested password reset for userId={UserId}", userId);

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            return Result.Failure("Новий пароль обов'язковий");
        }

        var (succeeded, errors) = await adminUserRepository.ResetPasswordAsync(userId, newPassword, cancellationToken);
        if (!succeeded)
        {
            logger.LogWarning(
                "Admin password reset failed for userId={UserId}. Errors: {Errors}",
                userId,
                string.Join("; ", errors));
            return Result.Failure(errors);
        }

        logger.LogInformation("Admin password reset succeeded for userId={UserId}", userId);
        return Result.Success();
    }

    public async Task<Result> UpdateUserAsync(
        int userId,
        AdminUpdateUserRequestDto request,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Admin requested account update for userId={UserId}", userId);

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return Result.Failure("Ім'я користувача обов'язкове");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Result.Failure("Email обов'язковий");
        }

        var (succeeded, errors) = await adminUserRepository.UpdateUserAsync(userId, request, cancellationToken);
        if (!succeeded)
        {
            logger.LogWarning(
                "Admin account update failed for userId={UserId}. Errors: {Errors}",
                userId,
                string.Join("; ", errors));
            return Result.Failure(errors);
        }

        logger.LogInformation("Admin account update succeeded for userId={UserId}", userId);
        return Result.Success();
    }

    public async Task<Result> DeleteUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Admin requested delete for userId={UserId}", userId);

        var (succeeded, errors) = await adminUserRepository.DeleteUserAsync(userId, cancellationToken);
        if (!succeeded)
        {
            logger.LogWarning(
                "Admin delete failed for userId={UserId}. Errors: {Errors}",
                userId,
                string.Join("; ", errors));
            return Result.Failure(errors);
        }

        logger.LogInformation("Admin deleted user userId={UserId}", userId);
        return Result.Success();
    }
}
