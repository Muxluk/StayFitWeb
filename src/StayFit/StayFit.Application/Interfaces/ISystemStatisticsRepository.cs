using StayFit.Application.DTOs;

namespace StayFit.Application.Interfaces;

public interface ISystemStatisticsRepository
{
    Task<SystemStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default);
}