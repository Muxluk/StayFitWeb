using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Web.Controllers;

[Authorize]
public class FoodController : BaseController
{
    private readonly IFoodService _foodService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<FoodController> _logger;

    public FoodController(IFoodService foodService, IUserRepository userRepository, ILogger<FoodController> logger)
    {
        _foodService = foodService;
        _userRepository = userRepository;
        _logger = logger;
    }

    // GET: Food
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserIdOrDefault();
        var foods = await _foodService.GetAllFoodsAsync(userId);

        return View(foods);
    }

    // GET: Food/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Food/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Food food)
    {
        if (ModelState.IsValid)
        {
            var userId = GetCurrentUserIdOrDefault();
            food.CreatedByEmail = User.Identity?.Name;

            await _foodService.AddFoodAsync(food, userId);

            return RedirectToAction(nameof(Index));
        }

        return View(food);
    }

    // GET: Food/{id}/edit
    [HttpGet("Food/{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = GetCurrentUserIdOrDefault();
        var food = await _foodService.GetFoodByIdAsync(id, userId);

        if (food == null)
        {
            return NotFound();
        }

        return View(food);
    }

    // POST: Food/{id}/edit
    [HttpPost("Food/{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Food food)
    {
        if (id != food.Id)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            var userId = GetCurrentUserIdOrDefault();

            await _foodService.UpdateFoodAsync(food, userId);
            _logger.LogInformation("Продукт з ID {FoodId} оновлено користувачем {UserId}", id, userId);

            return RedirectToAction(nameof(Index));
        }

        return View(food);
    }

    // POST: Food/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = GetCurrentUserIdOrDefault();

        await _foodService.DeleteFoodAsync(id, userId);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> AddToLog(int? foodId)
    {
        if (foodId.HasValue)
        {
            var userId = GetCurrentUserIdOrDefault();
            var food = await _foodService.GetFoodByIdAsync(foodId.Value, userId);
            if (food != null)
                ViewBag.SelectedFood = food;
        }
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> SearchJson(string? q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Json(Array.Empty<object>());

        var userId = GetCurrentUserIdOrDefault();
        var all = await _foodService.GetAllFoodsAsync(userId);
        var filtered = all
            .Where(f => f.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
            .Take(20)
            .Select(f => new {
                f.Id,
                f.Name,
                f.CaloriesPer100g,
                f.ProteinPer100g,
                f.FatPer100g,
                f.CarbsPer100g,
                Category = f.Category.ToString()
            });
        return Json(filtered);
    }

    [HttpPost]
    public async Task<IActionResult> AddToLog(int foodId, double quantity, string? mealType)
    {
        _logger.LogInformation("🍽️ AddToLog (FoodController) викликана. FoodId={FoodId}, Quantity={Quantity}g, MealType={MealType}", foodId, quantity, mealType);

        var userEmail = User.Identity?.Name;
        if (string.IsNullOrEmpty(userEmail))
        {
            _logger.LogWarning("❌ UserEmail порожній");
            return Challenge();
        }

        // Map meal type to a specific hour of today so diary grouping works
        var today = DateTime.Today;
        DateTime? loggedAt = mealType?.ToLowerInvariant() switch
        {
            "breakfast" => today.AddHours(8),
            "lunch"     => today.AddHours(13),
            "dinner"    => today.AddHours(19),
            "snack"     => today.AddHours(23),
            _           => null
        };

        await _foodService.AddFoodToLogAsync(foodId, quantity, userEmail, loggedAt);

        _logger.LogInformation("✅ AddToLog завершено FoodId={FoodId}, Email={Email}", foodId, userEmail);

        return RedirectToAction("Index", "Diary");
    }

}
