using StayFit.Application.DTOs;

namespace StayFit.Application.Interfaces;

/// <summary>
/// Сервіс автентифікації: вхід та вихід користувача.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Виконує вхід користувача за email та паролем.
    /// </summary>
    Task<LoginResultDto> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default);
}
