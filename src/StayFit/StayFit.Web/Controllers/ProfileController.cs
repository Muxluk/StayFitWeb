using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Application.Options;
using StayFit.Domain.Entities;

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
    private readonly ProfilePhotoOptions _profilePhotoOptions;
    private readonly IWebHostEnvironment _environment;

    public ProfileController(
        IUserProfileService userProfileService,
        ISessionService sessionService,
        ILogger<ProfileController> logger,
        IOptions<ProfilePhotoOptions> profilePhotoOptions,
        IWebHostEnvironment environment)
    {
        _userProfileService = userProfileService;
        _sessionService = sessionService;
        _logger = logger;
        _profilePhotoOptions = profilePhotoOptions.Value;
        _environment = environment;
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

        var dto = new UpdateUserProfileDto
        {
            FullName = profile?.FullName ?? string.Empty,
            DateOfBirth = profile?.DateOfBirth,
            Gender = profile?.Gender,
            Weight = profile?.Weight,
            Height = profile?.Height,
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
    /// Завантажити фото профілю поточного користувача
    /// POST: /profile/upload-photo
    /// </summary>
    [HttpPost("upload-photo")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPhoto(IFormFile? photo)
    {
        var userId = GetRequiredCurrentUserId();

        if (photo is null || photo.Length == 0)
        {
            TempData["ErrorMessage"] = "Оберіть файл для завантаження.";
            return RedirectToAction(nameof(Edit));
        }

        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
        var allowedExtensions = (_profilePhotoOptions.AllowedExtensions ?? Array.Empty<string>())
            .Select(e => e.StartsWith('.') ? e.ToLowerInvariant() : $".{e.ToLowerInvariant()}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!allowedExtensions.Contains(extension))
        {
            TempData["ErrorMessage"] = $"Недозволений формат файлу. Дозволено: {string.Join(", ", allowedExtensions)}";
            return RedirectToAction(nameof(Edit));
        }

        var maxFileSizeBytes = _profilePhotoOptions.MaxFileSizeMb * 1024 * 1024;
        if (photo.Length > maxFileSizeBytes)
        {
            TempData["ErrorMessage"] = $"Файл занадто великий. Максимальний розмір: {_profilePhotoOptions.MaxFileSizeMb} MB.";
            return RedirectToAction(nameof(Edit));
        }

        var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", "profile-photos", userId.ToString());
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadsRoot, fileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await photo.CopyToAsync(stream);
        }

        _logger.LogInformation(
            "Користувач {UserId} завантажив фото профілю {FileName}",
            userId,
            fileName);

        TempData["SuccessMessage"] = "Фото профілю завантажено."
            + " Збереження шляху в профіль буде виконано на наступному кроці (репозиторій/сервіс).";

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
