using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;

namespace StayFit.Web.Controllers;

[Authorize]
public class FoodController : Controller
{
    private readonly IFoodService _foodService;

    public FoodController(IFoodService foodService)
    {
        _foodService = foodService;
    }

    // GET: Food
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
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
            var userId = GetCurrentUserId();
            food.CreatedByEmail = User.Identity?.Name;

            await _foodService.AddFoodAsync(food, userId);

            return RedirectToAction(nameof(Index));
        }

        return View(food);
    }

    // GET: Food/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var userId = GetCurrentUserId();
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
            var userId = GetCurrentUserId();

            await _foodService.UpdateFoodAsync(food, userId);

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
        try
        {
            var userId = GetCurrentUserId();

            await _foodService.DeleteFoodAsync(id, userId);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> AddToLog(int foodId, double quantity)
    {
        var userEmail = User.Identity?.Name;
        if (string.IsNullOrEmpty(userEmail)) 
        {
            return Challenge();
        }

        var log = new FoodLog
        {
            FoodId = foodId,
            Quantity = quantity,
            LogDate = DateTime.UtcNow,
            UserEmail = userEmail
        };

        await _foodService.AddFoodToLogAsync(log);
        
        return RedirectToAction("Index", "Diary");
    }
    private int GetCurrentUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userIdStr, out var userId) ? userId : 0;
    }
}
