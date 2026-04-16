using StayFit.Domain.Results;

namespace StayFit.Application.Interfaces;

/// <summary>
/// Сервіс для зміни пароля користувача
/// </summary>
public interface IPasswordChangeService
{
    /// <summary>
    /// Змінити пароль користувача
    /// </summary>
    /// <param name="userId">ID користувача</param>
    /// <param name="currentPassword">Поточний пароль</param>
    /// <param name="newPassword">Новий пароль</param>
    /// <param name="confirmPassword">Підтвердження нового пароля</param>
    /// <returns>Результат операції з можливими помилками</returns>
    Task<Result<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword, string confirmPassword);
}
