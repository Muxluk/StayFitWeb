using StayFit.Application.DTOs;
using StayFit.Domain.Results;

namespace StayFit.Application.Interfaces;

/// <summary>
/// Сервіс для керування журналом безпеки акаунта
/// </summary>
public interface ISecurityLogService
{
    /// <summary>
    /// Залогувати вхід користувача
    /// </summary>
    Task LogLoginAsync(
        int userId,
        string? ipAddress = null,
        string? userAgent = null,
        bool isSuccessful = true,
        string? failureReason = null);
    
    /// <summary>
    /// Залогувати зміну пароля
    /// </summary>
    Task LogPasswordChangeAsync(
        int userId,
        string? ipAddress = null,
        string? userAgent = null,
        bool isSuccessful = true,
        string? failureReason = null);
    
    /// <summary>
    /// Залогувати вихід користувача
    /// </summary>
    Task LogLogoutAsync(int userId, string? ipAddress = null);
    
    /// <summary>
    /// Залогувати завершення сеансу
    /// </summary>
    Task LogSessionTerminatedAsync(int userId, string? ipAddress = null);
    
    /// <summary>
    /// Отримати записи журналу користувача з пагінацією
    /// </summary>
    Task<Result<PagedResult<SecurityLogDto>>> GetUserSecurityLogsAsync(int userId, int pageNumber);
    
    /// <summary>
    /// Отримати останні записи журналу
    /// </summary>
    Task<Result<IEnumerable<SecurityLogDto>>> GetRecentLogsAsync(int userId, int count);
    
    /// <summary>
    /// Видалити старі записи журналу
    /// </summary>
    Task<Result<int>> CleanupOldLogsAsync();
}
