namespace StayFit.Application.Interfaces;

public interface IProfileSetupService
{
    Task<bool> IsProfileCompleteAsync(int userId);
}
