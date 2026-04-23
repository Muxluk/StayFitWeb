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

    // GET: Food/Edit/5
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

    // POST: Food/Edit/5
    [HttpPost]
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

    [HttpPost]
    public async Task<IActionResult> AddToLog(int foodId, double quantity)
    {
        _logger.LogInformation("🍽️ AddToLog (FoodController) викликана. FoodId={FoodId}, Quantity={Quantity}g", foodId, quantity);

        var userEmail = User.Identity?.Name;
        if (string.IsNullOrEmpty(userEmail))
        {
            _logger.LogWarning("❌ UserEmail порожній");
            return Challenge();
        }

        await _foodService.AddFoodToLogAsync(foodId, quantity, userEmail);

        _logger.LogInformation("✅ AddToLog завершено FoodId={FoodId}, Email={Email}", foodId, userEmail);

        return RedirectToAction("Index", "Diary");
    }

}
