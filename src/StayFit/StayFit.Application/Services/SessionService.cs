using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StayFit.Application.Interfaces;
using StayFit.Application.Options;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Domain.Results;

namespace StayFit.Application.Services;

/// <summary>
/// Сервіс для керування активними сеансами користувача
/// Використовує Result патерн — без throw, всі помилки через Result.Failure
/// </summary>
public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly SessionSettings _settings;
    private readonly ILogger<SessionService> _logger;

    public SessionService(
        ISessionRepository sessionRepository,
        IOptions<SessionSettings> settings,
        ILogger<SessionService> logger)
    {
        _sessionRepository = sessionRepository;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IList<UserSession>>> GetActiveSessionsAsync(int userId)
    {
        _logger.LogInformation("Отримання активних сеансів для користувача {UserId}", userId);

        var sessions = await _sessionRepository.GetActiveByUserIdAsync(userId);

        _logger.LogInformation("Знайдено {Count} активних сеансів для користувача {UserId}", sessions.Count, userId);

        return new Result<IList<UserSession>>.Success(sessions);
    }

    /// <inheritdoc/>
    public async Task<bool> IsSessionValidAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;
        return await _sessionRepository.IsValidAsync(token);
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> TerminateSessionAsync(int userId, int sessionId)
    {
        _logger.LogInformation("Завершення сеансу {SessionId} для користувача {UserId}", sessionId, userId);

        // Отримати всі активні сеанси для перевірки власника
        var sessions = await _sessionRepository.GetActiveByUserIdAsync(userId);
        var targetSession = sessions.FirstOrDefault(s => s.Id == sessionId);

        if (targetSession == null)
        {
            _logger.LogWarning("Сеанс {SessionId} не знайдено серед активних сеансів користувача {UserId}", sessionId, userId);
            return new Result<bool>.Failure("Сеанс не знайдено або він вже завершений", "SESSION_NOT_FOUND");
        }

        // Перевірка: сеанс належить поточному користувачу
        if (targetSession.UserId != userId)
        {
            _logger.LogWarning("Користувач {UserId} намагається завершити чужий сеанс {SessionId}", userId, sessionId);
            return new Result<bool>.Failure("Немає доступу до цього сеансу", "ACCESS_DENIED");
        }

        var success = await _sessionRepository.DeactivateAsync(sessionId);

        if (success)
        {
            _logger.LogInformation("Сеанс {SessionId} успішно завершено для користувача {UserId}", sessionId, userId);
            return new Result<bool>.Success(true);
        }

        _logger.LogError("Не вдалося завершити сеанс {SessionId} для користувача {UserId}", sessionId, userId);
        return new Result<bool>.Failure("Не вдалося завершити сеанс", "TERMINATE_FAILED");
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> TerminateAllExceptCurrentAsync(int userId, string currentToken)
    {
        if (string.IsNullOrWhiteSpace(currentToken))
        {
            _logger.LogWarning("Спроба завершити всі сеанси без поточного токена для {UserId}", userId);
            return new Result<bool>.Failure("Поточний сеанс не визначено", "INVALID_TOKEN");
        }

        _logger.LogInformation("Завершення всіх сеансів крім поточного для користувача {UserId}", userId);

        var count = await _sessionRepository.DeactivateAllExceptAsync(userId, currentToken);

        _logger.LogInformation("Завершено {Count} сеансів для користувача {UserId}", count, userId);

        return new Result<bool>.Success(true);
    }

    /// <inheritdoc/>
    public async Task<string> CreateSessionAsync(int userId, string? ipAddress, string? userAgent)
    {
        var token = Guid.NewGuid().ToString("N");

        var session = new UserSession
        {
            UserId = userId,
            SessionToken = token,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(_settings.SessionLifetimeHours),
            IsActive = true
        };

        await _sessionRepository.CreateAsync(session);

        _logger.LogInformation("Створено новий сеанс для користувача {UserId}, IP: {Ip}", userId, ipAddress ?? "невідомо");

        return token;
    }

    /// <inheritdoc/>
    public async Task DeactivateSessionAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return;

        var session = await _sessionRepository.GetByTokenAsync(token);

        if (session == null)
        {
            _logger.LogDebug("Сеанс з токеном не знайдено при деактивації");
            return;
        }

        await _sessionRepository.DeactivateAsync(session.Id);
        _logger.LogInformation("Сеанс {SessionId} деактивовано для користувача {UserId}", session.Id, session.UserId);
    }
}
