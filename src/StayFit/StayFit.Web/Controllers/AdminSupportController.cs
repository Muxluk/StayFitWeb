using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Domain.Enums;

namespace StayFit.Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/support")]
public class AdminSupportController : BaseController
{
    private readonly ISupportService _supportService;

    public AdminSupportController(ISupportService supportService)
    {
        _supportService = supportService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(SupportStatus? statusFilter, int pageNumber = 1)
    {
        int pageSize = 10;
        var result = await _supportService.GetAdminTicketsAsync(statusFilter, pageNumber, pageSize);
        
        ViewBag.CurrentFilter = statusFilter;
        return View(result);
    }

    [HttpGet("details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var ticket = await _supportService.GetAdminTicketByIdAsync(id);
        if (ticket == null)
        {
            TempData["Error"] = "Звернення не знайдено.";
            return RedirectToAction(nameof(Index));
        }
        
        return View(ticket);
    }

    [HttpPost("change-status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, SupportStatus status)
    {
        var success = await _supportService.ChangeTicketStatusAsync(id, status);
        if (success) TempData["Success"] = "Статус успішно змінено.";
        else TempData["Error"] = "Помилка при зміні статусу.";

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("reply")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(SupportReplyDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Некоректні дані.";
            return RedirectToAction(nameof(Details), new { id = dto.TicketId });
        }

        var success = await _supportService.ReplyToTicketAsync(dto);
        if (success) TempData["Success"] = "Відповідь успішно надіслана. Звернення закрито.";
        else TempData["Error"] = "Помилка при збереженні відповіді.";

        return RedirectToAction(nameof(Details), new { id = dto.TicketId });
    }
}