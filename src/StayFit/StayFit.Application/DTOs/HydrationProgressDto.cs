namespace StayFit.Application.DTOs;

public class HydrationProgressDto
{
    public int DailyGoalMl { get; set; }
    public int ConsumedMl { get; set; }
    public int Percentage => DailyGoalMl == 0 ? 0 : (int)Math.Min(100, (double)ConsumedMl / DailyGoalMl * 100);
    public List<int> QuickAddOptions { get; set; } = new();
    public IEnumerable<WaterLogDto> TodayLogs { get; set; } = new List<WaterLogDto>();
}

public class WaterLogDto
{
    public int Id { get; set; }
    public int VolumeMl { get; set; }
    public DateTime LoggedAt { get; set; }
}