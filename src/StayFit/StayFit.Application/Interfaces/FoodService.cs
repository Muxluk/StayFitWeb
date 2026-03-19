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

    public async Task<IEnumerable<Food>> GetAllFoodsAsync(int userId)
    {
        _logger.LogInformation("Отримання списку продуктів для користувача: {UserId}", userId);
        return await _foodRepository.GetAllByOwnerAsync(userId);
    }

    public async Task<Food?> GetFoodByIdAsync(int id, int userId)
    {
        _logger.LogInformation("Пошук продукту з ID: {Id} для користувача: {UserId}", id, userId);
        var food = await _foodRepository.GetByIdAndOwnerAsync(id, userId);

        if (food == null)
        {
            _logger.LogWarning("Продукт з ID: {Id} не знайдено для користувача: {UserId}.", id, userId);
        }

        return food;
    }

    public async Task AddFoodAsync(Food food, int userId)
    {
        if (food == null)
        {
            throw new ArgumentNullException(nameof(food));
        }

        food.OwnerUserId = userId;
        _logger.LogInformation("Додавання нового продукту: {Name}", food.Name);
        await _foodRepository.AddAsync(food);
    }

    public async Task UpdateFoodAsync(Food food, int userId)
    {
        if (food == null)
        {
            throw new ArgumentNullException(nameof(food));
        }

        if (food.OwnerUserId != userId)
        {
            _logger.LogWarning("Спроба оновити чужий продукт. FoodId: {FoodId}, UserId: {UserId}", food.Id, userId);
            throw new KeyNotFoundException($"Продукт з ID {food.Id} не знайдено.");
        }

        _logger.LogInformation("Оновлення продукту з ID: {Id}", food.Id);
        await _foodRepository.UpdateAsync(food);
    }

    public async Task DeleteFoodAsync(int id, int userId)
    {
        _logger.LogInformation("Видалення продукту з ID: {Id} для користувача: {UserId}", id, userId);
        var food = await _foodRepository.GetByIdAndOwnerAsync(id, userId);
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
