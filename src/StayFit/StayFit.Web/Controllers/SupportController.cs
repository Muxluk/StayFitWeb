using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Application.Common;

namespace StayFit.Web.Controllers;

[Route("support")]
public class SupportController : Controller
{
    private readonly ISupportService _supportService;

    public SupportController(ISupportService supportService)
    {
        _supportService = supportService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var userId = ResolveUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var result = await _supportService.GetMyTicketsAsync(userId.Value);
        if (result.IsFailure)
        {
            // surface empty list on failure (service logs the error)
            return View(Enumerable.Empty<SupportTicketDto>());
        }

        return View(result.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var userId = ResolveUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var result = await _supportService.GetTicketRepliesAsync(userId.Value, id);
        if (result.IsFailure)
        {
            return View("Details", Enumerable.Empty<object>());
        }

        return View(result.Value);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] CreateSupportTicketRequestDto request)
    {
        var userId = ResolveUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var result = await _supportService.CreateTicketAsync(userId.Value, request);

        if (result.IsFailure)
        {
            TempData["SupportError"] = string.Join("; ", result.Errors);
            return RedirectToAction("Index");
        }

        return RedirectToAction("Index");
    }

    private int? ResolveUserId()
    {
        if (User?.Identity?.IsAuthenticated != true)
            return null;

        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (int.TryParse(idClaim, out var id))
            return id;

        return null;
    }
}
