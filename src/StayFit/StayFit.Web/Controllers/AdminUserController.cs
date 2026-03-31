using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Web.Models;

namespace StayFit.Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/users")]
public class AdminUserController : BaseController
{
    private readonly IAdminUserService _adminUserService;

    public AdminUserController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] int? userId, [FromQuery] string? email)
    {
        var model = new AdminUserSearchViewModel
        {
            UserId = userId,
            Email = email,
        };

        if (userId is null && string.IsNullOrWhiteSpace(email))
        {
            return View(model);
        }

        var result = await _adminUserService.SearchUsersAsync(new AdminUserSearchRequestDto
        {
            UserId = userId,
            Email = email,
        });

        if (result.IsFailure)
        {
            AddResultErrorsToModelState(result, "Не вдалося виконати пошук користувачів");
            return View(model);
        }

        model.Users = result.Value ?? Array.Empty<AdminUserListItemDto>();
        return View(model);
    }

    [HttpGet("{userId:int}")]
    public async Task<IActionResult> Details(int userId)
    {
        var result = await _adminUserService.GetUserDetailsAsync(userId);
        if (result.IsFailure)
        {
            return NotFound();
        }

        var model = new AdminUserDetailsViewModel
        {
            User = result.Value,
        };

        return View(model);
    }

    [HttpPost("{userId:int}/block")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Block(int userId)
    {
        var result = await _adminUserService.BlockUserAsync(userId);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Errors.FirstOrDefault() ?? "Не вдалося заблокувати акаунт";
            return RedirectToAction(nameof(Details), new { userId });
        }

        TempData["Success"] = "Акаунт користувача заблоковано";
        return RedirectToAction(nameof(Details), new { userId });
    }

    [HttpPost("{userId:int}/reset-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int userId, AdminResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var detailsResult = await _adminUserService.GetUserDetailsAsync(userId);
            var detailsModel = new AdminUserDetailsViewModel
            {
                User = detailsResult.Value,
                ResetPassword = model,
            };

            return View(nameof(Details), detailsModel);
        }

        var result = await _adminUserService.ResetPasswordAsync(userId, model.NewPassword);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Errors.FirstOrDefault() ?? "Не вдалося скинути пароль";
            return RedirectToAction(nameof(Details), new { userId });
        }

        TempData["Success"] = "Пароль користувача успішно змінено";
        return RedirectToAction(nameof(Details), new { userId });
    }
}
