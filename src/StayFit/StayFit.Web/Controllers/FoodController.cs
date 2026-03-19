using Microsoft.AspNetCore.Mvc;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;

namespace StayFit.Web.Controllers;

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
        var foods = await _foodService.GetAllFoodsAsync();
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
            await _foodService.AddFoodAsync(food);
            return RedirectToAction(nameof(Index));
        }

        return View(food);
    }

    // GET: Food/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var food = await _foodService.GetFoodByIdAsync(id);
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
            await _foodService.UpdateFoodAsync(food);
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
            await _foodService.DeleteFoodAsync(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }
}
