[Authorize]
public class DiaryController : Controller
{
    private readonly IFoodService _foodService;

    public DiaryController(IFoodService foodService)
    {
        _foodService = foodService;
    }

    public async Task<IActionResult> Index(DateTime? date)
    {
        var selectedDate = date ?? DateTime.Today;
        var userEmail = User.Identity.Name;
        
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
    var userEmail = User.Identity.Name;

    var meals = await _mealRepository.GetMealsWithFoodsAsync(userEmail, selectedDate);

    var viewModel = new DailyDiaryViewModel
    {
        Date = selectedDate,
        Meals = meals
    };

    return View(viewModel);
}
}