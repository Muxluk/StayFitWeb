namespace StayFit.Domain.Entities;

public class NutritionSummary
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public decimal TotalCalories { get; set; }
    public decimal TotalProtein { get; set; }
    public decimal TotalFat { get; set; }
    public decimal TotalCarbs { get; set; }

    public int DaysWithLogs { get; set; }

    public decimal AverageDailyCalories => DaysWithLogs > 0 ? TotalCalories / DaysWithLogs : 0m;
    public decimal AverageDailyProtein => DaysWithLogs > 0 ? TotalProtein / DaysWithLogs : 0m;
    public decimal AverageDailyFat => DaysWithLogs > 0 ? TotalFat / DaysWithLogs : 0m;
    public decimal AverageDailyCarbs => DaysWithLogs > 0 ? TotalCarbs / DaysWithLogs : 0m;
}
