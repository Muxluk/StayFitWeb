namespace StayFit.Application.DTOs;

/// <summary>
/// DTO для відображення дашборду - порівняння фактичних та цільових значень
/// </summary>
public class DashboardDto
{
    public float ActualCalories { get; set; }
    public float TargetCalories { get; set; }
    
    public float ActualProtein { get; set; }
    public float TargetProtein { get; set; }
    
    public float ActualFat { get; set; }
    public float TargetFat { get; set; }
    
    public float ActualCarbs { get; set; }
    public float TargetCarbs { get; set; }
}
