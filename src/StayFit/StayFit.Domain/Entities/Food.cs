using StayFit.Domain.Enums;

namespace StayFit.Domain.Entities;

public class Food
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public float CaloriesPer100g { get; set; }
    public float ProteinPer100g { get; set; }
    public float FatPer100g { get; set; }
    public float CarbsPer100g { get; set; }

    public string? CreatedByEmail { get; set; }
    public int OwnerUserId { get; set; }

    public string? Brand { get; set; }
    public string? Barcode { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsVerified { get; set; }

    public FoodCategory Category { get; set; } = FoodCategory.General;

    public ICollection<FoodLog> FoodLogs { get; set; } = new List<FoodLog>();
}
