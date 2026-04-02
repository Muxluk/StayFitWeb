using StayFit.Application.Common;
using StayFit.Application.DTOs;

namespace StayFit.Application.Interfaces;

public interface IDashboardService
{
    Task<Result<DashboardDto>> GetTodayDashboardAsync(int userId);
}
