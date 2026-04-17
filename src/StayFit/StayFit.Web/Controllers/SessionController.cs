using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StayFit.Application.Interfaces;
using StayFit.Domain.Results;

namespace StayFit.Web.Controllers;

/// <summary>
/// Контролер для управління активними сеансами користувача
/// </summary>
[Authorize]
[Route("sessions")]
public class SessionController : BaseController
{
    private readonly ISessionService _sessionService;
    private readonly ILogger<SessionController> _logger;

    public SessionController(ISessionService sessionService, ILogger<SessionController> logger)
    {
        _sessionService = sessionService;
        _logger = logger;
    }

    /// <summary>
    /// Завершити конкретний сеанс
    /// POST: /sessions/terminate
    /// </summary>
    [HttpPost("terminate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TerminateSession(int sessionId)
    {
        var userId = GetRequiredCurrentUserId();
        TempData["ActiveTab"] = "Security";

        var result = await _sessionService.TerminateSessionAsync(userId, sessionId);

        if (result.IsFailure)
        {
            var failure = result as Result<bool>.Failure;
            TempData["Error"] = failure?.ErrorMessage ?? "Не вдалося завершити сеанс.";
            _logger.LogWarning("Помилка завершення сеансу {SessionId} для користувача {UserId}", sessionId, userId);
        }
        else
        {
            TempData["Success"] = "Сеанс успішно завершено.";
            _logger.LogInformation("Сеанс {SessionId} завершено для користувача {UserId}", sessionId, userId);
        }

        return RedirectToAction("Edit", "Profile", null, "security");
    }

    /// <summary>
    /// Вийти зі всіх інших сеансів (крім поточного)
    /// POST: /sessions/terminate-all
    /// </summary>
    [HttpPost("terminate-all")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TerminateAllExceptCurrent()
    {
        var userId = GetRequiredCurrentUserId();
        TempData["ActiveTab"] = "Security";
        var currentToken = Request.Cookies["SessionToken"] ?? string.Empty;

        var result = await _sessionService.TerminateAllExceptCurrentAsync(userId, currentToken);

        if (result.IsFailure)
        {
            var failure = result as Result<bool>.Failure;
            TempData["Error"] = failure?.ErrorMessage ?? "Не вдалося завершити сеанси.";
        }
        else
        {
            TempData["Success"] = "Усі інші сеанси успішно завершено!";
            _logger.LogInformation("Користувач {UserId} завершив усі інші сеанси", userId);
        }

        return RedirectToAction("Edit", "Profile", null, "security");
    }
}
