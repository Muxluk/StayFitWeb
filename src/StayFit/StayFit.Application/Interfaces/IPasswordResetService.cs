using StayFit.Application.DTOs;

namespace StayFit.Application.Interfaces;

/// <summary>
/// Сервіс відновлення пароля: генерація токену та скидання пароля.
/// </summary>
public interface IPasswordResetService
{
    /// <summary>
    /// Генерує токен скидання пароля і "надсилає" його (логує в dev-режимі).
    /// Завжди повертає true — щоб не розкривати, чи існує такий email.
    /// </summary>
    Task<bool> SendPasswordResetTokenAsync(
        ForgotPasswordDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Скидає пароль за токеном. Повертає список помилок або порожній список при успіху.
    /// </summary>
    Task<IReadOnlyList<string>> ResetPasswordAsync(
        ResetPasswordDto request,
        CancellationToken cancellationToken = default);
}
