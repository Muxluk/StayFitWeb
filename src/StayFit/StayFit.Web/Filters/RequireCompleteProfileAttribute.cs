using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<RequireCompleteProfileFilter> _logger;

        public RequireCompleteProfileFilter(ILogger<RequireCompleteProfileFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // В даний час фільтр деактивований
            // Можна розширити в майбутньому при наявності придатного сервісу
            await next();
        }
    }
}
