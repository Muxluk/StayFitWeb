using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFit.Domain.Interfaces;

namespace StayFit.Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin")]
public class AdminController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IFoodRepository _foodRepository;
    private readonly ISupportMessageRepository _supportMessageRepository;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IUserRepository userRepository,
        IFoodRepository foodRepository,
        ISupportMessageRepository supportMessageRepository,
        ILogger<AdminController> logger)
    {
        _userRepository = userRepository;
        _foodRepository = foodRepository;
        _supportMessageRepository = supportMessageRepository;
        _logger = logger;
    }

    [HttpGet("")]
    public IActionResult Index() => View();

    [HttpGet("statistics")]
    public async Task<IActionResult> Statistics()
    {
        _logger.LogInformation("Адмін переглядає статистику системи");

        var allUsers = await _userRepository.GetAllAsync();
        var pendingProducts = await _foodRepository.GetPendingProductsAsync();
        var unnotifiedMessages = await _supportMessageRepository.GetUnnotifiedAsync();

        var stats = new AdminStatisticsViewModel
        {
            TotalUsers = allUsers.Count(),
            TotalProducts = (await _foodRepository.GetAllAsync()).Count(),
            PendingProductsCount = pendingProducts.Count(),
            SupportMessagesCount = unnotifiedMessages.Count(),
            LastUpdated = DateTime.UtcNow
        };

        return View(stats);
    }
}

public class AdminStatisticsViewModel
{
    public int TotalUsers { get; set; }
    public int TotalProducts { get; set; }
    public int PendingProductsCount { get; set; }
    public int SupportMessagesCount { get; set; }
    public DateTime LastUpdated { get; set; }
}
