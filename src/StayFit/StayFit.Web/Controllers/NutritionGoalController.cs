using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Domain.Interfaces;

namespace StayFit.Web.Controllers;

[Authorize]
[Route("nutrition-goal")]
public class NutritionGoalController : BaseController
{
    private readonly INutritionGoalService _nutritionGoalService;
    private readonly IFoodService _foodService;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;

    public NutritionGoalController(
        INutritionGoalService nutritionGoalService, 
        IFoodService foodService,
        IUserRepository userRepository,
        INotificationService notificationService)
    {
        _nutritionGoalService = nutritionGoalService;
        _foodService = foodService;
        _userRepository = userRepository;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = GetRequiredCurrentUserId().ToString();
        var userEmail = GetCurrentUserEmailOrEmpty();

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

        var userId = GetRequiredCurrentUserId().ToString();
        var result = await _nutritionGoalService.SetGoalAsync(userId, dto);

        if (result.IsSuccess)
        {
            // Створити сповіщення про встановлення цілі
            var userEmail = GetCurrentUserEmailOrEmpty();
            if (!string.IsNullOrEmpty(userEmail))
            {
                var user = await _userRepository.GetByEmailAsync(userEmail);
                if (user != null)
                {
                    await _notificationService.CreateNutritionGoalSetNotificationAsync(user.Id, (int)dto.CaloriesGoal);
                }
            }

            TempData["SuccessMessage"] = "Цілі збережено!";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, result.Errors?.FirstOrDefault() ?? "Помилка");
        return await Index();
    }
}