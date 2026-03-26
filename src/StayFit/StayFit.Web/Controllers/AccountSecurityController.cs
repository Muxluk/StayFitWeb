using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.Services;
using StayFit.Domain.Results;
using StayFit.Domain.Entities;
using StayFit.Infrastructure.Identity;

namespace StayFit.Web.Controllers;

/// <summary>
/// Контролер для операцій безпеки акаунта
/// Тільки виклики сервісів, обробка результатів глобально
/// </summary>
[Authorize]
[Route("account/security")]
[ApiExplorerSettings(IgnoreApi = true)]
public class AccountSecurityController : Controller
{
    private readonly AccountSecurityService _accountSecurityService;
    private readonly ILogger<AccountSecurityController> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountSecurityController(
        AccountSecurityService accountSecurityService,
        ILogger<AccountSecurityController> logger,
        SignInManager<ApplicationUser> signInManager)
    {
        _accountSecurityService = accountSecurityService;
        _logger = logger;
        _signInManager = signInManager;
    }

    // ────────────────────────────────────────────────────────────────────────
    // GET
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Сторінка безпеки акаунта
    /// </summary>
    [HttpGet("")]
    public IActionResult Index()
    {
        // Перенаправити на Profile Edit (де інтегровано всю безпеку)
        return RedirectToAction("Edit", "Profile");
    }

    // ────────────────────────────────────────────────────────────────────────
    // POST
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Змінити пароль
    /// </summary>
    [HttpPost("change-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
            return RedirectToAction("Edit", "Profile");

        var userId = GetUserId();
        var result = await _accountSecurityService.ChangePasswordAsync(
            userId,
            request.CurrentPassword ?? string.Empty,
            request.NewPassword ?? string.Empty,
            request.ConfirmPassword ?? string.Empty);

        return result.Match(
            success =>
            {
                TempData["Success"] = "Пароль успішно змінено";
                return RedirectToAction("Edit", "Profile");
            },
            failure =>
            {
                TempData["Error"] = failure.ErrorMessage;
                return RedirectToAction("Edit", "Profile");
            });
    }

    /// <summary>
    /// Вихід з усіх сесій
    /// </summary>
    [HttpPost("logout-all")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogoutAll()
    {
        var userId = GetUserId();
        var result = await _accountSecurityService.LogoutAllSessionsAsync(userId);

        return result.Match(
            success =>
            {
                // Вилогінити користувача з усіх сеансів
                _signInManager.SignOutAsync().Wait();
                TempData["Success"] = "Ви вийшли зі всіх сеансів";
                return RedirectToAction("Index", "Home");
            },
            failure =>
            {
                TempData["Error"] = failure.ErrorMessage;
                return RedirectToAction("Edit", "Profile");
            });
    }

    /// <summary>
    /// Видалити акаунт
    /// </summary>
    [HttpPost("delete-account")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount([FromForm] DeleteAccountRequest request)
    {
        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Index));

        var userId = GetUserId();
        var result = await _accountSecurityService.DeleteAccountAsync(
            userId,
            request.ConfirmationToken ?? string.Empty);

        return result.Match(
            success =>
            {
                TempData["Success"] = "Акаунт успішно видалено";
                return RedirectToAction("Index", "Home");
            },
            failure =>
            {
                TempData["Error"] = failure.ErrorMessage;
                return RedirectToAction("Edit", "Profile");
            });
    }

    // ────────────────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────────────────

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        throw new InvalidOperationException("Не вдалося отримати ID користувача");
    }
}

// ────────────────────────────────────────────────────────────────────────────
// DTOs
// ────────────────────────────────────────────────────────────────────────────

public class AccountSecurityViewModel
{
    public List<UserSession> ActiveSessions { get; set; } = new();
}

public class ChangePasswordRequest
{
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }
}

public class DeleteAccountRequest
{
    public string? ConfirmationToken { get; set; }
}
