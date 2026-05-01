using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.Interfaces;
using StayFit.Domain.Interfaces;

namespace StayFit.Web.Controllers;

[Authorize]
public class StatisticsController(
    IFoodService foodService, 
    IProgressService progressService, 
    IHydrationRepository hydrationRepository) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Progress(DateTime? startDate, DateTime? endDate)
    {
        var end = (endDate ?? DateTime.Today).Date;
        var start = (startDate ?? end.AddDays(-6)).Date;
        
        var userEmail = GetCurrentUserEmailOrEmpty();
        var userId = GetRequiredCurrentUserId();
        
        var waterLogs = await hydrationRepository.GetLogsForPeriodAsync(userId, start, end);
        ViewBag.WaterTotal = waterLogs.Sum(x => x.VolumeMl);
        
        var result = await progressService.GetProgressAnalysisAsync(userId, userEmail, start, end);
        
        return MatchResult(result,
            success => View(success),
            failure => 
            {
                // Використовуємо ErrorMessage, як у вашому ExportController
                TempData["ErrorMessage"] = failure.ErrorMessage;
                return RedirectToAction(nameof(Daily));
            });
    }

    [HttpGet]
    public IActionResult Index(DateTime? date) => RedirectToAction(nameof(Daily), new { date });

    [HttpGet]
    public async Task<IActionResult> Daily(DateTime? date)
    {
        var selectedDate = (date ?? DateTime.Today).Date;
        var userEmail = GetCurrentUserEmailOrEmpty();
        var userId = GetRequiredCurrentUserId();

        var logs = await foodService.GetDailyLogAsync(userEmail, selectedDate);
        var waterLogs = await hydrationRepository.GetLogsForDateAsync(userId, selectedDate);
        
        ViewBag.SelectedDate = selectedDate;
        ViewBag.WaterTotal = waterLogs.Sum(x => x.VolumeMl);
        
        return View(logs);
    }

    [HttpGet]
    public async Task<IActionResult> Weekly(DateTime? date)
    {
        var selectedDate = date ?? DateTime.Today;
        var userEmail = GetCurrentUserEmailOrEmpty();

        var logs = await foodService.GetDailyLogAsync(userEmail, selectedDate);
        return View(logs);
    }

    [HttpGet]
    public async Task<IActionResult> Monthly(DateTime? date)
    {
        var selectedDate = date ?? DateTime.Today;
        var userEmail = GetCurrentUserEmailOrEmpty();

        var logs = await foodService.GetDailyLogAsync(userEmail, selectedDate);
        return View(logs);
    }
}