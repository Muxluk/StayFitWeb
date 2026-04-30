using StayFit.Domain.Entities;

namespace StayFit.Application.DTOs;

public class BarcodeScanResultDto
{
    public Food Food { get; set; } = null!;
    public bool ExistsInLocalDb { get; set; }
}
