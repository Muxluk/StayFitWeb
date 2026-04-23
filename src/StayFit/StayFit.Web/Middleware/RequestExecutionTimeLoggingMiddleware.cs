using System.Diagnostics;

namespace StayFit.Web.Middleware;

/// <summary>
/// Логує час виконання HTTP-запиту.
/// </summary>
public sealed class RequestExecutionTimeLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestExecutionTimeLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            logger.LogInformation(
                "RequestTiming: Method={Method}, Path={Path}, StatusCode={StatusCode}, ElapsedMs={ElapsedMs:0.000}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.Elapsed.TotalMilliseconds);
        }
    }
}
