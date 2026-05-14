using StayFit.Domain.Entities;
using StayFit.Domain.Results;

namespace StayFit.Application.Interfaces;

/// <summary>
/// Сервіс для управління категоріями продуктів
/// </summary>
public interface IFoodCategoryService
{
    /// <summary>
    /// Отримати всі активні категорії
    /// </summary>
    Task<Result<IEnumerable<FoodCategoryEntity>>> GetAllAsync();

    /// <summary>
    /// Отримати категорію за ID
    /// </summary>
    Task<Result<FoodCategoryEntity>> GetByIdAsync(int id);

    /// <summary>
    /// Отримати активні категорії
    /// </summary>
    Task<Result<IEnumerable<FoodCategoryEntity>>> GetActiveAsync();

    /// <summary>
    /// Створити нову категорію
    /// </summary>
    Task<Result<FoodCategoryEntity>> CreateAsync(string name, string? description, string? icon, string? colorClass);

    /// <summary>
    /// Оновити категорію
    /// </summary>
    Task<Result<FoodCategoryEntity>> UpdateAsync(int id, string name, string? description, bool isActive, string? icon, string? colorClass);

    /// <summary>
    /// Видалити категорію
    /// </summary>
    Task<Result> DeleteAsync(int id);
}
