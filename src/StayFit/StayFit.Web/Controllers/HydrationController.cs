using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StayFit.Application.Interfaces;

namespace StayFit.Web.Controllers;

[Authorize]
public class HydrationController : BaseController
{
    private readonly IHydrationService _hydrationService;

    public HydrationController(IHydrationService hydrationService)
    {
        _hydrationService = hydrationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        int userId = GetRequiredCurrentUserId();
        var result = await _hydrationService.GetTodayProgressAsync(userId);
        
        if (!result.IsSuccess)
        {
            TempData["Error"] = string.Join(", ", result.Errors); 
            return RedirectToAction("Index", "Dashboard");
        }

        return View(result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("WaterLogPolicy")]
    public async Task<IActionResult> LogWater(int volumeMl)
    {
        int userId = GetRequiredCurrentUserId();
        var result = await _hydrationService.LogWaterAsync(userId, volumeMl);

        if (result.IsSuccess)
        {
            TempData["Success"] = $"Додано {volumeMl} мл води!";
        }
        else
        {
            TempData["Error"] = string.Join(", ", result.Errors);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetCustomGoal(int goalMl)
    {
        int userId = GetRequiredCurrentUserId();
        var result = await _hydrationService.SetGoalAsync(userId, goalMl);

        if (result.IsSuccess)
        {
            TempData["Success"] = "Ціль успішно оновлено!";
        }
        else
        {
            TempData["Error"] = string.Join(", ", result.Errors);
        }

        return RedirectToAction(nameof(Index));
    }
}