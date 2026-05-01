using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.Interfaces;
using StayFit.Application.DTOs;

namespace StayFit.Web.Controllers;

[Authorize]
[Route("dashboard")]
public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboardService;
    private readonly IHydrationService _hydrationService;
    private readonly ILogger<DashboardController> _logger;
    
    public DashboardController(
        IDashboardService dashboardService, 
        IHydrationService hydrationService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _hydrationService = hydrationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Користувач відвідав сторінку дашборду");

        var userId = GetRequiredCurrentUserId();
        var userEmail = GetCurrentUserEmailOrEmpty();
        var result = await _dashboardService.GetTodayDashboardAsync(userId, userEmail);
        var waterProgress = await _hydrationService.GetTodayProgressAsync(GetRequiredCurrentUserId());
        ViewBag.WaterProgress = waterProgress.IsSuccess ? waterProgress.Value : new HydrationProgressDto();
        var waterResult = await _hydrationService.GetTodayProgressAsync(userId);
        ViewBag.WaterProgress = waterResult.IsSuccess ? waterResult.Value : new HydrationProgressDto();


        if (result.IsFailure)
        {
            _logger.LogWarning("Помилка при отриманні даних дашборду: {Errors}", string.Join(", ", result.Errors));
            AddResultErrorsToModelState(result);
            return View();
        }

        return View(result.Value);
    }
}