using Microsoft.Extensions.Logging;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Domain.Results;

namespace StayFit.Application.Services;

/// <summary>
/// Сервіс для управління категоріями продуктів
/// </summary>
public class FoodCategoryService : IFoodCategoryService
{
    private readonly IFoodCategoryRepository _repository;
    private readonly ILogger<FoodCategoryService> _logger;

    public FoodCategoryService(
        IFoodCategoryRepository repository,
        ILogger<FoodCategoryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Отримати всі категорії
    /// </summary>
    public async Task<Result<IEnumerable<FoodCategoryEntity>>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Завантаження всіх категорій продуктів");
            
            var categories = await _repository.GetAllAsync();
            
            _logger.LogInformation("Успішно завантажено {Count} категорій", categories.Count());

            return new Result<IEnumerable<FoodCategoryEntity>>.Success(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при завантаженні всіх категорій");
            return new Result<IEnumerable<FoodCategoryEntity>>.Failure(
                "Помилка при завантаженні категорій",
                "GET_ALL_CATEGORIES_ERROR"
            );
        }
    }

    /// <summary>
    /// Отримати категорію за ID
    /// </summary>
    public async Task<Result<FoodCategoryEntity>> GetByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Завантаження категорії #{CategoryId}", id);
            
            var category = await _repository.GetByIdAsync(id);
            
            if (category is null)
            {
                _logger.LogWarning("Категорія #{CategoryId} не знайдена", id);
                return new Result<FoodCategoryEntity>.Failure(
                    $"Категорія з ID {id} не знайдена",
                    "CATEGORY_NOT_FOUND"
                );
            }
            
