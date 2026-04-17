using StayFit.Domain.Entities;

namespace StayFit.Domain.Interfaces;

/// <summary>
/// Репозиторій для керування сеансами користувачів
/// </summary>
public interface ISessionRepository
{
    /// <summary>Створити новий сеанс</summary>
    Task<UserSession> CreateAsync(UserSession session);

    /// <summary>Отримати всі активні сеанси користувача</summary>
    Task<IList<UserSession>> GetActiveByUserIdAsync(int userId);

    /// <summary>Знайти сеанс за токеном</summary>
    Task<UserSession?> GetByTokenAsync(string token);

    /// <summary>Перевірити чи токен є дійсним</summary>
    Task<bool> IsValidAsync(string token);

    /// <summary>Деактивувати конкретний сеанс</summary>
    Task<bool> DeactivateAsync(int sessionId);

    /// <summary>Деактивувати всі сеанси, крім поточного</summary>
    Task<int> DeactivateAllExceptAsync(int userId, string currentToken);

    /// <summary>Деактивувати всі сеанси користувача</summary>
    Task<int> DeactivateAllAsync(int userId);

    /// <summary>Оновити час останньої активності</summary>
    Task UpdateLastActivityAsync(string token);

    /// <summary>Видалити прострочені сеанси</summary>
    Task<int> DeleteExpiredAsync();
}
