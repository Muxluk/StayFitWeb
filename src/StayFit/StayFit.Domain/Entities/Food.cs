namespace StayFit.Domain.Entities;

public class Food
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public float CaloriesPer100g { get; set; }
    public float ProteinPer100g { get; set; }
    public float FatPer100g { get; set; }
    public float CarbsPer100g { get; set; }

    public ICollection<FoodLog> FoodLogs { get; set; } = new List<FoodLog>();
}
