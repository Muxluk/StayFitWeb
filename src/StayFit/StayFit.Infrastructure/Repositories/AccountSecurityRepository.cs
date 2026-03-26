using Microsoft.AspNetCore.Identity;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Infrastructure.Data;
using StayFit.Infrastructure.Identity;

namespace StayFit.Infrastructure.Repositories;

/// <summary>
/// Repository для операцій безпеки акаунта
/// </summary>
public class AccountSecurityRepository : IAccountSecurityRepository
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountSecurityRepository(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return false;

        // Перевірити поточний пароль та змінити на новий
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded;
    }

    public async Task<IEnumerable<UserSession>> GetActiveSessionsAsync(int userId)
    {
        // Поки що повертаємо пусто - в реальному проекті це було б з DB
        return await Task.FromResult(Enumerable.Empty<UserSession>());
    }

    public async Task<bool> InvalidateSessionAsync(int sessionId)
    {
        // Поки що - заглушка
        return await Task.FromResult(true);
    }

    public async Task<bool> InvalidateAllSessionsAsync(int userId)
    {
        // Поки що - заглушка
        return await Task.FromResult(true);
    }

    public async Task<bool> DeleteAccountAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> UserExistsAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user != null;
    }
}
