using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace StayFit.Web.Controllers;

public abstract class BaseController : Controller
{
    protected int GetCurrentUserIdOrDefault()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    protected int GetRequiredCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        throw new InvalidOperationException("Не вдалося отримати ID користувача");
    }

    protected string GetCurrentUserEmailOrEmpty() => User.Identity?.Name ?? string.Empty;

    protected void AddResultErrorsToModelState(StayFit.Application.Common.Result result, string fallbackMessage = "Сталася помилка")
    {
        if (result.Errors.Count == 0)
        {
            ModelState.AddModelError(string.Empty, fallbackMessage);
            return;
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }
    }

    protected IActionResult MatchResult<T>(
        StayFit.Domain.Results.Result<T> result,
        Func<T, IActionResult> onSuccess,
        Func<StayFit.Domain.Results.Result<T>.Failure, IActionResult> onFailure)
    {
        return result.Match(
            success => onSuccess(success.Data),
            failure => onFailure(failure));
    }
}
