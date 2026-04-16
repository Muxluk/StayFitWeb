namespace StayFit.Web.Models;

public sealed class HttpErrorViewModel
{
    public int StatusCode { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
