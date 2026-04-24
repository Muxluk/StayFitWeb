using StayFit.Application.Common;
using StayFit.Application.DTOs;

namespace StayFit.Application.Interfaces;

public interface ISystemStatisticsService
{
    Task<Result<SystemStatisticsDto>> GetSystemStatisticsAsync(CancellationToken cancellationToken = default);
}