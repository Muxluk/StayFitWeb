using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using StayFit.Application.Interfaces;
using StayFit.Application.Options;
using StayFit.Application.Services;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure;
using StayFit.Application.Configuration;
using StayFit.Web.Filters;
using StayFit.Infrastructure.Data;
using StayFit.Infrastructure.Identity;
using StayFit.Infrastructure.Repositories;
using StayFit.Web.Services;
using StayFit.Web.Middleware;
using System.Text;
using System.Threading.RateLimiting;


var builder = WebApplication.CreateBuilder(args);
if (!builder.Environment.IsDevelopment())
{
    try
    {
        var client = new AmazonSecretsManagerClient(RegionEndpoint.EUNorth1);
        var request = new GetSecretValueRequest
        {
            SecretId = "StayFit/Prod/Secrets"
        };

        var response = client.GetSecretValueAsync(request).GetAwaiter().GetResult();

        if (!string.IsNullOrEmpty(response.SecretString))
        {
            var secretBytes = Encoding.UTF8.GetBytes(response.SecretString);
            builder.Configuration.AddJsonStream(new MemoryStream(secretBytes));
            Console.WriteLine("Секрети з AWS завантажено!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Помилка AWS Secrets Manager: {ex.Message}");
    }
}
builder.Configuration.AddUserSecrets<Program>(optional: true, reloadOnChange: true);

// ─── Serilog ────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .MinimumLevel.Information()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/stayfit-.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}",
            restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
        .WriteTo.Seq(
            serverUrl: "http://localhost:5341",
            apiKey: null,
            restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "StayFit"));

// ─── MVC ────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<SessionValidationFilter>();
});

// ─── Global Exception Handler ───────────────────────────────────────────────
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddMemoryCache();

// ─── Infrastructure (БД + репозиторії) ─────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ─── Configuration Sections ─────────────────────────────────────────────────
var passwordSettingsSection = builder.Configuration.GetSection("PasswordSettings");
builder.Services.Configure<PasswordSettings>(passwordSettingsSection);
var passwordSettings = passwordSettingsSection.Get<PasswordSettings>() ?? new PasswordSettings();
builder.Services.Configure<ProfileSetupOptions>(builder.Configuration.GetSection("ProfileSetup"));
builder.Services.Configure<DashboardSettings>(builder.Configuration.GetSection("Dashboard"));
builder.Services.Configure<ProfilePhotoOptions>(builder.Configuration.GetSection(ProfilePhotoOptions.SectionName));

builder.Services.Configure<SessionSettings>(builder.Configuration.GetSection(SessionSettings.SectionName));
builder.Services.Configure<DiaryNoteSettings>(builder.Configuration.GetSection("DiaryNotes"));
builder.Services.Configure<NotificationSettings>(builder.Configuration.GetSection(NotificationSettings.SectionName));
builder.Services.Configure<UsdaFoodDataOptions>(builder.Configuration.GetSection("UsdaFoodData"));
builder.Services.Configure<SecurityLogSettings>(builder.Configuration.GetSection(SecurityLogSettings.SectionName));
builder.Services.Configure<StayFit.Application.Configuration.SystemStatisticsSettings>(builder.Configuration.GetSection("SystemStatisticsCache"));
builder.Services.Configure<HydrationSettings>(builder.Configuration.GetSection("HydrationSettings"));
builder.Services.AddHttpClient();

// ─── Identity ───────────────────────────────────────────────────────────────
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = passwordSettings.MinLength;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireDigit = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Error/403";
});
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("WaterLogPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 40;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});
// ─── Application Services ───────────────────────────────────────────────────
builder.Services.AddScoped<LoggingService>();
builder.Services.AddScoped<IFoodService>(provider =>
    new FoodService(
        provider.GetRequiredService<IFoodRepository>(),
        provider.GetRequiredService<ILogger<FoodService>>(),
        provider.GetRequiredService<IFoodLogRepository>(),
        provider.GetRequiredService<IUserRepository>(),
        provider.GetRequiredService<INotificationService>(),
        provider.GetRequiredService<INutritionGoalRepository>(),
        provider.GetRequiredService<IOptions<NotificationSettings>>()
    ));
builder.Services.AddScoped<IProductSearchService, ProductSearchService>();
builder.Services.AddScoped<MealService>();
builder.Services.AddScoped<IFoodCategoryService, FoodCategoryService>();
builder.Services.AddScoped<QuickAddService>(provider =>
    new QuickAddService(
        provider.GetRequiredService<IMealRepository>(),
        provider.GetRequiredService<IFoodLogRepository>(),
        provider.GetRequiredService<IUserRepository>(),
        provider.GetRequiredService<INutritionGoalRepository>(),
        provider.GetRequiredService<INotificationService>(),
        provider.GetRequiredService<IOptions<NotificationSettings>>(),
        provider.GetRequiredService<ILogger<QuickAddService>>()
    ));
builder.Services.AddScoped<DiaryNoteService>();
builder.Services.AddScoped<IProfilePhotoService, ProfilePhotoService>();
builder.Services.AddScoped<IHydrationRepository, HydrationRepository>();
builder.Services.AddScoped<IHydrationService, HydrationService>();
// ─── Build ──────────────────────────────────────────────────────────────────
var app = builder.Build();

// ─── Auto migrations ─────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

await IdentityRoleSeeder.SeedRolesAsync(app.Services);
await IdentityRoleSeeder.SeedAdminUserAsync(app.Services, app.Configuration);

// ─── Exception Handler ──────────────────────────────────────────────────────
app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<RequestExecutionTimeLoggingMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
