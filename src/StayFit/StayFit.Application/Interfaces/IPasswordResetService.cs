using StayFit.Application.Common;
using StayFit.Application.DTOs;

namespace StayFit.Application.Interfaces;

/// <summary>
/// Сервіс відновлення пароля: генерація токену та скидання пароля.
/// </summary>
public interface IPasswordResetService
{
    /// <summary>
    /// Генерує токен скидання пароля і "надсилає" його (логує в dev-режимі).
    /// Повертає Result.Success навіть якщо користувача не знайдено (захист від email enumeration).
    /// </summary>
    Task<Result> SendPasswordResetTokenAsync(
        ForgotPasswordDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Скидає пароль за токеном і повертає Result з бізнес-помилками у Errors.
    /// </summary>
    Task<Result> ResetPasswordAsync(
        ResetPasswordDto request,
        CancellationToken cancellationToken = default);
}
