using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StayFit.Application.Interfaces;
using System.Security.Claims;

namespace StayFit.Web.Filters;

/// <summary>
/// Action Filter для перевірки заповненості профілю користувача
/// Якщо профіль не заповнений, перенаправляє на сторінку налаштування
/// </summary>
public class RequireCompleteProfileAttribute : TypeFilterAttribute
{
    public RequireCompleteProfileAttribute() : base(typeof(RequireCompleteProfileFilter))
    {
    }

    private class RequireCompleteProfileFilter : IAsyncActionFilter
    {
        private readonly IProfileSetupService _profileSetupService;
        private readonly ILogger<RequireCompleteProfileFilter> _logger;

        public RequireCompleteProfileFilter(
            IProfileSetupService profileSetupService,
            ILogger<RequireCompleteProfileFilter> logger)
        {
            _profileSetupService = profileSetupService;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Пропустити перевірку для сторінки налаштування профілю та для неавторизованих користувачів
            var routeValues = context.RouteData.Values;
            var controller = routeValues["controller"]?.ToString() ?? "";
            
            if (controller == "Profile" || controller == "Account" || controller == "Error" || controller == "AccountSecurity")
            {
                await next();
                return;
            }

            // Отримати ID користувача з Claims
            var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int parsedUserId))
            {
                // Користувач не авторизований, пропустити фільтр
                await next();
                return;
            }

            try
            {
                // Перевірити, чи заповнений профіль
                var isProfileComplete = await _profileSetupService.IsProfileCompleteAsync(parsedUserId);

                if (!isProfileComplete)
                {
                    _logger.LogInformation(
                        "Профіль користувача {UserId} не заповнений. Перенаправлення на налаштування профілю",
                        parsedUserId);

                    // Перенаправити на сторінку редагування профіля
                    context.Result = new RedirectToActionResult("Edit", "Profile", null);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при перевірці заповненості профілю для користувача {UserId}", parsedUserId);
                // Продовжити виконання дії в разі помилки (не блокувати користувача)
            }

            await next();
        }
    }
}
