using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Concurrent;

namespace StayFit.Web.Filters;

/// <summary>
/// Action filter для обмеження кількості запитів з однієї IP-адреси
/// за визначений проміжок часу.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class RateLimitAttribute : TypeFilterAttribute
{
    public RateLimitAttribute(int maxRequests, int timeWindowMinutes)
        : base(typeof(RateLimitFilter))
    {
        MaxRequests = maxRequests;
        TimeWindowMinutes = timeWindowMinutes;
        Arguments = [maxRequests, timeWindowMinutes];
    }

    public int MaxRequests { get; }

    public int TimeWindowMinutes { get; }

    private sealed class RateLimitFilter : IActionFilter
    {
        private static readonly ConcurrentDictionary<string, List<DateTime>> RequestLog = new();
        private readonly int _maxRequests;
        private readonly TimeSpan _timeWindow;

        public RateLimitFilter(int maxRequests, int timeWindowMinutes)
        {
            _maxRequests = maxRequests;
            _timeWindow = TimeSpan.FromMinutes(timeWindowMinutes);
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (_maxRequests <= 0 || _timeWindow <= TimeSpan.Zero)
            {
                return;
            }

            var ipAddress = GetClientIp(context.HttpContext);
            var now = DateTime.UtcNow;

            var requests = RequestLog.GetOrAdd(ipAddress, _ => []);

            lock (requests)
            {
                requests.RemoveAll(requestTime => now - requestTime > _timeWindow);

                if (requests.Count >= _maxRequests)
                {
                    context.Result = new RedirectResult("/error/rate-limited");
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
}
