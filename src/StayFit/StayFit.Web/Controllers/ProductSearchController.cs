using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.Interfaces;
using StayFit.Domain.Enums;
using System.Security.Claims;

namespace StayFit.Web.Controllers;

[Authorize]
public class ProductSearchController : Controller
{
    private readonly IProductSearchService _productSearchService;

    public ProductSearchController(IProductSearchService productSearchService)
    {
        _productSearchService = productSearchService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? searchTerm, FoodCategory? category, int page = 1)
    {
        var userId = GetCurrentUserId();

        // Показуємо 6 продуктів на сторінку (2 ряди по 3 картки)
        var result = await _productSearchService.SearchAsync(searchTerm, category, page, 6, userId);

        if (result.IsFailure)
        {
            // Optional: Handle failure (e.g. invalid page number)
            // Could redirect to page 1 or show error in TempData
            ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault() ?? "Error occurred");
            return View(); // Either an empty view or fallback
        }

        ViewBag.SearchTerm = searchTerm;
        ViewBag.CategoryFilter = category;

        return View(result.Value);
    }

    private int GetCurrentUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdStr, out var userId) ? userId : 0;
    }
}
