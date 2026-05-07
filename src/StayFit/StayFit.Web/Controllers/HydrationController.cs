using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StayFit.Web.Controllers;

[Authorize]
[Route("hydration")]
public class HydrationController : BaseController
{
    private readonly ILogger<HydrationController> _logger;

    public HydrationController(ILogger<HydrationController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        _logger.LogInformation("Користувач відвідав сторінку водного балансу");
        return View();
    }

    [HttpPost("add")]
    public IActionResult AddWaterIntake([FromBody] AddWaterRequest request)
    {
        _logger.LogInformation("Користувач додав {Glasses} склянок води", request.Glasses);

        if (request.Glasses <= 0 || request.Glasses > 20)
        {
            return BadRequest(new { message = "Невірна кількість склянок" });
        }

        return Ok(new { message = $"Додано {request.Glasses} склянок(и) води" });
    }

    public class AddWaterRequest
    {
        public int Glasses { get; set; }
    }
}
