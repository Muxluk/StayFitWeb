namespace StayFit.Domain.Entities;

public class MealEntry
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public virtual ICollection<FoodLog> FoodLogs { get; set; } = new List<FoodLog>();
}