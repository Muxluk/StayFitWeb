using Microsoft.Extensions.Logging;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Application.Services;

public class MealService
{
    private readonly IRepository<MealEntry> _mealRepository;
    private readonly ILogger<MealService> _logger;

    public MealService(IRepository<MealEntry> mealRepository, ILogger<MealService> logger)
    {
        _mealRepository = mealRepository;
        _logger = logger;
    }

    public async Task CreateMealAsync(MealEntry meal)
    {
        _logger.LogInformation("Attempting to create a meal: {MealName} for user {User}", meal.Name, meal.UserEmail);

        await _mealRepository.AddAsync(meal);

        _logger.LogInformation("Meal {MealId} successfully created.", meal.Id);
    }

    public async Task<IEnumerable<MealEntry>> GetUserMealsAsync(string email)
    {
        _logger.LogDebug("Fetching meals for user: {Email}", email);
        return await _mealRepository.GetAllAsync();
    }
}