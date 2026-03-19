namespace StayFit.Domain.Entities;

public class MealEntry
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Time { get; set; }
    public string UserEmail { get; set; }

    public virtual ICollection<FoodLog> FoodLogs { get; set; } = new List<FoodLog>();
}