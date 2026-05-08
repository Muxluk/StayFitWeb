using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Application.Common;
using StayFit.Web.Filters;

namespace StayFit.Web.Controllers;

[Authorize]
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
            return RedirectToAction("Index");
        }

        return View(result.Value);
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    [RateLimit(MaxRequests = 5, TimeWindowMinutes = 1)]
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

    [HttpPost("reply")]
    [ValidateAntiForgeryToken]
    [RateLimit(MaxRequests = 5, TimeWindowMinutes = 1)]
    public async Task<IActionResult> AddReply([FromForm] int ticketId, [FromForm] string message)
    {
        var userId = ResolveUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        if (string.IsNullOrWhiteSpace(message))
        {
            TempData["SupportError"] = "Коментар не може бути порожнім.";
            return RedirectToAction("Details", new { id = ticketId });
        }

        var result = await _supportService.AddUserReplyAsync(userId.Value, ticketId, message);
        if (result.IsFailure)
        {
            TempData["SupportError"] = string.Join("; ", result.Errors);
        }

        return RedirectToAction("Details", new { id = ticketId });
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
