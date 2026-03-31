using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StayFit.Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/users")]
public class AdminUserController : BaseController
{
    [HttpGet("")]
    public IActionResult Index()
    {
        // Temporary endpoint while admin user service is being added.
        return View();
    }
}
