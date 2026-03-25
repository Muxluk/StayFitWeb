using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.Interfaces;

namespace StayFit.Web.Controllers;

[Authorize]
public class StatisticsController(IFoodService foodService) : Controller
{
    [HttpGet]
    public IActionResult Index(DateTime? date) => RedirectToAction(nameof(Daily), new { date });

    [HttpGet]
    public async Task<IActionResult> Daily(DateTime? date)
    {
        var selectedDate = (date ?? DateTime.Today).Date;
        var userEmail = User.Identity?.Name ?? string.Empty;

        var logs = await foodService.GetDailyLogAsync(userEmail, selectedDate);
        ViewBag.SelectedDate = selectedDate;
        return View(logs);
    }

    [HttpGet]
    public async Task<IActionResult> Weekly(DateTime? date)
    {
        var selectedDate = date ?? DateTime.Today;
        var userEmail = User.Identity?.Name ?? string.Empty;

        var logs = await foodService.GetDailyLogAsync(userEmail, selectedDate);
        return View(logs);
    }

    [HttpGet]
    public async Task<IActionResult> Monthly(DateTime? date)
    {
        var selectedDate = date ?? DateTime.Today;
        var userEmail = User.Identity?.Name ?? string.Empty;

        var logs = await foodService.GetDailyLogAsync(userEmail, selectedDate);
        return View(logs);
    }
}
