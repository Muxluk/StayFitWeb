using Microsoft.Extensions.Logging;

namespace StayFit.Application.Services;

/// <summary>
/// Приклад сервісу з логуванням.
/// </summary>
public class LoggingService
{
    private readonly ILogger<LoggingService> _logger;

    public LoggingService(ILogger<LoggingService> logger)
    {
        _logger = logger;
    }

    public async Task<string> ProcessDataAsync(string data)
    {
        _logger.LogInformation("Почато обробку даних: {Data}", data);

        _logger.LogDebug("Перевірка вхідних даних");

        if (string.IsNullOrEmpty(data))
        {
            _logger.LogWarning("Отримані порожні дані для обробки");
            return "Помилка: дані порожні";
        }

        _logger.LogInformation("Дані успішно перевірені. Довжина: {Length}", data.Length);

        // Симуляція асинхронної обробки
        await Task.Delay(100);

        var result = data.ToUpper();
        _logger.LogInformation("Обробка завершена успішно. Результат: {Result}", result);

        return result;
    }

    public void LogApplicationEvent(string eventName, string details)
    {
        _logger.LogInformation("Подія додатку: {EventName} | Деталі: {Details}", eventName, details);
    }

    public void LogPerformanceMetric(string metricName, long milliseconds)
    {
        if (milliseconds > 1000)
        {
            _logger.LogWarning("Повільна операція: {MetricName} тривала {Milliseconds}ms", metricName, milliseconds);
        }
        else
        {
            _logger.LogInformation("Метрика продуктивності: {MetricName} - {Milliseconds}ms", metricName, milliseconds);
        }
    }
}
