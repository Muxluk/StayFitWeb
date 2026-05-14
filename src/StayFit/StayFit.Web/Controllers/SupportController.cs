using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Application.Common;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Identity;
using StayFit.Web.Filters;
using StayFit.Web.Hubs;

namespace StayFit.Web.Controllers;

[Authorize]
[Route("support")]
public class SupportController : Controller
{
    private readonly ISupportService _supportService;
    private readonly ISupportRepository _supportRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<SupportController> _logger;

    public SupportController(
        ISupportService supportService,
        ISupportRepository supportRepository,
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        IHubContext<NotificationHub> hubContext,
        UserManager<ApplicationUser> userManager,
        ILogger<SupportController> logger)
    {
        _supportService = supportService;
        _supportRepository = supportRepository;
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _hubContext = hubContext;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var userId = ResolveUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var result = await _supportService.GetMyTicketsAsync(userId.Value);
        if (result.IsFailure)
        {
            // surface empty list on failure (service logs the error)
            return View(Enumerable.Empty<SupportTicketDto>());
        }

        return View(result.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var userId = ResolveUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var result = await _supportService.GetTicketRepliesAsync(userId.Value, id);
        if (result.IsFailure)
        {
            return RedirectToAction("Index");
        }

        return View(result.Value);
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    [RateLimit(MaxRequests = 5, TimeWindowMinutes = 1)]
    public async Task<IActionResult> Create([FromForm] CreateSupportTicketRequestDto request)
    {
        var userId = ResolveUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var result = await _supportService.CreateTicketAsync(userId.Value, request);

        if (result.IsFailure)
        {
            TempData["SupportError"] = string.Join("; ", result.Errors);
            return RedirectToAction("Index");
        }

        // ── Миттєве сповіщення адмінам через SignalR ──
        try
        {
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");

            foreach (var adminUser in adminUsers)
            {
                var domainAdmin = await _userRepository.GetByEmailAsync(adminUser.Email ?? string.Empty);
                if (domainAdmin == null) continue;

                var notification = new Notification
                {
                    UserId = domainAdmin.Id,
                    Title = "🆘 Нове звернення техпідтримки",
                    Message = $"Тема: {request.Subject}",
                    Type = "SupportRequest",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationRepository.AddAsync(notification);

                await _hubContext.Clients.User(adminUser.Id.ToString())
                    .SendAsync("ReceiveNotification", new
                    {
                        title = notification.Title,
                        message = notification.Message,
                        type = notification.Type,
                        createdAt = notification.CreatedAt
                    });
            }

            // Отримуємо створений тікет та позначаємо як "сповіщено", щоб BackgroundService не дублював
            var ticket = await _supportRepository.GetTicketWithRepliesByIdAsync(result.Value);
            if (ticket != null)
            {
                ticket.IsAdminNotified = true;
                await _supportRepository.UpdateTicketAsync(ticket);
            }

            _logger.LogInformation("📢 Сповіщення про нове звернення «{Subject}» надіслано адмінам", request.Subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при надсиланні сповіщення адмінам про нове звернення");
        }

        return RedirectToAction("Index");
    }

    [HttpPost("reply")]
    [ValidateAntiForgeryToken]
    [RateLimit(MaxRequests = 5, TimeWindowMinutes = 1)]
    public async Task<IActionResult> AddReply([FromForm] int ticketId, [FromForm] string message)
    {
        var userId = ResolveUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        if (string.IsNullOrWhiteSpace(message))
        {
            TempData["SupportError"] = "Коментар не може бути порожнім.";
            return RedirectToAction("Details", new { id = ticketId });
        }

        var result = await _supportService.AddUserReplyAsync(userId.Value, ticketId, message);
        if (result.IsFailure)
        {
            TempData["SupportError"] = string.Join("; ", result.Errors);
        }

        return RedirectToAction("Details", new { id = ticketId });
    }

    private int? ResolveUserId()
    {
        if (User?.Identity?.IsAuthenticated != true)
            return null;

        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (int.TryParse(idClaim, out var id))
            return id;

        return null;
    }
}
