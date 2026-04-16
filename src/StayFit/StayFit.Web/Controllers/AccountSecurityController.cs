using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Domain.Results;

namespace StayFit.Web.Controllers;

/// <summary>
/// Контролер для керування безпекою акаунта (зміна пароля, активні сесії тощо)
/// </summary>
[Authorize]
[Route("account-security")]
public class AccountSecurityController : BaseController
{
    private readonly IPasswordChangeService _passwordChangeService;
    private readonly Microsoft.AspNetCore.Identity.SignInManager<StayFit.Infrastructure.Identity.ApplicationUser> _signInManager;
    private readonly ILogger<AccountSecurityController> _logger;

    public AccountSecurityController(
        IPasswordChangeService passwordChangeService,
        Microsoft.AspNetCore.Identity.SignInManager<StayFit.Infrastructure.Identity.ApplicationUser> signInManager,
        ILogger<AccountSecurityController> logger)
    {
        _passwordChangeService = passwordChangeService;
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>
    /// Змінити пароль користувача
    /// </summary>
    [HttpPost("change-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
    {
        if (!ModelState.IsValid)
        {
            // Якщо модель невалідна, перенаправляємо назад з помилками
            TempData["Error"] = "Будь ласка, виправте помилки у формі безпеки.";
            return RedirectToAction("Edit", "Profile");
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
            
            return RedirectToAction("Edit", "Profile");
        }

        var user = await _signInManager.UserManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            await _signInManager.RefreshSignInAsync(user);
        }

        _logger.LogInformation("Користувач {UserId} успішно змінив пароль", userId);
        TempData["Success"] = "Пароль успішно змінено!";
        
        return RedirectToAction("Edit", "Profile");
    }

    /// <summary>
    /// Вихід зі всіх активних сеансів (заглушка для UI)
    /// </summary>
    [HttpPost("logout-all")]
    [ValidateAntiForgeryToken]
    public IActionResult LogoutAll()
    {
        var userId = GetRequiredCurrentUserId();
        _logger.LogInformation("Запит на вихід з усіх сеансів для користувача {UserId} (тимчасова заглушка)", userId);
        
        TempData["Success"] = "Усі сеанси успішно завершено!";
        return RedirectToAction("Edit", "Profile");
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
        
        if (confirmationToken?.ToUpper() != "ВИДАЛИТИ")
        {
            TempData["Error"] = "Невірний токен підтвердження. Акаунт не видалено.";
            return RedirectToAction("Edit", "Profile");
        }

        TempData["Success"] = "Функція видалення акаунту в розробці.";
        return RedirectToAction("Edit", "Profile");
    }
}
