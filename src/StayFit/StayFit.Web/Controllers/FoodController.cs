using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Identity;
using StayFit.Web.Hubs;

namespace StayFit.Web.Controllers;

[Authorize]
public class FoodController : BaseController
{
    private readonly IFoodService _foodService;
    private readonly IUserRepository _userRepository;
    private readonly IFoodRepository _foodRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<FoodController> _logger;

    public FoodController(
        IFoodService foodService,
        IUserRepository userRepository,
        IFoodRepository foodRepository,
        INotificationRepository notificationRepository,
        IHubContext<NotificationHub> hubContext,
        UserManager<ApplicationUser> userManager,
        ILogger<FoodController> logger)
    {
        _foodService = foodService;
        _userRepository = userRepository;
        _foodRepository = foodRepository;
        _notificationRepository = notificationRepository;
        _hubContext = hubContext;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: Food
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserIdOrDefault();
        var foods = await _foodService.GetAllFoodsAsync(userId);

        return View(foods);
    }

    // GET: Food/Create
    public IActionResult Create()
    {
        return View(new Food { Category = StayFit.Domain.Enums.FoodCategory.General });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Food food)
    {
        _logger.LogInformation("📝 Спроба створення продукту: {FoodName}", food?.Name);
        
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            _logger.LogWarning("❌ ModelState невалідний: {Errors}", errors);
            return View(food);
        }

        try
        {
            if (food == null) return BadRequest("Продукт не може бути порожнім");

            var userId = GetCurrentUserIdOrDefault();
            food.CreatedByEmail = User.Identity?.Name;

            await _foodService.AddFoodAsync(food, userId);
            _logger.LogInformation("✅ Продукт збережено в БД з ID: {FoodId}", food.Id);

            // ── Миттєве сповіщення адмінам через SignalR ──
            try
            {
                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                _logger.LogInformation("📢 Знайдено {Count} адміністраторів для сповіщення", adminUsers.Count);

                foreach (var adminUser in adminUsers)
                {
                    var domainAdmin = await _userRepository.GetByEmailAsync(adminUser.Email ?? string.Empty);
                    if (domainAdmin == null)
                    {
                        _logger.LogWarning("⚠️ Не знайдено доменного користувача для адміна {Email}", adminUser.Email);
                        continue;
                    }

                    var notification = new Notification
                    {
                        UserId = domainAdmin.Id,
                        Title = "Новий продукт на модерацію",
                        Message = $"Продукт «{food.Name}» очікує перевірки",
                        Type = "PendingProduct",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _notificationRepository.AddAsync(notification);

                    _logger.LogInformation("📤 Надсилання SignalR push для AdminId={AdminId} (IdentityId={IdentityId})", domainAdmin.Id, adminUser.Id);
                    await _hubContext.Clients.User(adminUser.Id.ToString())
                        .SendAsync("ReceiveNotification", new
                        {
                            title = notification.Title,
                            message = notification.Message,
                            type = notification.Type,
                            createdAt = notification.CreatedAt
                        });
                }

                // Отримуємо "свіжий" об'єкт з бази та позначаємо як сповіщений
                var savedFood = await _foodRepository.GetByIdAsync(food.Id);
                if (savedFood != null)
                {
                    savedFood.IsAdminNotified = true;
                    await _foodRepository.UpdateAsync(savedFood);
                    _logger.LogInformation("✅ Прапорець IsAdminNotified встановлено для FoodId={FoodId}", food.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Помилка при обробці сповіщень адмінам");
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Помилка при створенні продукту");
            ModelState.AddModelError("", "Сталася помилка при збереженні продукту.");
            return View(food);
        }
    }

    // GET: Food/{id}/edit
    [HttpGet("Food/{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = GetCurrentUserIdOrDefault();
        var food = await _foodService.GetFoodByIdAsync(id, userId);

        if (food == null)
        {
            return NotFound();
        }

        return View(food);
    }

    // POST: Food/{id}/edit
    [HttpPost("Food/{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Food food)
    {
        if (id != food.Id)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            var userId = GetCurrentUserIdOrDefault();

            await _foodService.UpdateFoodAsync(food, userId);
            _logger.LogInformation("Продукт з ID {FoodId} оновлено користувачем {UserId}", id, userId);

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
        var userId = GetCurrentUserIdOrDefault();

        await _foodService.DeleteFoodAsync(id, userId);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> AddToLog(int? foodId)
    {
        if (foodId.HasValue)
        {
            var userId = GetCurrentUserIdOrDefault();
            var food = await _foodService.GetFoodByIdAsync(foodId.Value, userId);
            if (food != null)
                ViewBag.SelectedFood = food;
        }
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> SearchJson(string? q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Json(Array.Empty<object>());

        var userId = GetCurrentUserIdOrDefault();
        var all = await _foodService.GetAllFoodsAsync(userId);
        var filtered = all
            .Where(f => f.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
            .Take(20)
            .Select(f => new {
                f.Id,
                f.Name,
                f.CaloriesPer100g,
                f.ProteinPer100g,
                f.FatPer100g,
                f.CarbsPer100g,
                Category = f.Category.ToString()
            });
        return Json(filtered);
    }

    [HttpPost]
    public async Task<IActionResult> AddToLog(int foodId, double quantity, string? mealType)
    {
        _logger.LogInformation("🍽️ AddToLog (FoodController) викликана. FoodId={FoodId}, Quantity={Quantity}g, MealType={MealType}", foodId, quantity, mealType);

        var userEmail = User.Identity?.Name;
        if (string.IsNullOrEmpty(userEmail))
        {
            _logger.LogWarning("❌ UserEmail порожній");
            return Challenge();
        }

        try
        {
            // Map meal type to a specific hour of today so diary grouping works
            var today = DateTime.Today;
            DateTime? loggedAt = mealType?.ToLowerInvariant() switch
            {
                "breakfast" => today.AddHours(8),
                "lunch"     => today.AddHours(13),
                "dinner"    => today.AddHours(19),
                "snack"     => today.AddHours(23),
                _           => null
            };

            await _foodService.AddFoodToLogAsync(foodId, quantity, userEmail, loggedAt);

            _logger.LogInformation("✅ AddToLog завершено FoodId={FoodId}, Email={Email}", foodId, userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Помилка при додаванні продукту {FoodId} до щоденника", foodId);
            TempData["ErrorMessage"] = "Помилка при додаванні продукту до щоденника.";
        }

        return RedirectToAction("Index", "Diary");
    }

}
