using System;
using System.Collections.Generic;

namespace StayFit.Application.DTOs;

public class ProgressAnalysisDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double CaloriesGoal { get; set; }
    public double ProteinGoal { get; set; }
    public double FatGoal { get; set; }
    public double CarbsGoal { get; set; }
    
    public List<DailyProgressDto> DailyProgress { get; set; } = new();
    
    public int DaysCaloriesMet { get; set; }
    public int TotalDays { get; set; }
}

public class DailyProgressDto
{
    public DateTime Date { get; set; }
    public double TotalCalories { get; set; }
    public double TotalProtein { get; set; }
    public double TotalFat { get; set; }
    public double TotalCarbs { get; set; }
    public bool CaloriesGoalMet { get; set; }
}
