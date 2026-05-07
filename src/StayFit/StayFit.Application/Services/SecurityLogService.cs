using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StayFit.Application.Configuration;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Domain.Results;

namespace StayFit.Application.Services;

/// <summary>
/// Сервіс для керування журналом безпеки акаунта
/// </summary>
public class SecurityLogService : ISecurityLogService
{
    private readonly ISecurityLogRepository _repository;
    private readonly ILogger<SecurityLogService> _logger;
    private readonly SecurityLogSettings _settings;

    public SecurityLogService(
        ISecurityLogRepository repository,
        ILogger<SecurityLogService> logger,
        IOptions<SecurityLogSettings> options)
    {
        _repository = repository;
        _logger = logger;
        _settings = options.Value;
    }

    public async Task LogLoginAsync(
        int userId,
        string? ipAddress = null,
        string? userAgent = null,
        bool isSuccessful = true,
        string? failureReason = null)
    {
        try
        {
            var entry = new SecurityLogEntry
            {
                UserId = userId,
                EventType = "Login",
                Description = isSuccessful ? "Успішний вхід в систему" : "Невдалий вхід в систему",
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Status = isSuccessful ? "Success" : "Failure",
                AdditionalInfo = failureReason,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddLogEntryAsync(entry);
            
            if (isSuccessful)
                _logger.LogInformation("Залоговано вхід користувача {UserId} з IP {IpAddress}", userId, ipAddress);
            else
                _logger.LogWarning("Невдалий вхід користувача {UserId} з IP {IpAddress}. Причина: {Reason}", userId, ipAddress, failureReason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при логуванні входу для користувача {UserId}", userId);
            // Не кидаємо помилку - логування безпеки не повинно блокувати операцію
        }
    }

    public async Task LogPasswordChangeAsync(
        int userId,
        string? ipAddress = null,
        string? userAgent = null,
        bool isSuccessful = true,
        string? failureReason = null)
    {
        try
        {
            var entry = new SecurityLogEntry
            {
                UserId = userId,
                EventType = "PasswordChange",
                Description = isSuccessful ? "Пароль успішно змінено" : "Невдала спроба зміни пароля",
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Status = isSuccessful ? "Success" : "Failure",
                AdditionalInfo = failureReason,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddLogEntryAsync(entry);
            
            if (isSuccessful)
                _logger.LogInformation("Залоговано зміну пароля для користувача {UserId} з IP {IpAddress}", userId, ipAddress);
            else
                _logger.LogWarning("Невдала спроба зміни пароля для користувача {UserId} з IP {IpAddress}. Причина: {Reason}", userId, ipAddress, failureReason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при логуванні зміни пароля для користувача {UserId}", userId);
            // Не кидаємо помилку - логування безпеки не повинно блокувати операцію
        }
    }

    public async Task LogLogoutAsync(int userId, string? ipAddress = null)
    {
        try
        {
            var entry = new SecurityLogEntry
            {
                UserId = userId,
                EventType = "Logout",
                Description = "Вихід з системи",
                IpAddress = ipAddress,
                Status = "Success",
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddLogEntryAsync(entry);
            _logger.LogInformation("Залоговано вихід користувача {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при логуванні виходу для користувача {UserId}", userId);
            // Не кидаємо помилку
        }
    }

    public async Task LogSessionTerminatedAsync(int userId, string? ipAddress = null)
    {
        try
        {
            var entry = new SecurityLogEntry
            {
                UserId = userId,
                EventType = "SessionTerminated",
                Description = "Сеанс завершено",
                IpAddress = ipAddress,
                Status = "Success",
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddLogEntryAsync(entry);
            _logger.LogInformation("Залоговано завершення сеансу для користувача {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при логуванні завершення сеансу для користувача {UserId}", userId);
            // Не кидаємо помилку
        }
    }

    public async Task<Result<PagedResult<SecurityLogDto>>> GetUserSecurityLogsAsync(int userId, int pageNumber, string? eventType = null)
    {
        try
        {
            if (pageNumber < 1)
                pageNumber = 1;

            var normalizedEventType = string.IsNullOrWhiteSpace(eventType)
                ? null
                : eventType.Trim();

            var pageSize = _settings.DefaultPageSize;
            if (pageSize < 1 || pageSize > _settings.MaxPageSize)
                pageSize = _settings.DefaultPageSize;

            var (entries, totalCount) = await _repository.GetUserLogsAsync(userId, pageNumber, pageSize, normalizedEventType);

            var dtos = entries.Select(MapToDto).ToList();

            var result = new PagedResult<SecurityLogDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _logger.LogInformation(
                "Отримано {Count} записів журналу безпеки для користувача {UserId}. Фільтр події: {EventTypeFilter}",
                dtos.Count,
                userId,
                normalizedEventType ?? "all");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні журналу безпеки для користувача {UserId}", userId);
            return new Result<PagedResult<SecurityLogDto>>.Failure("Не вдалося отримати журнал безпеки", "ERROR");
        }
    }

    public async Task<Result<IEnumerable<SecurityLogDto>>> GetRecentLogsAsync(int userId, int count)
    {
        try
        {
            if (count < 1)
                count = 5;

            var entries = await _repository.GetRecentLogsAsync(userId, count);
            var dtos = entries.Select(MapToDto).ToList();

            _logger.LogInformation("Отримано {Count} останніх записів журналу для користувача {UserId}", dtos.Count, userId);
            return new Result<IEnumerable<SecurityLogDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні останніх записів журналу для користувача {UserId}", userId);
            return new Result<IEnumerable<SecurityLogDto>>.Failure("Не вдалося отримати записи журналу", "ERROR");
        }
    }

    public async Task<Result<int>> CleanupOldLogsAsync()
    {
        try
        {
            var deletedCount = await _repository.DeleteOldLogsAsync(_settings.RetentionDays);
            _logger.LogInformation("Видалено {Count} старих записів журналу безпеки", deletedCount);
            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при видаленні старих записів журналу безпеки");
            return new Result<int>.Failure("Не вдалося видалити старі записи", "ERROR");
        }
    }

    private SecurityLogDto MapToDto(SecurityLogEntry entry)
    {
        return new SecurityLogDto
        {
            Id = entry.Id,
            UserId = entry.UserId,
            EventType = entry.EventType,
            Description = entry.Description,
            IpAddress = entry.IpAddress,
            UserAgent = entry.UserAgent,
            CreatedAt = entry.CreatedAt,
            Status = entry.Status,
            AdditionalInfo = entry.AdditionalInfo
        };
    }
}
