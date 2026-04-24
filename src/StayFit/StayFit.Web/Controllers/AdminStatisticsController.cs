using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.Interfaces;

namespace StayFit.Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/statistics")]
public class AdminStatisticsController : BaseController
{
    private readonly ISystemStatisticsService _statisticsService;

    public AdminStatisticsController(ISystemStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _statisticsService.GetSystemStatisticsAsync(cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Errors.FirstOrDefault() ?? "Помилка завантаження статистики";
            return RedirectToAction("Index", "Admin");
        }

        return View(result.Value);
    }
}