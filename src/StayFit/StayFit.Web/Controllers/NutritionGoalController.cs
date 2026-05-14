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
    private readonly IHydrationService _hydrationService; // Додано
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    
    public NutritionGoalController(
        INutritionGoalService nutritionGoalService, 
        IFoodService foodService,
        IHydrationService hydrationService, // Додано
        IUserRepository userRepository,
        INotificationService notificationService)
    {
        _nutritionGoalService = nutritionGoalService;
        _foodService = foodService;
        _hydrationService = hydrationService; // Додано
        _userRepository = userRepository;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userIdInt = GetRequiredCurrentUserId();
        var userEmail = GetCurrentUserEmailOrEmpty();
        var domainUser = !string.IsNullOrEmpty(userEmail) ? await _userRepository.GetByEmailAsync(userEmail) : null;
        var userIdStr = domainUser?.Id.ToString() ?? userIdInt.ToString();

        // Отримання цілей КБЖВ
        var goalResult = await _nutritionGoalService.GetGoalAsync(userIdStr);
        
        // Отримання логів їжі
        var logs = await _foodService.GetDailyLogAsync(userEmail, DateTime.Today);
        
        // Отримання прогресу води
        var waterResult = await _hydrationService.GetTodayProgressAsync(userIdInt);
        ViewBag.WaterProgress = waterResult.IsSuccess ? waterResult.Value : new HydrationProgressDto();

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

        var userEmail = GetCurrentUserEmailOrEmpty();
        var domainUser = !string.IsNullOrEmpty(userEmail) ? await _userRepository.GetByEmailAsync(userEmail) : null;
        var userIdStr = domainUser?.Id.ToString() ?? GetRequiredCurrentUserId().ToString();
        var result = await _nutritionGoalService.SetGoalAsync(userIdStr, dto);

        if (result.IsSuccess)
        {
            if (domainUser != null)
            {
                await _notificationService.CreateNutritionGoalSetNotificationAsync(domainUser.Id, (int)dto.CaloriesGoal);
            }

            TempData["SuccessMessage"] = "Цілі збережено!";
            return RedirectToAction(nameof(Index));
        }

        // Використовуємо .Errors, як у твоїй логіці Result
        ModelState.AddModelError(string.Empty, result.Errors?.FirstOrDefault() ?? "Помилка при збереженні");
        return await Index();
    }
}