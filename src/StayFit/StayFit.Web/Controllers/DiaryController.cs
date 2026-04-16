using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    private readonly QuickAddService _quickAddService;
    private readonly DiaryNoteService _diaryNoteService;

    public DiaryController(
        IFoodService foodService,
        IMealRepository mealRepository,
        QuickAddService quickAddService,
        DiaryNoteService diaryNoteService)
    {
        _foodService = foodService;
        _mealRepository = mealRepository;
        _quickAddService = quickAddService;
        _diaryNoteService = diaryNoteService;
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
        await _foodService.RemoveFromLogAsync(id);
        return RedirectToAction(nameof(Index));
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
    public async Task<IActionResult> UpdateMealNote(int mealId, string? note)
    {
        var userEmail = GetCurrentUserEmailOrEmpty();
        var success = await _diaryNoteService.UpdateNoteAsync(mealId, userEmail, note);

        if (success)
        {
            return Json(new { success = true });
        }

        return Json(new { success = false, error = "Unable to update note. Please try again." });
    }
}