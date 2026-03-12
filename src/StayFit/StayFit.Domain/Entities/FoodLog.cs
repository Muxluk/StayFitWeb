namespace StayFit.Domain.Entities;

public class FoodLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int FoodId { get; set; }
    public float AmountGrams { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Food Food { get; set; } = null!;
}
