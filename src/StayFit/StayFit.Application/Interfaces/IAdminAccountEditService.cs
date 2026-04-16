using StayFit.Application.Common;
using StayFit.Application.DTOs;

namespace StayFit.Application.Interfaces;

/// <summary>
/// Інтерфейс сервісу для редагування даних акаунта користувача адміністратором.
/// </summary>
public interface IAdminAccountEditService
{
    /// <summary>
    /// Валідує дані для редагування акаунта користувача.
    /// </summary>
    /// <param name="request">DTO з даними для оновлення</param>
    /// <param name="userId">ID користувача</param>
    /// <returns>Result з повідомленням про помилку, якщо валідація не пройшла</returns>
    Result ValidateUpdateRequest(AdminUpdateUserRequestDto request, int userId);

    /// <summary>
    /// Перевіряє, чи email унікальний (не використовується іншим користувачем).
    /// </summary>
    /// <param name="email">Email для перевірки</param>
    /// <param name="excludeUserId">ID користувача, якого виключити з перевірки</param>
    /// <param name="repository">Репозиторій для пошуку користувачів</param>
    /// <returns>true якщо email унікальний, false якщо вже використовується</returns>
    Task<bool> IsEmailUniqueAsync(string email, int excludeUserId, IAdminUserRepository repository);
}
