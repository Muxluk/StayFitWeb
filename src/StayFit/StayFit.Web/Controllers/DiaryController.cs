using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StayFit.Application.Interfaces;
using StayFit.Application.Services;
using StayFit.Domain.Interfaces;
using StayFit.Web.Models;

namespace StayFit.Web.Controllers;

[Authorize]
public class DiaryController : BaseController
{
    private readonly IFoodService _foodService;
    private readonly IMealRepository _mealRepository;
    private readonly IFoodLogRepository _foodLogRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly QuickAddService _quickAddService;
    private readonly DiaryNoteService _diaryNoteService;
    private readonly ILogger<DiaryController> _logger;

    public DiaryController(
        IFoodService foodService,
        IMealRepository mealRepository,
        IFoodLogRepository foodLogRepository,
        IUserRepository userRepository,
        INotificationService notificationService,
        QuickAddService quickAddService,
        DiaryNoteService diaryNoteService,
        ILogger<DiaryController> logger)
    {
        _foodService = foodService;
        _mealRepository = mealRepository;
        _foodLogRepository = foodLogRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _quickAddService = quickAddService;
        _diaryNoteService = diaryNoteService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(DateTime? date)
    {
        var selectedDate = date ?? DateTime.Today;
        var userEmail = GetCurrentUserEmailOrEmpty();

        var logs = await _foodService.GetDailyLogAsync(userEmail, selectedDate);

        ViewBag.SelectedDate = selectedDate;
        return View(logs);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteEntry(int id)
    {
        _logger.LogInformation("🗑️ DeleteEntry викликана. LogId={LogId}", id);

        var log = await _foodLogRepository.GetByIdAsync(id);
        var foodName = log?.Food?.Name ?? "Продукт";
        _logger.LogInformation("📋 FoodName={FoodName}", foodName);

        await _foodService.RemoveFromLogAsync(id);
        _logger.LogInformation("✅ Запис видалено з БД");

        var userEmail = GetCurrentUserEmailOrEmpty();
        if (!string.IsNullOrEmpty(userEmail))
        {
            var user = await _userRepository.GetByEmailAsync(userEmail);
            if (user != null)
            {
                _logger.LogInformation("📢 Створення сповіщення FoodRemoved. UserId={UserId}", user.Id);
                await _notificationService.CreateFoodRemovedNotificationAsync(user.Id, foodName);
                _logger.LogInformation("✅ Сповіщення FoodRemoved створено");
            }
            else
            {
                _logger.LogWarning("❌ Користувач не знайдений для email={Email}", userEmail);
            }
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> QuickAddLogEntry(int logId)
    {
        _logger.LogInformation("⚡ QuickAddLogEntry (Додати знову) викликана. LogId={LogId}", logId);

        var userEmail = GetCurrentUserEmailOrEmpty();
        var success = await _foodService.QuickAddFoodLogEntryAsync(logId, userEmail);

        if (success)
        {
            _logger.LogInformation("✅ QuickAddLogEntry успішна");
            return RedirectToAction(nameof(Index), new { date = DateTime.Today });
        }

        _logger.LogError("❌ QuickAddLogEntry невдала");
        return BadRequest("Unable to quickly add log entry.");
    }

    public async Task<IActionResult> DailyDetails(DateTime? date)
    {
        var selectedDate = date ?? DateTime.Today;
        var userEmail = GetCurrentUserEmailOrEmpty();

        var meals = await _mealRepository.GetMealsWithFoodsAsync(userEmail, selectedDate);

        var viewModel = new DailyDiaryViewModel
        {
            Date = selectedDate,
            Meals = meals,
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> QuickAddMealToday(int mealId)
    {
        var userEmail = GetCurrentUserEmailOrEmpty();
        var success = await _quickAddService.QuickAddMealTodayAsync(mealId, userEmail);

        if (success)
        {
            return RedirectToAction(nameof(DailyDetails), new { date = DateTime.Today });
        }

        return BadRequest("Unable to quickly add meal. Please try again.");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateFoodLogNote(int logId, string? note)
    {
        var userEmail = GetCurrentUserEmailOrEmpty();
        var success = await _diaryNoteService.UpdateNoteAsync(logId, userEmail, note);

        if (success)
        {
            return Json(new { success = true });
        }

        return Json(new { success = false, error = "Unable to update note. Please try again." });
    }
}