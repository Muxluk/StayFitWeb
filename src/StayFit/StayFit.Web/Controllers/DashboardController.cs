using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.Interfaces;

namespace StayFit.Web.Controllers;

[Authorize]
[Route("dashboard")]
public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Користувач відвідав сторінку дашборду");

        var userId = GetRequiredCurrentUserId();
        var userEmail = GetCurrentUserEmailOrEmpty();
        var result = await _dashboardService.GetTodayDashboardAsync(userId, userEmail);

        if (result.IsFailure)
        {
            _logger.LogWarning("Помилка при отриманні даних дашборду: {Errors}", string.Join(", ", result.Errors));
            AddResultErrorsToModelState(result);
            return View();
        }

        return View(result.Value);
    }
}
