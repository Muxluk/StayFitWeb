using StayFit.Domain.Entities;

namespace StayFit.Application.Interfaces;

/// <summary>
/// Repository для журналу безпеки акаунта
/// </summary>
public interface ISecurityLogRepository
{
    /// <summary>
    /// Додати запис до журналу безпеки
    /// </summary>
    Task<SecurityLogEntry> AddLogEntryAsync(SecurityLogEntry entry);
    
    /// <summary>
    /// Отримати записи журналу для користувача з пагінацією
    /// </summary>
    Task<(IEnumerable<SecurityLogEntry> entries, int totalCount)> GetUserLogsAsync(
        int userId, 
        int pageNumber, 
        int pageSize);
    
    /// <summary>
    /// Отримати останні записи журналу
    /// </summary>
    Task<IEnumerable<SecurityLogEntry>> GetRecentLogsAsync(int userId, int count);
    
    /// <summary>
    /// Отримати записи журналу за період
    /// </summary>
    Task<IEnumerable<SecurityLogEntry>> GetLogsByDateRangeAsync(
        int userId, 
        DateTime startDate, 
        DateTime endDate);
    
    /// <summary>
    /// Видалити старі записи журналу
    /// </summary>
    Task<int> DeleteOldLogsAsync(int retentionDays);
}
