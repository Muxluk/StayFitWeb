namespace StayFit.Domain.Entities;

public class HydrationGoal
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int DailyGoalMl { get; set; }
    
    public User User { get; set; } = null!;
}