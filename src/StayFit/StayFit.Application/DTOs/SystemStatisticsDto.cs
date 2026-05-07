namespace StayFit.Application.DTOs;

public class SystemStatisticsDto
{
    public int TotalUsers { get; set; }
    public int TotalProducts { get; set; }
    public int TotalDiaryEntries { get; set; }
    public int ActiveSessions { get; set; }
    public DateTime LastUpdated { get; set; }
}