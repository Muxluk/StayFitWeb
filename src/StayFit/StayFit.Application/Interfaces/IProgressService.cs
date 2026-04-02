using System;
using System.Threading.Tasks;
using StayFit.Application.DTOs;
using StayFit.Domain.Results;

namespace StayFit.Application.Interfaces;

public interface IProgressService
{
    Task<Result<ProgressAnalysisDto>> GetProgressAnalysisAsync(int userId, string userEmail, DateTime startDate, DateTime endDate);
}
