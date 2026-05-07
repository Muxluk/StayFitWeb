using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StayFit.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminProductController : BaseController
{
    private const int PageSize = 15;
    private readonly IProductModerationService _moderationService;

    public AdminProductController(IProductModerationService moderationService)
    {
        _moderationService = moderationService;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var result = await _moderationService.GetPendingProductsAsync();

        return result.Match(
            success =>
            {
                var all = success.Data.ToList();
                var totalPages = (int)Math.Ceiling(all.Count / (double)PageSize);
                var paged = all.Skip((page - 1) * PageSize).Take(PageSize).ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalCount = all.Count;

                return (IActionResult)View(paged);
            },
            failure =>
            {
                ModelState.AddModelError("", failure.ErrorMessage);
                return (IActionResult)View(new List<StayFit.Domain.Entities.Food>());
            }
        );
    }

    public async Task<IActionResult> Details(int id)
    {
        var result = await _moderationService.GetPendingProductsAsync();
        return result.Match(
            success =>
            {
                var item = success.Data.FirstOrDefault(f => f.Id == id);
                if (item == null) return (IActionResult)NotFound();
                return (IActionResult)View(item);
            },
            failure => (IActionResult)NotFound()
        );
    }

    [HttpPost]
    public async Task<IActionResult> Approve(int id)
    {
        await _moderationService.ApproveProductAsync(id);
        TempData["Success"] = "Продукт підтверджено.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Reject(int id)
    {
        await _moderationService.RejectProductAsync(id);
        TempData["Warning"] = "Продукт відхилено.";
        return RedirectToAction(nameof(Index));
    }
}