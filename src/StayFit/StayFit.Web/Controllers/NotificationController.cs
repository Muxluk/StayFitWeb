using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.Interfaces;
using StayFit.Domain.Interfaces;

namespace StayFit.Web.Controllers;

[Authorize]
[Route("notifications")]
public class NotificationController : BaseController
{
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        INotificationService notificationService,
        IUserRepository userRepository,
        ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _userRepository = userRepository;
        _logger = logger;
    }

    private async Task<int?> GetDomainUserIdAsync()
    {
        var email = GetCurrentUserEmailOrEmpty();
        if (string.IsNullOrEmpty(email)) return null;
        var user = await _userRepository.GetByEmailAsync(email);
        return user?.Id;
    }

    /// <summary>
    /// Отримати всі непрочитані сповіщення користувача
    /// </summary>
    [HttpGet]
    [Route("unread")]
    public async Task<IActionResult> GetUnreadNotifications()
    {
        try
        {
            var userId = await GetDomainUserIdAsync();
            if (userId == null) return Challenge();
            _logger.LogInformation("Користувач {UserId} запросив непрочитані сповіщення", userId);

            var result = await _notificationService.GetUnreadNotificationsAsync(userId.Value);

            if (result.IsFailure)
            {
                _logger.LogWarning("Помилка при отриманні сповіщень користувача {UserId}: {Errors}",
                    userId, string.Join(", ", result.Errors));
                return BadRequest(result.Errors);
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні непрочитаних сповіщень");
            return StatusCode(500, "Сталася помилка при отриманні сповіщень");
        }
    }

    /// <summary>
    /// Отримати кількість непрочитаних сповіщень
    /// </summary>
    [HttpGet]
    [Route("unread-count")]
    public async Task<IActionResult> GetUnreadNotificationsCount()
    {
        try
        {
            var userId = await GetDomainUserIdAsync();
            if (userId == null) return Challenge();
            var result = await _notificationService.GetUnreadCountAsync(userId.Value);

            if (result.IsFailure)
            {
                _logger.LogWarning("Помилка при отриманні кількості сповіщень користувача {UserId}", userId);
                return BadRequest(result.Errors);
            }

            return Ok(new { count = result.Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні кількості непрочитаних сповіщень");
            return StatusCode(500, "Сталася помилка при отриманні кількості сповіщень");
        }
    }

    /// <summary>
    /// Позначити сповіщення як прочитане
    /// </summary>
    [HttpPost]
    [Route("{id:int}/mark-as-read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        try
        {
            var userId = await GetDomainUserIdAsync();
            if (userId == null) return Challenge();
            _logger.LogInformation("Користувач {UserId} позначив сповіщення {NotificationId} як прочитане", userId, id);

            var result = await _notificationService.MarkAsReadAsync(id, userId.Value);

            if (result.IsFailure)
            {
                _logger.LogWarning("Помилка при позначенні сповіщення {NotificationId} як прочитаного: {Errors}", 
                    id, string.Join(", ", result.Errors));
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "Сповіщення позначено як прочитане" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при позначенні сповіщення {NotificationId} як прочитаного", id);
            return StatusCode(500, "Сталася помилка при позначенні сповіщення");
        }
    }

    /// <summary>
    /// Позначити всі сповіщення користувача як прочитані
    /// </summary>
    [HttpPost]
    [Route("mark-all-as-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = await GetDomainUserIdAsync();
            if (userId == null) return Challenge();
            _logger.LogInformation("Користувач {UserId} позначив всі сповіщення як прочитані", userId);

            var result = await _notificationService.MarkAllAsReadAsync(userId.Value);

            if (result.IsFailure)
            {
                _logger.LogWarning("Помилка при позначенні всіх сповіщень як прочитаних: {Errors}", 
                    string.Join(", ", result.Errors));
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "Всі сповіщення позначено як прочитані" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при позначенні всіх сповіщень як прочитаних");
            return StatusCode(500, "Сталася помилка при позначенні всіх сповіщень");
        }
    }

    /// <summary>
    /// Очистити все сповіщення користувача
    /// </summary>
    [HttpPost]
    [Route("clear-all")]
    public async Task<IActionResult> ClearAllNotifications()
    {
        try
        {
            var userId = await GetDomainUserIdAsync();
            if (userId == null) return Challenge();
            _logger.LogInformation("Користувач {UserId} очистив всі сповіщення", userId);

            var result = await _notificationService.ClearAllNotificationsAsync(userId.Value);

            if (result.IsFailure)
            {
                _logger.LogWarning("Помилка при очищенні сповіщень користувача {UserId}: {Errors}", 
                    userId, string.Join(", ", result.Errors));
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "Всі сповіщення очищено" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при очищенні всіх сповіщень");
            return StatusCode(500, "Сталася помилка при очищенні сповіщень");
        }
    }
}
