using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFit.Application.Interfaces;
using StayFit.Domain.Interfaces;
using StayFit.Web.Models;

namespace StayFit.Web.Controllers;

[Authorize]
public class DiaryController : Controller
{
    private readonly IFoodService _foodService;
    private readonly IMealRepository _mealRepository;

    public DiaryController(IFoodService foodService, IMealRepository mealRepository)
    {
        _foodService = foodService;
        _mealRepository = mealRepository;
    }

    public async Task<IActionResult> Index(DateTime? date)
    {
        var selectedDate = date ?? DateTime.Today;
        var userEmail = User.Identity?.Name ?? string.Empty;

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
        var userEmail = User.Identity?.Name ?? string.Empty;

        var meals = await _mealRepository.GetMealsWithFoodsAsync(userEmail, selectedDate);

        var viewModel = new DailyDiaryViewModel
        {
            Date = selectedDate,
            Meals = meals,
        };

        return View(viewModel);
    }
}