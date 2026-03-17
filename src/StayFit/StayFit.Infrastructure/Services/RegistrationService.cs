using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Infrastructure.Identity;

namespace StayFit.Infrastructure.Services;

public sealed class RegistrationService(
    UserManager<ApplicationUser> userManager,
    ILogger<RegistrationService> logger)
    : IRegistrationService
{
    public async Task<RegisterUserResultDto> RegisterAsync(
        RegisterUserRequestDto request,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Registration attempt for email {Email}", request.Email);

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            logger.LogWarning(
                "Registration failed for email {Email}. Errors: {Errors}",
                request.Email,
                string.Join("; ", errors));

            return new RegisterUserResultDto
            {
                Succeeded = false,
                Errors = errors,
            };
        }

        logger.LogInformation("Registration succeeded for email {Email}. UserId {UserId}", request.Email, user.Id);

        return new RegisterUserResultDto
        {
            Succeeded = true,
            UserId = user.Id.ToString(),
        };
    }
}
