using StayFit.Domain.Entities;

namespace StayFit.Application.Interfaces;

/// <summary>
/// Repository для операцій безпеки акаунта
/// </summary>
public interface IAccountSecurityRepository
{
    /// <summary>
    /// Змінити пароль користувача
    /// </summary>
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

    /// <summary>
    /// Отримати активні сесії користувача
    /// </summary>
    Task<IEnumerable<UserSession>> GetActiveSessionsAsync(int userId);

    /// <summary>
    /// Вихід з конкретної сесії
    /// </summary>
    Task<bool> InvalidateSessionAsync(int sessionId);

    /// <summary>
    /// Вихід з усіх сесій
    /// </summary>
    Task<bool> InvalidateAllSessionsAsync(int userId);

    /// <summary>
    /// Видалити акаунт
    /// </summary>
    Task<bool> DeleteAccountAsync(int userId);

    /// <summary>
    /// Перевірити чи користувач існує
    /// </summary>
    Task<bool> UserExistsAsync(int userId);
}
