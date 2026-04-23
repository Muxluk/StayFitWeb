using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StayFit.Application.Interfaces;
using StayFit.Application.Services;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Data;
using StayFit.Infrastructure.Repositories;
using StayFit.Infrastructure.Services;

namespace StayFit.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // БД
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Репозиторії
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFoodRepository, FoodRepository>();
        services.AddScoped<IFoodLogRepository, FoodLogRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IMealRepository, MealRepository>();
        services.AddScoped<IStatisticsRepository, StatisticsRepository>();
        services.AddScoped<INutritionGoalRepository, NutritionGoalRepository>();
        services.AddScoped<IAdminUserRepository, AdminUserRepository>();
        services.AddScoped<IFoodCategoryRepository, FoodCategoryRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ISecurityLogRepository, SecurityLogRepository>();

        // Services
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPasswordResetService, PasswordResetService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<INutritionGoalService, NutritionGoalService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IAdminAccountEditService, AdminAccountEditService>();        
        services.AddScoped<IDashboardService, DashboardService>();        
        services.AddScoped<IProgressService, ProgressService>();
        services.AddScoped<IProductModerationService, ProductModerationService>();
        services.AddScoped<IPasswordChangeService, PasswordChangeService>();
        services.AddScoped<IProfileSetupService, ProfileSetupService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IAccountDeletionRepository, AccountDeletionRepository>();
        services.AddScoped<IAccountDeletionService, AccountDeletionService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFoodImportRepository, FoodImportRepository>();
        services.AddScoped<IFoodImportService, FoodImportService>();
        services.AddScoped<ISecurityLogService, SecurityLogService>();
        return services;
    }
}
