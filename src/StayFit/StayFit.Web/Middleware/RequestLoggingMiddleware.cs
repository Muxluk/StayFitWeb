using System.Security.Claims;
using System.Text;

namespace StayFit.Web.Middleware;

/// <summary>
/// Логує базову інформацію про вхідний HTTP-запит.
/// </summary>
public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    private const int MaxLoggedBodyLength = 8_192;

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        var method = request.Method;
        var url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
        var ipAddress = ResolveIpAddress(context);
        var userId = ResolveUserId(context);
        var headers = BuildHeadersSnapshot(request.Headers);
        var body = await ReadRequestBodyAsync(request);
        var headersText = FormatHeaders(headers);
        var bodyText = FormatBody(body);
        var details = $"HTTP Request{Environment.NewLine}"
            + $"  Method: {method}{Environment.NewLine}"
            + $"  Url: {url}{Environment.NewLine}"
            + $"  Ip: {ipAddress}{Environment.NewLine}"
            + $"  UserId: {userId ?? "null"}{Environment.NewLine}"
            + $"  Headers:{Environment.NewLine}{headersText}{Environment.NewLine}"
            + $"  Body:{Environment.NewLine}{bodyText}";

        logger.LogInformation("{RequestDetails}", details);

        await next(context);
    }

    private static string ResolveIpAddress(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static string? ResolveUserId(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub");
    }

    private static Dictionary<string, string> BuildHeadersSnapshot(IHeaderDictionary headers)
    {
        var snapshot = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var header in headers)
        {
            var value = string.Join(",", header.Value.ToArray());
            snapshot[header.Key] = IsSensitiveHeader(header.Key) ? "***" : value;
        }

        return snapshot;
    }

    private static bool IsSensitiveHeader(string headerName) =>
        headerName.Equals("Authorization", StringComparison.OrdinalIgnoreCase)
        || headerName.Equals("Cookie", StringComparison.OrdinalIgnoreCase)
        || headerName.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase);

    private static string FormatHeaders(Dictionary<string, string> headers)
    {
        if (headers.Count == 0)
        {
            return "    (none)";
        }

        var builder = new StringBuilder();

        foreach (var header in headers.OrderBy(h => h.Key, StringComparer.OrdinalIgnoreCase))
        {
            builder.Append("    - ")
                .Append(header.Key)
                .Append(": ")
                .AppendLine(header.Value);
        }

        return builder.ToString().TrimEnd();
    }

    private static string FormatBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return "    (empty)";
        }

        var lines = body.Replace("\r\n", "\n").Split('\n');
        var builder = new StringBuilder();

        foreach (var line in lines)
        {
            builder.Append("    ").AppendLine(line);
        }

        return builder.ToString().TrimEnd();
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        if (request.ContentLength is null or 0)
        {
            return string.Empty;
        }

        if (request.ContentType?.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "[multipart/form-data omitted]";
        }

        request.EnableBuffering();
        request.Body.Position = 0;

        using var reader = new StreamReader(
            request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        if (body.Length <= MaxLoggedBodyLength)
        {
            return body;
        }

        return $"{body[..MaxLoggedBodyLength]}...[truncated]";
    }
}
