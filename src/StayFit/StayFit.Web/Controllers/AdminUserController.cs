using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Web.Models;
using Microsoft.Extensions.Options;
using StayFit.Application.Configuration;

namespace StayFit.Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/users")]
public class AdminUserController : BaseController
{
    private readonly IAdminUserService _adminUserService;
    private readonly PaginationSettings _paginationSettings;
    public AdminUserController(
        IAdminUserService adminUserService, 
        IOptions<PaginationSettings> paginationSettings)
    {
        _adminUserService = adminUserService;
        _paginationSettings = paginationSettings.Value;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] int? userId, [FromQuery] string? email, [FromQuery] int page = 1)
    {
        var model = new AdminUserSearchViewModel
        {
            UserId = userId,
            Email = email,
        };

        var result = await _adminUserService.SearchUsersAsync(new AdminUserSearchRequestDto
        {
            UserId = userId,
            Email = email,
            PageNumber = page > 0 ? page : 1,
            PageSize = _paginationSettings.DefaultPageSize
        });

        if (result.IsFailure)
        {
            AddResultErrorsToModelState(result, "Не вдалося виконати пошук користувачів");
            return View(model);
        }

        model.Users = result.Value ?? new PagedResult<AdminUserListItemDto>();
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

    [HttpGet("{userId:int}/edit")]
    public async Task<IActionResult> Edit(int userId)
    {
        var result = await _adminUserService.GetUserDetailsAsync(userId);
        if (result.IsFailure)
        {
            return NotFound();
        }

        var userDetails = result.Value!;
        var model = new AdminUserEditViewModel
        {
            UserId = userId,
            UserName = userDetails.UserName,
            Email = userDetails.Email,
            FullName = userDetails.Profile?.FullName,
            DateOfBirth = userDetails.Profile?.DateOfBirth,
            Gender = userDetails.Profile?.Gender,
            Weight = userDetails.Profile?.Weight,
            Height = userDetails.Profile?.Height,
        };

        return View(model);
    }

    [HttpPost("{userId:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int userId, AdminUserEditViewModel model)
    {
        if (userId != model.UserId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var updateRequest = new AdminUpdateUserRequestDto
        {
            UserName = model.UserName ?? string.Empty,
            Email = model.Email ?? string.Empty,
            FullName = model.FullName,
            DateOfBirth = model.DateOfBirth,
            Gender = model.Gender,
            Weight = model.Weight,
            Height = model.Height,
        };

        var result = await _adminUserService.UpdateUserAsync(userId, updateRequest);
        if (result.IsFailure)
        {
            AddResultErrorsToModelState(result, "Не вдалося оновити дані акаунта");
            return View(model);
        }

        TempData["Success"] = "Дані акаунта користувача успішно оновлено";
        return RedirectToAction(nameof(Details), new { userId });
    }

    [HttpPost("{userId:int}/block")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BlockUser(int userId)
    {
        var result = await _adminUserService.BlockUserAsync(userId);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Errors.FirstOrDefault() ?? "Не вдалося заблокувати користувача";
            return RedirectToAction(nameof(Details), new { userId });
        }

        TempData["Success"] = "Користувач успішно заблокований";
        return RedirectToAction(nameof(Details), new { userId });
    }

    [HttpPost("{userId:int}/unblock")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnblockUser(int userId)
    {
        var result = await _adminUserService.UnblockUserAsync(userId);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Errors.FirstOrDefault() ?? "Не вдалося розблокувати користувача";
            return RedirectToAction(nameof(Details), new { userId });
        }

        TempData["Success"] = "Користувач успішно розблокований";
        return RedirectToAction(nameof(Details), new { userId });
    }

    [HttpPost("{userId:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        var result = await _adminUserService.DeleteUserAsync(userId);
        if (result.IsFailure)
        {
            TempData["Error"] = result.Errors.FirstOrDefault() ?? "Не вдалося видалити користувача";
            return RedirectToAction(nameof(Details), new { userId });
        }

        TempData["Success"] = "Користувач успішно видалений";
        return RedirectToAction(nameof(Index));
    }
}
