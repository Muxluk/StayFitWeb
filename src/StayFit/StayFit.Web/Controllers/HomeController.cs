using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.Services;
using StayFit.Web.Models;

namespace StayFit.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly LoggingService _loggingService;

    public HomeController(ILogger<HomeController> logger, LoggingService loggingService)
    {
        _logger = logger;
        _loggingService = loggingService;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Користувач відвідав сторінку Index");
        try
        {
            _logger.LogDebug("Комбінування даних для сторінки Index");
            
            // Використання LoggingService
            _loggingService.LogApplicationEvent("PageVisit", "Користувач відвідав головну сторінку");
            
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при завантаженні сторінки Index");
            throw;
        }
    }

    public IActionResult Privacy()
    {
        _logger.LogInformation("Користувач відвідав сторінку Privacy");
        try
        {
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при завантаженні сторінки Privacy");
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
