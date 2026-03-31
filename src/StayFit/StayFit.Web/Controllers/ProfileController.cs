using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;

namespace StayFit.Web.Controllers;

/// <summary>
/// Контролер для управління профілем користувача
/// </summary>
[Authorize]
[Route("profile")]
public class ProfileController : BaseController
{
    private readonly IUserProfileService _userProfileService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(IUserProfileService userProfileService, ILogger<ProfileController> logger)
    {
        _userProfileService = userProfileService;
        _logger = logger;
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

        // Спробуємо оновити існуючий профіль
        var updated = await _userProfileService.UpdateProfileAsync(userId, dto);

        if (!updated)
        {
            // Якщо профіль не існує, створюємо новий
            _logger.LogInformation("Профіль не знайдено, створюємо новий для {UserId}", userId);

            var createDto = new CreateUserProfileDto
            {
                UserId = userId,
                FullName = dto.FullName,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Weight = dto.Weight,
                Height = dto.Height,
            };

            await _userProfileService.CreateProfileAsync(createDto);
        }

        _logger.LogInformation("Профіль {UserId} успішно збережено", userId);
        TempData["SuccessMessage"] = "Профіль успішно збережено!";
        return RedirectToAction(nameof(View));
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
        TempData["SuccessMessage"] = "Профіль видалено!";
        return RedirectToAction("Index", "Home");
    }

}
