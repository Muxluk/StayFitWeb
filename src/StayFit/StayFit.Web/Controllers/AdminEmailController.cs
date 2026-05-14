using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Identity;

namespace StayFit.Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/email")]
public class AdminEmailController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRepository<EmailBroadcast> _broadcastRepository;
    private readonly ILogger<AdminEmailController> _logger;

    public AdminEmailController(
        IUserRepository userRepository,
        IEmailSender emailSender,
        UserManager<ApplicationUser> userManager,
        IRepository<EmailBroadcast> broadcastRepository,
        ILogger<AdminEmailController> logger)
    {
        _userRepository = userRepository;
        _emailSender = emailSender;
        _userManager = userManager;
        _broadcastRepository = broadcastRepository;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Адмін переглядає форму розсилки");

        var allBroadcasts = (await _broadcastRepository.GetAllAsync())
            .OrderByDescending(b => b.SentAt)
            .ToList();

        var viewModel = new AdminEmailViewModel
        {
            History = allBroadcasts,
            TotalUsers = (await _userRepository.GetAllAsync()).Count()
        };

        return View("Index", viewModel);
    }

    [HttpGet("create")]
    [HttpGet("~/AdminEmail/Create")]
    public IActionResult Create()
    {
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("history")]
    [HttpGet("~/AdminEmail/History")]
    public IActionResult History()
    {
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("send")]
    [HttpPost("~/AdminEmail/Send")]
    public async Task<IActionResult> SendBroadcast([FromBody] SendEmailBroadcastDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _logger.LogInformation("Адмін розпочав розсилку: {Subject}", request.Subject);

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

            var allUsers = await _userRepository.GetAllAsync();
            var failedCount = 0;

            // Запис у БД
            var broadcast = new EmailBroadcast
            {
                AdminUserId = currentUser.Id,
                Subject = request.Subject,
                HtmlBody = request.HtmlBody,
                RecipientCount = allUsers.Count(),
                SentAt = DateTime.UtcNow,
                Status = "Pending"
            };

            foreach (var user in allUsers)
            {
                try
                {
                    await _emailSender.SendAsync(
                        user.Email,
                        request.Subject,
                        request.HtmlBody,
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Помилка при відправленні листа користувачу {Email}", user.Email);
                    failedCount++;
                }
            }

            broadcast.Status = failedCount == 0 ? "Sent" : "PartiallyFailed";
            await _broadcastRepository.AddAsync(broadcast);

            var successCount = allUsers.Count() - failedCount;
            _logger.LogInformation(
                "Розсилка завершена. Успішно: {SuccessCount}, Помилок: {FailedCount}",
                successCount,
                failedCount);

            return Json(new
            {
                success = true,
                message = $"Розсилка завершена. Отримано {successCount} листів (помилок: {failedCount})",
                recipientCount = allUsers.Count(),
                sentCount = successCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критична помилка при розсилці");
            return StatusCode(500, new { success = false, message = "Помилка сервера" });
        }
    }
}

public class AdminEmailViewModel
{
    public IEnumerable<EmailBroadcast> History { get; set; } = new List<EmailBroadcast>();
    public int TotalUsers { get; set; }
}

public class SendEmailBroadcastDto
{
    public required string Subject { get; set; }
    public required string HtmlBody { get; set; }
}
