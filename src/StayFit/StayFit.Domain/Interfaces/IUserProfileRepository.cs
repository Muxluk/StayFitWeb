using StayFit.Domain.Entities;

namespace StayFit.Domain.Interfaces;

/// <summary>
/// Репозиторій для управління профілями користувачів
/// </summary>
public interface IUserProfileRepository : IRepository<UserProfile>
{
    /// <summary>
    /// Отримати профіль за ID користувача
    /// </summary>
    Task<UserProfile?> GetByUserIdAsync(int userId);

    /// <summary>
    /// Перевірити, чи існує профіль для користувача
    /// </summary>
    Task<bool> ExistsForUserAsync(int userId);

    /// <summary>
    /// Оновити шлях до фото профілю користувача
    /// </summary>
    Task<bool> UpdateProfilePhotoPathAsync(int userId, string profilePhotoPath);
}
