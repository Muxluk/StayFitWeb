namespace StayFit.Domain.Entities;

public class WaterLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int VolumeMl { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}