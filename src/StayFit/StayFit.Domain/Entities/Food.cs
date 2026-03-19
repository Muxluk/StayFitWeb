namespace StayFit.Domain.Entities;

public class Food
{
    public int Id { get; set; }
    public int OwnerUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public float CaloriesPer100g { get; set; }
    public float ProteinPer100g { get; set; }
    public float FatPer100g { get; set; }
    public float CarbsPer100g { get; set; }
    public string? CreatedByEmail { get; set; }

    // Backward-compatible aliases used by existing web views.
    public float Calories => CaloriesPer100g;
    public float Proteins => ProteinPer100g;
    public float Fats => FatPer100g;
    public float Carbohydrates => CarbsPer100g;

    public ICollection<FoodLog> FoodLogs { get; set; } = new List<FoodLog>();
}
