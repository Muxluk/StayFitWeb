namespace StayFit.Application.DTOs;

public class RecentDiaryEntryDto
{
    public DateTime LoggedAt { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public float AmountGrams { get; set; }
    public float Calories { get; set; }
    public float Protein { get; set; }
    public float Fat { get; set; }
    public float Carbs { get; set; }
}