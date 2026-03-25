using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StayFit.Application.Common;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Infrastructure.Identity;

namespace StayFit.Infrastructure.Services;

/// <summary>
/// Реалізація сервісу автентифікації з логуванням.
/// Використовує тільки UserManager (без SignInManager) щоб не залежати від HTTP-контексту.
/// SignInAsync викликається в контролері після успішної перевірки.
/// </summary>
public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    ILogger<AuthService> logger)
    : IAuthService
{
    public async Task<Result<string>> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Login attempt for email {Email}", request.Email);

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            logger.LogWarning("Login failed: user not found for email {Email}", request.Email);
            return Result<string>.Failure("Невірний email або пароль.");
        }

        // Перевіряємо lockout вручну
        if (await userManager.IsLockedOutAsync(user))
        {
            logger.LogWarning("Login failed: account locked out for email {Email}", request.Email);
            return Result<string>.Failure("Акаунт заблоковано. Спробуйте пізніше.");
        }

        var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            // Збільшуємо лічильник невдалих спроб
            await userManager.AccessFailedAsync(user);

            logger.LogWarning("Login failed: invalid password for email {Email}", request.Email);
            return Result<string>.Failure("Невірний email або пароль.");
        }

        // Скидаємо лічильник невдалих спроб після успішного входу
        await userManager.ResetAccessFailedCountAsync(user);

        logger.LogInformation(
            "Login credentials verified for email {Email}, userId {UserId}",
            request.Email, user.Id);

        return Result<string>.Success(user.UserName!);
    }
}
