using Microsoft.AspNetCore.Identity;
using Serilog;
using StayFit.Application.Interfaces;
using StayFit.Application.Services;
using StayFit.Infrastructure;
using StayFit.Infrastructure.Data;
using StayFit.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddControllersWithViews();

// ─── Infrastructure (БД + репозиторії) ─────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ─── Identity ───────────────────────────────────────────────────────────────
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireDigit = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ─── Application Services ───────────────────────────────────────────────────
builder.Services.AddScoped<LoggingService>();
builder.Services.AddScoped<IFoodService, FoodService>();
builder.Services.AddScoped<MealService>();

// ─── Build ──────────────────────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
