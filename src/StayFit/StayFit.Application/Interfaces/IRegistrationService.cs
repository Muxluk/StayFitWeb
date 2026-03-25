using StayFit.Application.Common;
using StayFit.Application.DTOs;

namespace StayFit.Application.Interfaces;

public interface IRegistrationService
{
    Task<Result<int>> RegisterAsync(
        RegisterUserRequestDto request,
        CancellationToken cancellationToken = default);
}
