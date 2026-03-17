using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StayFit.Application.Interfaces;
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

        // Services
        services.AddScoped<IRegistrationService, RegistrationService>();

        return services;
    }
}
