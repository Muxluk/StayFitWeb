using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StayFit.Application.Configuration;
using StayFit.Application.Interfaces;
using StayFit.Domain.Enums;

namespace StayFit.Web.Controllers;

[Authorize]
public class ProductSearchController : BaseController
{
    private readonly IProductSearchService _productSearchService;
    private readonly IOptions<PaginationSettings> _paginationSettings;

    public ProductSearchController(
        IProductSearchService productSearchService,
        IOptions<PaginationSettings> paginationSettings)
    {
        _productSearchService = productSearchService;
        _paginationSettings = paginationSettings;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? searchTerm, FoodCategory? category, int page = 1)
    {
        var userId = GetCurrentUserIdOrDefault();

        // Використовуємо розмір сторінки з конфіга
        var pageSize = _paginationSettings.Value.DefaultPageSize;

        var result = await _productSearchService.SearchAsync(searchTerm, category, page, pageSize, userId);

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
}
