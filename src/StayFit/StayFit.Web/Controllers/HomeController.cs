using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.Interfaces;
using StayFit.Application.DTOs;
using StayFit.Application.Services;
using StayFit.Web.Models;

namespace StayFit.Web.Controllers;

public class HomeController : BaseController
{
    private readonly ILogger<HomeController> _logger;
    private readonly LoggingService _loggingService;
    private readonly IDashboardService _dashboardService;

    public HomeController(
        ILogger<HomeController> logger,
        LoggingService loggingService,
        IDashboardService dashboardService)
    {
        _logger = logger;
        _loggingService = loggingService;
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Користувач відвідав сторінку Index");
        try
        {
            _logger.LogDebug("Комбінування даних для сторінки Index");

            // Використання LoggingService
            _loggingService.LogApplicationEvent("PageVisit", "Користувач відвідав головну сторінку");

            DashboardDto? model = null;

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = GetRequiredCurrentUserId();
                var userEmail = GetCurrentUserEmailOrEmpty();
                var dashboardResult = await _dashboardService.GetTodayDashboardAsync(userId, userEmail);

                if (dashboardResult.IsFailure)
                {
                    AddResultErrorsToModelState(dashboardResult);
                }
                else
                {
                    model = dashboardResult.Value;
                }
            }

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при завантаженні сторінки Index");
            throw;
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        _logger.LogError("Сталася помилка. ID запиту: {RequestId}", requestId);

        return View(new ErrorViewModel { RequestId = requestId });
    }
}
