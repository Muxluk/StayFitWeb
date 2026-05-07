using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;

namespace StayFit.Web.Controllers;

[Authorize]
public class FoodImportController : BaseController
{
    private readonly IFoodImportService _foodImportService;

    public FoodImportController(IFoodImportService foodImportService)
    {
        _foodImportService = foodImportService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm)
    {
        ViewBag.SearchTerm = searchTerm;

        IEnumerable<Food> results = Array.Empty<Food>();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            results = await _foodImportService.SearchGlobalAsync(searchTerm);

            if (results.Any())
            {
                var existingNames = await _foodImportService.GetExistingNamesAsync(results.Select(r => r.Name));
                ViewBag.ExistingNames = existingNames;
            }
        }

        return View(results);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(Food importedFood)
    {
        var userId = GetCurrentUserIdOrDefault();
        var userEmail = User.Identity?.Name ?? "Unknown";
        
        await _foodImportService.ImportProductAsync(importedFood, userId, userEmail);
        
        // Redirect back to generic product search
        return RedirectToAction("Index", "ProductSearch", new { searchTerm = importedFood.Name });
    }
}
