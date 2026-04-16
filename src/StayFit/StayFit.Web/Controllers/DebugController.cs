using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StayFit.Application.Options;
using StayFit.Web.Models;

namespace StayFit.Web.Controllers;

[AllowAnonymous]
[Route("debug")]
public class DebugController : Controller
{
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly IOptions<DashboardSettings> _dashboardOptions;
    private readonly IConfiguration _configuration;

    public DebugController(
        IWebHostEnvironment hostEnvironment,
        IOptions<DashboardSettings> dashboardOptions,
        IConfiguration configuration)
    {
        _hostEnvironment = hostEnvironment;
        _dashboardOptions = dashboardOptions;
        _configuration = configuration;
    }

    [HttpGet("config")]
    public IActionResult Config()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        var runtimeUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        var model = new DebugConfigViewModel
        {
            EnvironmentName = _hostEnvironment.EnvironmentName,
            RecentDiaryEntriesCount = _dashboardOptions.Value.RecentDiaryEntriesCount,
            ConfiguredBaseUrl = _configuration["App:BaseUrl"] ?? string.Empty,
            RuntimeUrl = runtimeUrl,
            HasDefaultConnectionString = !string.IsNullOrWhiteSpace(connectionString)
        };

        return View(model);
    }
}