            _logger.LogInformation("Успішно завантажена категорія #{CategoryId}: {CategoryName}", id, category.Name);
            
            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при завантаженні категорії #{CategoryId}", id);
            return new Result<FoodCategoryEntity>.Failure(
                "Помилка при завантаженні категорії",
                "GET_CATEGORY_ERROR"
            );
        }
    }

    /// <summary>
    /// Отримати активні категорії
    /// </summary>
    public async Task<Result<IEnumerable<FoodCategoryEntity>>> GetActiveAsync()
    {
        try
        {
            _logger.LogInformation("Завантаження активних категорій продуктів");
            
            var categories = await _repository.GetActiveAsync();
            
            _logger.LogInformation("Успішно завантажено {Count} активних категорій", categories.Count());

            return new Result<IEnumerable<FoodCategoryEntity>>.Success(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при завантаженні активних категорій");
            return new Result<IEnumerable<FoodCategoryEntity>>.Failure(
                "Помилка при завантаженні активних категорій",
                "GET_ACTIVE_CATEGORIES_ERROR"
            );
        }
    }

    /// <summary>
    /// Створити нову категорію
    /// </summary>
    public async Task<Result<FoodCategoryEntity>> CreateAsync(string name, string? description, string? icon, string? colorClass)
    {
        try
        {
            // Валідація
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("Спроба створити категорію з пустою назвою");
                return new Result<FoodCategoryEntity>.Failure(
                    "Назва категорії не може бути пустою",
                    "CATEGORY_NAME_REQUIRED"
                );
            }

            if (name.Length > 100)
            {
                _logger.LogWarning("Назва категорії занадто довга: {NameLength}", name.Length);
                return new Result<FoodCategoryEntity>.Failure(
                    "Назва категорії не може перевищувати 100 символів",
                    "CATEGORY_NAME_TOO_LONG"
                );
            }

            // Перевірка дублікату
            var exists = await _repository.ExistsByNameAsync(name);
            if (exists)
            {
                _logger.LogWarning("Категорія з назвою '{CategoryName}' вже існує", name);
                return new Result<FoodCategoryEntity>.Failure(
                    $"Категорія з назвою '{name}' вже існує",
                    "CATEGORY_NAME_DUPLICATE"
                );
            }

            var category = new FoodCategoryEntity
            {
                Name = name,
                Description = description,
                Icon = icon,
                ColorClass = colorClass,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Створення нової категорії: {CategoryName}", name);
            
            var createdCategory = await _repository.AddAsync(category);
            
            _logger.LogInformation("Категорія успішно створена з ID: {CategoryId}, Name: {CategoryName}", 
                createdCategory.Id, createdCategory.Name);
            
            return createdCategory;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при створенні категорії '{CategoryName}'", name);
            return new Result<FoodCategoryEntity>.Failure(
                "Помилка при створенні категорії",
                "CREATE_CATEGORY_ERROR"
            );
        }
    }

    /// <summary>
    /// Оновити категорію
    /// </summary>
    public async Task<Result<FoodCategoryEntity>> UpdateAsync(int id, string name, string? description, bool isActive, string? icon, string? colorClass)
    {
        try
        {
            // Валідація
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("Спроба оновити категорію #{CategoryId} з пустою назвою", id);
                return new Result<FoodCategoryEntity>.Failure(
                    "Назва категорії не може бути пустою",
                    "CATEGORY_NAME_REQUIRED"
                );
            }

            if (name.Length > 100)
            {
                _logger.LogWarning("Назва категорії занадто довга при оновленні: {NameLength}", name.Length);
                return new Result<FoodCategoryEntity>.Failure(
                    "Назва категорії не може перевищувати 100 символів",
                    "CATEGORY_NAME_TOO_LONG"
                );
            }

            // Отримати існуючу категорію
            var category = await _repository.GetByIdAsync(id);
            if (category is null)
            {
                _logger.LogWarning("Категорія #{CategoryId} не знайдена при оновленні", id);
                return new Result<FoodCategoryEntity>.Failure(
                    $"Категорія з ID {id} не знайдена",
                    "CATEGORY_NOT_FOUND"
                );
            }

            // Перевірка дублікату (якщо назва змінилася)
            if (category.Name != name)
            {
                var exists = await _repository.ExistsByNameAsync(name);
                if (exists)
                {
                    _logger.LogWarning("Категорія з назвою '{NewName}' вже існує", name);
                    return new Result<FoodCategoryEntity>.Failure(
                        $"Категорія з назвою '{name}' вже існує",
                        "CATEGORY_NAME_DUPLICATE"
                    );
                }
            }

            _logger.LogInformation("Оновлення категорії #{CategoryId}: старе ім'я '{OldName}' → нове ім'я '{NewName}'", 
                id, category.Name, name);

            // Оновлення
            category.Name = name;
            category.Description = description;
            category.Icon = icon;
            category.ColorClass = colorClass;
            category.IsActive = isActive;
            category.UpdatedAt = DateTime.UtcNow;

            var updatedCategory = await _repository.UpdateAsync(category);
            
            _logger.LogInformation("Категорія #{CategoryId} успішно оновлена", id);
            
            return updatedCategory;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при оновленні категорії #{CategoryId}", id);
            return new Result<FoodCategoryEntity>.Failure(
                "Помилка при оновленні категорії",
                "UPDATE_CATEGORY_ERROR"
            );
        }
    }

    /// <summary>
    /// Видалити категорію
    /// </summary>
    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            _logger.LogInformation("Видалення категорії #{CategoryId}", id);
            
            var exists = await _repository.ExistsAsync(id);
            if (!exists)
            {
                _logger.LogWarning("Категорія #{CategoryId} не знайдена при видаленні", id);
                return new Result.Failure(
                    $"Категорія з ID {id} не знайдена",
                    "CATEGORY_NOT_FOUND"
                );
            }

            var deleted = await _repository.DeleteAsync(id);
            
            if (!deleted)
            {
                _logger.LogWarning("Не вдалось видалити категорію #{CategoryId}", id);
                return new Result.Failure(
                    "Не вдалось видалити категорію",
                    "DELETE_CATEGORY_FAILED"
                );
            }
            
            _logger.LogInformation("Категорія #{CategoryId} успішно видалена", id);
            
            return new Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при видаленні категорії #{CategoryId}", id);
            return new Result.Failure(
                "Помилка при видаленні категорії",
                "DELETE_CATEGORY_ERROR"
            );
        }
    }
}
