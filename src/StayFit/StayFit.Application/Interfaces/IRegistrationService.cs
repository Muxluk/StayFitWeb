using StayFit.Application.DTOs;

namespace StayFit.Application.Interfaces;

public interface IRegistrationService
{
    Task<RegisterUserResultDto> RegisterAsync(
        RegisterUserRequestDto request,
        CancellationToken cancellationToken = default);
}
