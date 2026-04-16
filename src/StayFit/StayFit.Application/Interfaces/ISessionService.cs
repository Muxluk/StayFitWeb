using StayFit.Domain.Entities;
using StayFit.Domain.Results;

namespace StayFit.Application.Interfaces;

/// <summary>
/// Сервіс для керування активними сеансами користувача
/// </summary>
public interface ISessionService
{
    /// <summary>Отримати список активних сеансів поточного користувача</summary>
    Task<Result<IList<UserSession>>> GetActiveSessionsAsync(int userId);

    /// <summary>Завершити конкретний сеанс</summary>
    Task<Result<bool>> TerminateSessionAsync(int userId, int sessionId);

    /// <summary>Завершити всі сеанси, крім поточного</summary>
    Task<Result<bool>> TerminateAllExceptCurrentAsync(int userId, string currentToken);

    /// <summary>Створити новий сеанс при вході</summary>
    Task<string> CreateSessionAsync(int userId, string? ipAddress, string? userAgent);

    /// <summary>Деактивувати сеанс при виході</summary>
    Task DeactivateSessionAsync(string token);
}
