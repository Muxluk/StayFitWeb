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
    public async Task<IActionResult> Index()
    {
        var currentUserId = GetCurrentUserId();
        var foods = await _foodService.GetAllFoodsAsync(currentUserId);
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
            var currentUserId = GetCurrentUserId();
            await _foodService.AddFoodAsync(food, currentUserId);
            return RedirectToAction(nameof(Index));
        }

        return View(food);
    }

    // GET: Food/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var currentUserId = GetCurrentUserId();
        var food = await _foodService.GetFoodByIdAsync(id, currentUserId);
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
            var currentUserId = GetCurrentUserId();
            var existingFood = await _foodService.GetFoodByIdAsync(id, currentUserId);
            if (existingFood == null)
            {
                return NotFound();
            }

            existingFood.Name = food.Name;
            existingFood.CaloriesPer100g = food.CaloriesPer100g;
            existingFood.ProteinPer100g = food.ProteinPer100g;
            existingFood.FatPer100g = food.FatPer100g;
            existingFood.CarbsPer100g = food.CarbsPer100g;

            await _foodService.UpdateFoodAsync(existingFood, currentUserId);
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
            var currentUserId = GetCurrentUserId();
            await _foodService.DeleteFoodAsync(id, currentUserId);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Неможливо визначити ідентифікатор користувача.");
        }

        return userId;
    }
}
