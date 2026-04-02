namespace StayFit.Domain.Entities;

/// <summary>
/// Категорія продуктів харчування (управління адміністратором)
/// </summary>
public class FoodCategoryEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Відношення
    public ICollection<Food> Foods { get; set; } = new List<Food>();
}
