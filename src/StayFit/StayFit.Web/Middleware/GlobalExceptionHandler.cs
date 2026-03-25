using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StayFit.Domain.Exceptions;
using System.Net;

namespace StayFit.Web.Middleware;

/// <summary>
/// Глобальний обробник винятків для всіх необроблених винятків у додатку.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Виняток типу {ExceptionType} з повідомленням: {Message}",
            exception.GetType().Name,
            exception.Message);

        var (statusCode, title, details) = MapException(exception);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = details,
            Instance = httpContext.Request.Path,
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title, string Details) MapException(Exception exception)
    {
        return exception switch
        {
            NotFoundException notFound => (
                StatusCode: (int)HttpStatusCode.NotFound,
                Title: "Ресурс не знайдено",
                Details: notFound.Message
            ),

            EmailSendException emailSend => (
                StatusCode: (int)HttpStatusCode.ServiceUnavailable,
                Title: "Помилка відправки email",
                Details: "Не вдалося відправити email. Спробуйте пізніше."
            ),

            InvalidTokenException invalidToken => (
                StatusCode: (int)HttpStatusCode.BadRequest,
                Title: "Невалідний токен",
                Details: invalidToken.Message
            ),

            InvalidOperationException or ArgumentNullException or ArgumentException => (
                StatusCode: (int)HttpStatusCode.BadRequest,
                Title: "Невалідний запит",
                Details: exception.Message
            ),

            KeyNotFoundException => (
                StatusCode: (int)HttpStatusCode.NotFound,
                Title: "Ресурс не знайдено",
                Details: exception.Message
            ),

            _ => (
                StatusCode: (int)HttpStatusCode.InternalServerError,
                Title: "Внутрішня помилка сервера",
                Details: "Сталася непередбачена помилка. Спробуйте пізніше."
            )
        };
    }
}
