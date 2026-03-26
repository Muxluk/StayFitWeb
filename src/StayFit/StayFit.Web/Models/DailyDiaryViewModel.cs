using StayFit.Domain.Entities;

namespace StayFit.Web.Models;

public class DailyDiaryViewModel
{
    public DateTime Date { get; set; }
    public IEnumerable<MealEntry> Meals { get; set; } = new List<MealEntry>();

    public double TotalCalories => Meals.SelectMany(m => m.FoodLogs)
        .Sum(fl => (fl.Food?.CaloriesPer100g ?? 0) * fl.Quantity / 100);

    public double TotalProteins => Meals.SelectMany(m => m.FoodLogs)
        .Sum(fl => (fl.Food?.ProteinPer100g ?? 0) * fl.Quantity / 100);

    public double TotalFats => Meals.SelectMany(m => m.FoodLogs)
        .Sum(fl => (fl.Food?.FatPer100g ?? 0) * fl.Quantity / 100);

    public double TotalCarbs => Meals.SelectMany(m => m.FoodLogs)
        .Sum(fl => (fl.Food?.CarbsPer100g ?? 0) * fl.Quantity / 100);
}