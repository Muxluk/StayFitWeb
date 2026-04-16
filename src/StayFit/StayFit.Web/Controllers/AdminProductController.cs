using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StayFit.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminProductController : BaseController
{
    private readonly IProductModerationService _moderationService;

    public AdminProductController(IProductModerationService moderationService)
    {
        _moderationService = moderationService;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _moderationService.GetPendingProductsAsync();
        
        // Використовуємо твій Match, але замість HandleError просто повертаємо View
        return result.Match(
            success => View(success.Data),
            failure => {
                ModelState.AddModelError("", failure.ErrorMessage);
                return View(new List<StayFit.Domain.Entities.Food>());
            }
        );
    }

    [HttpPost]
    public async Task<IActionResult> Approve(int id)
    {
        var result = await _moderationService.ApproveProductAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Reject(int id)
    {
        var result = await _moderationService.RejectProductAsync(id);
        return RedirectToAction(nameof(Index));
    }
}