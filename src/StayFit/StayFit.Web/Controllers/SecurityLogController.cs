using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StayFit.Application.Interfaces;
using StayFit.Domain.Results;

namespace StayFit.Web.Controllers;

/// <summary>
/// Контролер для керування журналом безпеки акаунта
/// </summary>
[Authorize]
[Route("security-log")]
public class SecurityLogController : BaseController
{
    private readonly ISecurityLogService _securityLogService;
    private readonly ILogger<SecurityLogController> _logger;

    public SecurityLogController(
        ISecurityLogService securityLogService,
        ILogger<SecurityLogController> logger)
    {
        _securityLogService = securityLogService;
        _logger = logger;
    }

    /// <summary>
    /// Отримати журнал безпеки користувача з пагінацією
    /// </summary>
    [HttpGet("logs")]
    public async Task<IActionResult> GetSecurityLogs(int page = 1)
    {
        var userId = GetRequiredCurrentUserId();

        try
        {
            var result = await _securityLogService.GetUserSecurityLogsAsync(userId, page);
            return result.Match<IActionResult>(
                success => Ok(success.Data),
                failure =>
                {
                    _logger.LogWarning("Помилка при отриманні журналу безпеки для користувача {UserId}: {Error}",
                        userId, failure.ErrorMessage);
                    return BadRequest(new { message = failure.ErrorMessage });
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Невідома помилка при отриманні журналу безпеки для користувача {UserId}", userId);
            return StatusCode(500, new { message = "Внутрішня помилка сервера" });
        }
    }

    /// <summary>
    /// Отримати останні записи журналу (для інформаційної панелі)
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentLogs(int count = 5)
    {
        var userId = GetRequiredCurrentUserId();

        try
        {
            var result = await _securityLogService.GetRecentLogsAsync(userId, count);
            return result.Match<IActionResult>(
                success => Ok(success.Data),
                failure =>
                {
                    _logger.LogWarning("Помилка при отриманні останніх записів журналу для користувача {UserId}: {Error}",
                        userId, failure.ErrorMessage);
                    return BadRequest(new { message = failure.ErrorMessage });
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Невідома помилка при отриманні останніх записів журналу для користувача {UserId}", userId);
            return StatusCode(500, new { message = "Внутрішня помилка сервера" });
        }
    }
}
