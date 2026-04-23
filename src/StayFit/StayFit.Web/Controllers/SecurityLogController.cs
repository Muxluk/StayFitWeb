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

            if (result.IsSuccess)
            {
                var successResult = result as Result<Application.DTOs.PagedResult<Application.DTOs.SecurityLogDto>>.Success;
                return Ok(successResult?.Data);
            }

            var failureResult = result as Result<Application.DTOs.PagedResult<Application.DTOs.SecurityLogDto>>.Failure;
            _logger.LogWarning("Помилка при отриманні журналу безпеки для користувача {UserId}: {Error}",
                userId, failureResult?.ErrorMessage);
            return BadRequest(new { message = failureResult?.ErrorMessage ?? "Помилка при отриманні журналу" });
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

            if (result.IsSuccess)
            {
                var successResult = result as Result<System.Collections.Generic.IEnumerable<Application.DTOs.SecurityLogDto>>.Success;
                return Ok(successResult?.Data);
            }

            var failureResult = result as Result<System.Collections.Generic.IEnumerable<Application.DTOs.SecurityLogDto>>.Failure;
            _logger.LogWarning("Помилка при отриманні останніх записів журналу для користувача {UserId}: {Error}",
                userId, failureResult?.ErrorMessage);
            return BadRequest(new { message = failureResult?.ErrorMessage ?? "Помилка при отриманні записів" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Невідома помилка при отриманні останніх записів журналу для користувача {UserId}", userId);
            return StatusCode(500, new { message = "Внутрішня помилка сервера" });
        }
    }
}
