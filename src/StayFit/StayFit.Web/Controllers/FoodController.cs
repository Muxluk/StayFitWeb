using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Web.Controllers;

[Authorize]
public class FoodController : Controller
{
    private readonly IFoodService _foodService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<FoodController> _logger;

    public FoodController(IFoodService foodService, IUserRepository userRepository, ILogger<FoodController> logger)
    {
        _foodService = foodService;
        _userRepository = userRepository;
        _logger = logger;
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
            try
            {
                var userId = GetCurrentUserId();

                await _foodService.UpdateFoodAsync(food, userId);
                _logger.LogInformation("Продукт з ID {FoodId} оновлено користувачем {UserId}", id, userId);

                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Спроба редагувати неіснуючий продукт з ID {FoodId}", id);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при редагуванні продукту з ID {FoodId}", id);
                ModelState.AddModelError("", "Помилка при збереженні змін. Спробуйте ще раз.");
            }
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

        try
        {
            var user = await _userRepository.GetByEmailAsync(userEmail);

            if (user == null)
            {
                user = new User
                {
                    Email = userEmail,
                    Name = userEmail,
                    CreatedAt = DateTime.UtcNow,
                };

                await _userRepository.AddAsync(user);
                _logger.LogInformation("Новий користувач створено: {Email}, ID: {UserId}", userEmail, user.Id);
            }

            var log = new FoodLog
            {
                UserId = user.Id,
                FoodId = foodId,
                AmountGrams = (float)quantity,
                LoggedAt = DateTime.Now.ToUniversalTime(),
                UserEmail = userEmail,
            };

            await _foodService.AddFoodToLogAsync(log);
            _logger.LogInformation("Запис в лог їжі створено: UserId={UserId}, FoodId={FoodId}, Quantity={Quantity}", user.Id, foodId, quantity);

            return RedirectToAction("Index", "Diary");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при додаванні запису в лог їжі для користувача {Email}", userEmail);
            return BadRequest("Помилка при додаванні в раціон");
        }
    }
    private int GetCurrentUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userIdStr, out var userId) ? userId : 0;
    }
}
