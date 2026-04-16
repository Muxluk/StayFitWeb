using StayFit.Application.Interfaces;
using StayFit.Domain.Interfaces;

namespace StayFit.Application.Services;

public class ProfileSetupService : IProfileSetupService
{
    private readonly IUserProfileRepository _profileRepository;

    public ProfileSetupService(IUserProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task<bool> IsProfileCompleteAsync(int userId)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);
        if (profile == null) return false;
        
        return !string.IsNullOrWhiteSpace(profile.FullName) &&
               profile.Weight.HasValue &&
               profile.Height.HasValue &&
               profile.DateOfBirth.HasValue;
    }
}
