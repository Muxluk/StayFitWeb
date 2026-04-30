using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StayFit.Application.Interfaces;
using StayFit.Web.Filters;

namespace StayFit.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminEmailController : Controller
    {
        private readonly IEmailBroadcastService _broadcastService;
        private readonly ILogger<AdminEmailController> _logger;

        public AdminEmailController(
            IEmailBroadcastService broadcastService,
            ILogger<AdminEmailController> logger)
        {
            _broadcastService = broadcastService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RateLimit(MaxRequests = 3, TimeWindowMinutes = 1)]
        public async Task<IActionResult> Send(string subject, string body, string audience)
        {
            var adminId = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(adminId))
            {
                return Unauthorized();
            }

            try
            {
                await _broadcastService.SendBroadcastAsync(adminId, subject, body, audience);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send admin email broadcast");
                TempData["AdminEmailError"] = "Не вдалося виконати розсилку. Перевірте SMTP налаштування та стан БД.";
                return RedirectToAction(nameof(Create));
            }

            TempData["AdminEmailSuccess"] = "Розсилку успішно відправлено.";
            return RedirectToAction("History");
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            try
            {
                var history = await _broadcastService.GetHistoryAsync();
                return View(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load admin email broadcast history");
                TempData["AdminEmailError"] = "Не вдалося завантажити історію розсилок. Перевірте, чи застосована міграція БД.";
                return View(Array.Empty<StayFit.Domain.Entities.EmailBroadcast>());
            }
        }
    }
}
