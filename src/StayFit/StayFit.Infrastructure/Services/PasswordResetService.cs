using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Domain.Exceptions;
using StayFit.Infrastructure.Identity;
using System.Text;

namespace StayFit.Infrastructure.Services;

/// <summary>
/// Сервіс відновлення пароля: генерація токену → відправка email → скидання пароля.
/// </summary>
public sealed class PasswordResetService(
    UserManager<ApplicationUser> userManager,
    IEmailSender emailSender,
    IConfiguration configuration,
    ILogger<PasswordResetService> logger)
    : IPasswordResetService
{
    public async Task<bool> SendPasswordResetTokenAsync(
        ForgotPasswordDto request,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Password reset requested for email {Email}", request.Email);

        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            logger.LogWarning(
                "Password reset: no user found for email {Email}. Silently returning success.",
                request.Email);
            return true;
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        // Беремо baseUrl з конфігурації, наприклад http://localhost:5250
        var baseUrl = configuration["App:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5250";
        var resetLink = $"{baseUrl}/Account/ResetPassword?email={Uri.EscapeDataString(request.Email)}&token={encodedToken}";

        var html = $"""
            <h2>Відновлення пароля — StayFit</h2>
            <p>Ми отримали запит на скидання пароля для вашого акаунту.</p>
            <p>
                <a href="{resetLink}" style="
                    display:inline-block;
                    padding:12px 24px;
                    background:#198754;
                    color:#fff;
                    text-decoration:none;
                    border-radius:6px;
                    font-weight:bold;">
                    Скинути пароль
                </a>
            </p>
            <p>Або скопіюйте посилання вручну:<br/><small>{resetLink}</small></p>
            <p>Якщо ви не надсилали цей запит — просто ігноруйте цей лист.</p>
            <p>Посилання дійсне 24 години.</p>
            """;

        await emailSender.SendAsync(
            request.Email,
            "Відновлення пароля — StayFit",
            html,
            cancellationToken);

        logger.LogInformation(
            "Password reset email sent to {Email}, userId {UserId}",
            request.Email, user.Id);

        return true;
    }

    public async Task<IReadOnlyList<string>> ResetPasswordAsync(
        ResetPasswordDto request,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Password reset attempt for email {Email}", request.Email);

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            logger.LogWarning("Password reset failed: user not found for email {Email}", request.Email);
            return ["Невірний або прострочений запит на скидання пароля."];
        }

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));

        var result = await userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            logger.LogWarning(
                "Password reset failed for email {Email}. Errors: {Errors}",
                request.Email,
                string.Join("; ", errors));
            return errors;
        }

        logger.LogInformation(
            "Password reset succeeded for email {Email}, userId {UserId}",
            request.Email, user.Id);
        return Array.Empty<string>();
    }
}
