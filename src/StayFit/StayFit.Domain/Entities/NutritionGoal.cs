namespace StayFit.Domain.Entities;

public class NutritionGoal
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public float CaloriesGoal { get; set; }
    public float ProteinGoal { get; set; }
    public float FatGoal { get; set; }
    public float CarbsGoal { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}