using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;
using StayFit.Infrastructure.Identity;

namespace StayFit.Infrastructure.Services;

public sealed class RegistrationService(
    UserManager<ApplicationUser> userManager,
    IUserRepository userRepository,
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

        // Create corresponding DomainUser
        try
        {
            var domainUser = new User
            {
                Id = user.Id,
                Name = request.UserName,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
            };

            await userRepository.AddAsync(domainUser);
            logger.LogInformation(
                "Registration succeeded. Identity UserId {UserId}, Domain UserId {DomainUserId}",
                user.Id,
                domainUser.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to create DomainUser for email {Email} after successful Identity registration",
                request.Email);
            throw;
        }

        return new RegisterUserResultDto
        {
            Succeeded = true,
            UserId = user.Id.ToString(),
        };
    }
}
