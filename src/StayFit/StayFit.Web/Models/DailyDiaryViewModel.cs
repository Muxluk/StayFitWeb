using StayFit.Domain.Entities;

namespace StayFit.Web.Models;

public class DailyDiaryViewModel
{
    public DateTime Date { get; set; }
    public IEnumerable<MealEntry> Meals { get; set; } = new List<MealEntry>();

    public double TotalCalories => Meals.SelectMany(m => m.FoodLogs)
        .Sum(fl => (fl.Food?.Calories ?? 0) * fl.Quantity / 100);

    public double TotalProteins => Meals.SelectMany(m => m.FoodLogs)
        .Sum(fl => (fl.Food?.Proteins ?? 0) * fl.Quantity / 100);

    public double TotalFats => Meals.SelectMany(m => m.FoodLogs)
        .Sum(fl => (fl.Food?.Fats ?? 0) * fl.Quantity / 100);

    public double TotalCarbs => Meals.SelectMany(m => m.FoodLogs)
        .Sum(fl => (fl.Food?.Carbohydrates ?? 0) * fl.Quantity / 100);
}