using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using StayFit.Application.Interfaces;
using StayFit.Application.Options;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Identity;
using StayFit.Web.Hubs;

namespace StayFit.Web.Services;

public class NutritionBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IOptionsMonitor<NotificationSettings> _settings;
    private readonly ILogger<NutritionBackgroundService> _logger;

    // Відстежуємо ID продуктів, які вже були повідомлені адміну в цьому сеансі
    private readonly HashSet<int> _notifiedPendingProductIds = new();

    public NutritionBackgroundService(
        IServiceScopeFactory scopeFactory,
        IHubContext<NotificationHub> hubContext,
        IOptionsMonitor<NotificationSettings> settings,
        ILogger<NutritionBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _settings = settings;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NutritionBackgroundService запущено");

        while (!stoppingToken.IsCancellationRequested)
        {
            var intervalSeconds = _settings.CurrentValue.BackgroundServiceIntervalSeconds;

            try
            {
                await RunChecksAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка під час виконання фонових перевірок сповіщень");
            }

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }

        _logger.LogInformation("NutritionBackgroundService зупинено");
    }

    private async Task RunChecksAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var nutritionGoalRepo = scope.ServiceProvider.GetRequiredService<INutritionGoalRepository>();
        var foodLogRepo = scope.ServiceProvider.GetRequiredService<IFoodLogRepository>();
        var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var foodRepo = scope.ServiceProvider.GetRequiredService<IFoodRepository>();
        var supportRepo = scope.ServiceProvider.GetRequiredService<ISupportRepository>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await CheckCalorieThresholdAsync(nutritionGoalRepo, foodLogRepo, notificationRepo, notificationService, cancellationToken);
        await CheckNewSupportTicketsAsync(supportRepo, notificationRepo, userManager, cancellationToken);
        await CheckPendingProductsAsync(foodRepo, notificationRepo, userManager, cancellationToken);
    }

    // ── Подія 1: Перевищення денної норми калорій ────────────────────────────
    private async Task CheckCalorieThresholdAsync(
        INutritionGoalRepository nutritionGoalRepo,
        IFoodLogRepository foodLogRepo,
        INotificationRepository notificationRepo,
        INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var threshold = _settings.CurrentValue.CalorieThresholdPercent;
        var allGoals = await nutritionGoalRepo.GetAllAsync();

        foreach (var goal in allGoals)
        {
            // NutritionGoal.UserId stores the domain integer ID as a string (e.g. "23")
            if (!int.TryParse(goal.UserId, out var userId))
                continue;

            if (await notificationRepo.HasCalorieThresholdNotificationTodayAsync(userId))
                continue;

            var todayLogs = await foodLogRepo.GetByUserIdAndDateAsync(userId, DateTime.UtcNow);
            var totalCalories = todayLogs.Sum(fl => fl.Food.CaloriesPer100g * fl.AmountGrams / 100f);

            var limitCalories = goal.CaloriesGoal * (float)(threshold / 100m);
            if (totalCalories < limitCalories)
                continue;

            var overage = (decimal)(totalCalories - goal.CaloriesGoal);
            await notificationService.CreateCalorieThresholdNotificationAsync(userId, overage);

            var notification = new
            {
                title = "⚠️ Перевищення денної норми калорій",
                message = $"Ви перевищили денну норму на {overage:F0} ккал",
                type = "CalorieThreshold",
                createdAt = DateTime.UtcNow
            };

            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("ReceiveNotification", notification);

            _logger.LogInformation(
                "Сповіщення про перевищення калорій надіслано користувачу {UserId}", userId);
        }
    }

    // ── Подія 2: Нове звернення техпідтримки → адміну ───────────────────────
    private async Task CheckNewSupportTicketsAsync(
        ISupportRepository supportRepo,
        INotificationRepository notificationRepo,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        // Отримуємо нові тікети, про які ще не сповіщено адміна
        var allNewTickets = await supportRepo.GetAllTicketsAsync(StayFit.Domain.Enums.SupportStatus.New, 0, 100);
        var unnotified = allNewTickets.Where(t => !t.IsAdminNotified).ToList();
        
        if (unnotified.Count == 0)
            return;

        var adminUsers = await userManager.GetUsersInRoleAsync("Admin");

        foreach (var adminUser in adminUsers)
        {
            foreach (var ticket in unnotified)
            {
                var adminNotification = new Notification
                {
                    UserId = adminUser.Id,
                    Title = "🆘 Нове звернення техпідтримки",
                    Message = $"Тема: {ticket.Subject}",
                    Type = "SupportRequest",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await notificationRepo.AddAsync(adminNotification);

                var payload = new
                {
                    title = adminNotification.Title,
                    message = adminNotification.Message,
                    type = "SupportRequest",
                    createdAt = adminNotification.CreatedAt
                };

                await _hubContext.Clients.User(adminUser.Id.ToString())
                    .SendAsync("ReceiveNotification", payload);
            }
        }

        foreach (var ticket in unnotified)
        {
            ticket.IsAdminNotified = true;
            await supportRepo.UpdateTicketAsync(ticket);
        }

        _logger.LogInformation(
            "Надіслано {Count} сповіщень про нові звернення техпідтримки адмінам",
            unnotified.Count);
    }

    // ── Подія 3: Новий продукт чекає модерації → адміну ─────────────────────
    private async Task CheckPendingProductsAsync(
        IFoodRepository foodRepo,
        INotificationRepository notificationRepo,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var pendingProducts = (await foodRepo.GetPendingProductsAsync()).ToList();

        var newProducts = pendingProducts
            .Where(p => !p.IsAdminNotified)
            .ToList();

        if (newProducts.Count == 0)
            return;

        var adminUsers = await userManager.GetUsersInRoleAsync("Admin");

        foreach (var adminUser in adminUsers)
        {
            foreach (var product in newProducts)
            {
                var adminNotification = new Notification
                {
                    UserId = adminUser.Id,
                    Title = "🛒 Новий продукт на модерацію",
                    Message = $"Продукт «{product.Name}» очікує перевірки",
                    Type = "PendingProduct",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await notificationRepo.AddAsync(adminNotification);

                var payload = new
                {
                    title = adminNotification.Title,
                    message = adminNotification.Message,
                    type = "PendingProduct",
                    createdAt = adminNotification.CreatedAt
                };

                await _hubContext.Clients.User(adminUser.Id.ToString())
                    .SendAsync("ReceiveNotification", payload);
            }
        }

        foreach (var product in newProducts)
        {
            product.IsAdminNotified = true;
            await foodRepo.UpdateAsync(product);
        }

        _logger.LogInformation(
            "Надіслано сповіщення про {Count} новий(х) продукт(ів) на модерацію",
            newProducts.Count);
    }
}
