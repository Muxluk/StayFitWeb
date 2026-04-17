using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StayFit.Application.Interfaces;
using StayFit.Infrastructure.Identity;

namespace StayFit.Web.Filters;

/// <summary>
/// Глобальний фільтр перевірки валідності сеансу користувача.
/// Звіряє токен сеансу з базою даних. Якщо сеанс завершено дистанційно,
/// користувача автоматично "викидає" з поточного акаунта.
/// </summary>
public class SessionValidationFilter : IAsyncActionFilter
{
    private readonly ISessionService _sessionService;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public SessionValidationFilter(ISessionService sessionService, SignInManager<ApplicationUser> signInManager)
    {
        _sessionService = sessionService;
        _signInManager = signInManager;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Перевіряємо тільки якщо Identity вважає, що користувач залогінений
        if (context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            // Отримуємо унікальний токен цього браузера/пристрою
            var token = context.HttpContext.Request.Cookies["SessionToken"];
            
            // Звіряємо токен з БД (чи активний він і чи не прострочений)
            bool isValid = !string.IsNullOrEmpty(token) && await _sessionService.IsSessionValidAsync(token);

            if (!isValid)
            {
                // Видаляємо куки Identity та нашого сеансу
                await _signInManager.SignOutAsync();
                context.HttpContext.Response.Cookies.Delete("SessionToken");

                // Якщо це AJAX-запит, повертаємо 401
                if (context.HttpContext.Request.Headers.XRequestedWith == "XMLHttpRequest")
                {
                    context.Result = new UnauthorizedResult();
                }
                else
                {
                    // Інакше редиректимо на сторінку логіну
                    context.Result = new RedirectToActionResult("Login", "Account", null);
                }
                
                return; // Зупиняємо виконання контролера
            }
        }

        // Продовжуємо виконання запиту
        await next();
    }
}
