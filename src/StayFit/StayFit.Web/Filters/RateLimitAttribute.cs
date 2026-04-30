using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Concurrent;

namespace StayFit.Web.Filters;

/// <summary>
/// Action filter для обмеження кількості запитів з однієї IP-адреси
/// за визначений проміжок часу.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class RateLimitAttribute : Attribute, IActionFilter
{
    private static readonly ConcurrentDictionary<string, List<DateTime>> RequestLog = new();

    public int MaxRequests { get; set; }

    public int TimeWindowMinutes { get; set; }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (MaxRequests <= 0 || TimeWindowMinutes <= 0)
        {
            return;
        }

        var timeWindow = TimeSpan.FromMinutes(TimeWindowMinutes);
        var ipAddress = GetClientIp(context.HttpContext);
        var now = DateTime.UtcNow;

        var requests = RequestLog.GetOrAdd(ipAddress, _ => []);

        lock (requests)
        {
            requests.RemoveAll(requestTime => now - requestTime > timeWindow);

            if (requests.Count >= MaxRequests)
            {
                context.Result = new Microsoft.AspNetCore.Mvc.RedirectResult("/error/rate-limited");
                return;
            }

            requests.Add(now);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    private static string GetClientIp(HttpContext httpContext)
    {
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
