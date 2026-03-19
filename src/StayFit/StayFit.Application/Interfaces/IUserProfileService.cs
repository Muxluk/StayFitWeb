using StayFit.Application.DTOs;

namespace StayFit.Application.Interfaces;

/// <summary>
/// Сервіс для управління профілями користувачів
/// </summary>
public interface IUserProfileService
{
    /// <summary>
    /// Отримати профіль користувача
    /// </summary>
    Task<UserProfileDto?> GetProfileAsync(int userId);

    /// <summary>
    /// Оновити профіль користувача
    /// </summary>
    Task<bool> UpdateProfileAsync(int userId, UpdateUserProfileDto dto);

    /// <summary>
    /// Створити профіль для нового користувача
    /// </summary>
    Task<UserProfileDto> CreateProfileAsync(CreateUserProfileDto dto);

    /// <summary>
    /// Видалити профіль користувача
    /// </summary>
    Task<bool> DeleteProfileAsync(int userId);
}
