using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StayFit.Application.Common;
using StayFit.Application.Configuration;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;

namespace StayFit.Application.Services;

public class SystemStatisticsService : ISystemStatisticsService
{
    private readonly ISystemStatisticsRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly SystemStatisticsSettings _settings;
    private readonly ILogger<SystemStatisticsService> _logger;

    private const string CacheKey = "AdminSystemStatistics";

    public SystemStatisticsService(
        ISystemStatisticsRepository repository,
        IMemoryCache cache,
        IOptions<SystemStatisticsSettings> options,
        ILogger<SystemStatisticsService> logger)
    {
        _repository = repository;
        _cache = cache;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<Result<SystemStatisticsDto>> GetSystemStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_cache.TryGetValue(CacheKey, out SystemStatisticsDto? stats) && stats != null)
            {
                _logger.LogInformation("Статистика системи успішно завантажена з MemoryCache.");
                return Result<SystemStatisticsDto>.Success(stats);
            }

            _logger.LogInformation("Дані статистики відсутні в кеші. Виконання запиту до БД...");
            
            stats = await _repository.GetStatisticsAsync(cancellationToken);

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(_settings.CacheDurationMinutes));

            _cache.Set(CacheKey, stats, cacheOptions);
            
            _logger.LogInformation("Статистика системи агрегована з БД та збережена в кеш на {Minutes} хвилин.", _settings.CacheDurationMinutes);

            return Result<SystemStatisticsDto>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Сталася помилка при отриманні статистики системи.");
            return Result<SystemStatisticsDto>.Failure("Не вдалося завантажити статистику системи через внутрішню помилку.");
        }
    }
}