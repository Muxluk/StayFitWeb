using Microsoft.Extensions.Logging;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Application.Services;

public class FoodService : IFoodService
{
    private readonly IFoodRepository _foodRepository;
    private readonly ILogger<FoodService> _logger;

    public FoodService(IFoodRepository foodRepository, ILogger<FoodService> logger)
    {
        _foodRepository = foodRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Food>> GetAllFoodsAsync()
    {
        _logger.LogInformation("Отримання списку всіх продуктів.");
        return await _foodRepository.GetAllAsync();
    }

    public async Task<Food?> GetFoodByIdAsync(int id)
    {
        _logger.LogInformation("Пошук продукту з ID: {Id}", id);
        var food = await _foodRepository.GetByIdAsync(id);

        if (food == null)
        {
            _logger.LogWarning("Продукт з ID: {Id} не знайдено.", id);
        }

        return food;
    }

    public async Task AddFoodAsync(Food food)
    {
        if (food == null)
        {
            throw new ArgumentNullException(nameof(food));
        }

        _logger.LogInformation("Додавання нового продукту: {Name}", food.Name);
        await _foodRepository.AddAsync(food);
    }

    public async Task UpdateFoodAsync(Food food)
    {
        if (food == null)
        {
            throw new ArgumentNullException(nameof(food));
        }

        _logger.LogInformation("Оновлення продукту з ID: {Id}", food.Id);
        await _foodRepository.UpdateAsync(food);
    }

    public async Task DeleteFoodAsync(int id)
    {
        _logger.LogInformation("Видалення продукту з ID: {Id}", id);
        var food = await _foodRepository.GetByIdAsync(id);
        if (food != null)
        {
            await _foodRepository.DeleteAsync(id);
        }
        else
        {
            _logger.LogWarning("Спроба видалити неіснуючий продукт з ID: {Id}", id);
            throw new KeyNotFoundException($"Продукт з ID {id} не знайдено.");
        }
    }
}
