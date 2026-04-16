using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Domain.Results;
using StayFit.Infrastructure.Identity;

namespace StayFit.Web.Controllers;

/// <summary>
/// Контролер для керування безпекою акаунта (зміна пароля, активні сесії тощо)
/// </summary>
[Authorize]
[Route("account-security")]
public class AccountSecurityController : BaseController
{
    private readonly IPasswordChangeService _passwordChangeService;
    private readonly ISessionService _sessionService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountSecurityController> _logger;

    public AccountSecurityController(
        IPasswordChangeService passwordChangeService,
        ISessionService sessionService,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountSecurityController> logger)
    {
        _passwordChangeService = passwordChangeService;
        _sessionService = sessionService;
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>
    /// Змінити пароль
    /// </summary>
    [HttpPost("change-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
    {
        TempData["ActiveTab"] = "Security";

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Будь ласка, виправте помилки у формі безпеки.";
            return RedirectToAction("Edit", "Profile", null, "security");
        }

        var userId = GetRequiredCurrentUserId();

        var result = await _passwordChangeService.ChangePasswordAsync(
            userId,
            request.CurrentPassword,
            request.NewPassword,
            request.ConfirmPassword);

        if (result.IsFailure)
        {
            var failure = result as Result<bool>.Failure;
            var errorMsg = failure?.ErrorMessage ?? "Не вдалося змінити пароль.";
            TempData["Error"] = errorMsg;
            _logger.LogWarning("Помилка зміни пароля для користувача {UserId}: {Error}", userId, errorMsg);
            return RedirectToAction("Edit", "Profile", null, "security");
        }

        // Оновити сесію після зміни пароля
        var user = await _signInManager.UserManager.FindByIdAsync(userId.ToString());
        if (user != null)
            await _signInManager.RefreshSignInAsync(user);

        _logger.LogInformation("Користувач {UserId} успішно змінив пароль", userId);
        TempData["Success"] = "Пароль успішно змінено!";
        return RedirectToAction("Edit", "Profile", null, "security");
    }

    /// <summary>
    /// Завершити конкретний сеанс
    /// </summary>
    [HttpPost("terminate-session")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TerminateSession(int sessionId)
    {
        var userId = GetRequiredCurrentUserId();

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
    /// </summary>
    [HttpPost("logout-all")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogoutAll()
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

    /// <summary>
    /// Видалення акаунту (заглушка для UI)
    /// </summary>
    [HttpPost("delete-account")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteAccount(string confirmationToken)
    {
        var userId = GetRequiredCurrentUserId();
        _logger.LogInformation("Запит на видалення акаунту для користувача {UserId} (тимчасова заглушка)", userId);
        TempData["ActiveTab"] = "Security";
        if (confirmationToken?.ToUpper() != "ВИДАЛИТИ")
        {
            TempData["Error"] = "Невірний токен підтвердження. Акаунт не видалено.";
            return RedirectToAction("Edit", "Profile", null, "security");
        }

        TempData["Success"] = "Функція видалення акаунту в розробці.";
        return RedirectToAction("Edit", "Profile", null, "security");
    }
}
