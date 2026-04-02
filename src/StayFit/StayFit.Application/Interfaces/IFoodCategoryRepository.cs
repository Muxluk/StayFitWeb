using StayFit.Domain.Entities;

namespace StayFit.Application.Interfaces;

/// <summary>
/// Репозиторій для роботи з категоріями продуктів
/// </summary>
public interface IFoodCategoryRepository
{
    /// <summary>
    /// Отримати всі категорії
    /// </summary>
    Task<IEnumerable<FoodCategoryEntity>> GetAllAsync();

    /// <summary>
    /// Отримати категорію за ID
    /// </summary>
    Task<FoodCategoryEntity?> GetByIdAsync(int id);

    /// <summary>
    /// Отримати тільки активні категорії
    /// </summary>
    Task<IEnumerable<FoodCategoryEntity>> GetActiveAsync();

    /// <summary>
    /// Додати нову категорію
    /// </summary>
    Task<FoodCategoryEntity> AddAsync(FoodCategoryEntity category);

    /// <summary>
    /// Оновити категорію
    /// </summary>
    Task<FoodCategoryEntity> UpdateAsync(FoodCategoryEntity category);

    /// <summary>
    /// Видалити категорію за ID
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Перевірити, чи існує категорія з таким ID
    /// </summary>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Перевірити, чи існує категорія з таким назвою
    /// </summary>
    Task<bool> ExistsByNameAsync(string name);

    /// <summary>
    /// Отримати категорію за назвою
    /// </summary>
    Task<FoodCategoryEntity?> GetByNameAsync(string name);
}
