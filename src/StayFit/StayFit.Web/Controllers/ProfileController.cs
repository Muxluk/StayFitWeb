using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Web.Services;

namespace StayFit.Web.Controllers;

/// <summary>
/// Контролер для управління профілем користувача
/// </summary>
[Authorize]
[Route("profile")]
public class ProfileController : BaseController
{
    private readonly IUserProfileService _userProfileService;
    private readonly ISessionService _sessionService;
    private readonly ILogger<ProfileController> _logger;
    private readonly IProfilePhotoService _profilePhotoService;

    public ProfileController(
        IUserProfileService userProfileService,
        ISessionService sessionService,
        ILogger<ProfileController> logger,
        IProfilePhotoService profilePhotoService)
    {
        _userProfileService = userProfileService;
        _sessionService = sessionService;
        _logger = logger;
        _profilePhotoService = profilePhotoService;
    }

    /// <summary>
    /// Сторінка редагування профілю поточного користувача
    /// GET: /profile/edit
    /// </summary>
    [HttpGet("edit")]
    public async Task<IActionResult> Edit()
    {
        var userId = GetRequiredCurrentUserId();
        _logger.LogInformation("Користувач {UserId} відкрив сторінку редагування", userId);

        var profile = await _userProfileService.GetProfileAsync(userId);
        ViewBag.ProfilePhotoPath = profile?.ProfilePhotoPath;

        var dto = new UpdateUserProfileDto
        {
            FullName = profile?.FullName ?? string.Empty,
            DateOfBirth = profile?.DateOfBirth,
            Gender = profile?.Gender,
            Weight = profile?.Weight,
            Height = profile?.Height,
            ActivityLevel = profile?.ActivityLevel,
        };

        // Завантажити активні сесії для вкладки Безпека
        var sessionsResult = await _sessionService.GetActiveSessionsAsync(userId);
        var currentToken = Request.Cookies["SessionToken"] ?? string.Empty;
        ViewBag.ActiveSessions = sessionsResult.IsSuccess
            ? ((Domain.Results.Result<IList<UserSession>>.Success)sessionsResult).Data
            : new List<UserSession>();
        ViewBag.CurrentSessionToken = currentToken;

        return View(dto);
    }

    /// <summary>
    /// Зберегти профіль поточного користувача
    /// POST: /profile/edit
    /// </summary>
    [HttpPost("edit")]
    public async Task<IActionResult> Edit(UpdateUserProfileDto dto)
    {
        var userId = GetRequiredCurrentUserId();
        _logger.LogInformation("Користувач {UserId} зберігає профіль", userId);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Невалідні дані при збереженні профілю {UserId}", userId);
            return View(dto);
        }

        await _userProfileService.UpdateProfileAsync(userId, dto);

        _logger.LogInformation("Профіль {UserId} успішно збережено", userId);
        TempData["SuccessMessage"] = "Профіль успішно збережено!";
        return RedirectToAction(nameof(View));
    }

    /// <summary>
    /// Автоматичне фонове збереження профілю
    /// POST: /profile/auto-save
    /// </summary>
    [HttpPost("auto-save")]
    public async Task<IActionResult> AutoSave([FromBody] UpdateUserProfileDto dto)
    {
        var userId = GetRequiredCurrentUserId();

        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        try
        {
            await _userProfileService.UpdateProfileAsync(userId, dto);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка автозбереження профілю для користувача {UserId}", userId);
            return StatusCode(500, new { success = false, message = "Внутрішня помилка при збереженні" });
        }
    }

    /// <summary>
    /// Завантажити фото профілю поточного користувача
    /// POST: /profile/upload-photo
    /// </summary>
    [HttpPost("upload-photo")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPhoto(IFormFile? photo)
    {
        var userId = GetRequiredCurrentUserId();
        var result = await _profilePhotoService.UploadAsync(userId, photo, HttpContext.RequestAborted);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Edit));
        }

        TempData["SuccessMessage"] = "Фото профілю завантажено.";

        return RedirectToAction(nameof(Edit));
    }

    /// <summary>
    /// Переглянути профіль поточного користувача
    /// GET: /profile/view або /profile
    /// </summary>
    [HttpGet("view")]
    [HttpGet("")]
    public new async Task<IActionResult> View()
    {
        var userId = GetRequiredCurrentUserId();
        _logger.LogInformation("Користувач {UserId} переглядає власний профіль", userId);

        var profile = await _userProfileService.GetProfileAsync(userId);

        if (profile == null)
        {
            _logger.LogInformation("Профіль {UserId} не існує, перенаправляємо на редагування", userId);
            return RedirectToAction(nameof(Edit));
        }

        return View(profile);
    }

    /// <summary>
    /// Видалити профіль поточного користувача
    /// DELETE: /profile/delete
    /// </summary>
    [HttpDelete("delete")]
    public async Task<IActionResult> Delete()
    {
        var userId = GetRequiredCurrentUserId();
        _logger.LogInformation("Користувач {UserId} видаляє профіль", userId);

        var result = await _userProfileService.DeleteProfileAsync(userId);

        if (!result)
        {
            _logger.LogWarning("Профіль {UserId} не знайдено", userId);
            return NotFound("Профіль не знайдено");
        }

        _logger.LogInformation("Профіль {UserId} успішно видалено", userId);
        TempData["Success"] = "Профіль видалено!";
        return RedirectToAction("Index", "Home");
    }
}
