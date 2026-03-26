using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;

namespace StayFit.Web.Controllers;

[Authorize]
[Route("nutrition-goal")]
public class NutritionGoalController : Controller
{
    private readonly INutritionGoalService _nutritionGoalService;
    private readonly IFoodService _foodService;

    public NutritionGoalController(
        INutritionGoalService nutritionGoalService, 
        IFoodService foodService)
    {
        _nutritionGoalService = nutritionGoalService;
        _foodService = foodService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var userEmail = User.Identity?.Name ?? "";

        var goalResult = await _nutritionGoalService.GetGoalAsync(userId);
        var logs = await _foodService.GetDailyLogAsync(userEmail, DateTime.Today);

        var model = new SetNutritionGoalDto();
        if (goalResult.IsSuccess && goalResult.Value != null)
        {
            model.CaloriesGoal = goalResult.Value.CaloriesGoal;
            model.ProteinGoal = goalResult.Value.ProteinGoal;
            model.FatGoal = goalResult.Value.FatGoal;
            model.CarbsGoal = goalResult.Value.CarbsGoal;
            ViewBag.Goal = goalResult.Value;
        }

        ViewBag.CurrentNutrition = logs;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(SetNutritionGoalDto dto)
    {
        if (!ModelState.IsValid) return await Index(); 

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _nutritionGoalService.SetGoalAsync(userId, dto);

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Цілі збережено!";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, result.Errors?.FirstOrDefault() ?? "Помилка");
        return await Index();
    }
}