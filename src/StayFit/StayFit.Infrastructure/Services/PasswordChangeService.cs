using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StayFit.Application.Interfaces;
using StayFit.Application.Options;
using StayFit.Domain.Results;
using StayFit.Infrastructure.Identity;

namespace StayFit.Infrastructure.Services;

public class PasswordChangeService : IPasswordChangeService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly PasswordSettings _passwordSettings;
    private readonly ILogger<PasswordChangeService> _logger;
    private readonly ISecurityLogService? _securityLogService;

    public PasswordChangeService(
        UserManager<ApplicationUser> userManager,
        IOptions<PasswordSettings> passwordSettings,
        ILogger<PasswordChangeService> logger,
        ISecurityLogService? securityLogService = null)
    {
        _userManager = userManager;
        _passwordSettings = passwordSettings.Value;
        _logger = logger;
        _securityLogService = securityLogService;
    }

    public async Task<Result<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword, string confirmPassword)
    {
        _logger.LogInformation("Початок процесу зміни пароля для користувача з ID: {UserId}", userId);

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            _logger.LogWarning("Користувача з ID {UserId} не знайдено", userId);
            return new Result<bool>.Failure("Користувача не знайдено", "USER_NOT_FOUND");
        }

        // Перевірка старого пароля
        var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, currentPassword);
        if (!isCurrentPasswordValid)
        {
            _logger.LogWarning("Користувач {UserId} ввів неправильний поточний пароль", userId);
            return new Result<bool>.Failure("Неправильний поточний пароль", "INVALID_CURRENT_PASSWORD");
        }

        // Валідація нового пароля (ідентичність старому)
        if (currentPassword == newPassword)
        {
            _logger.LogWarning("Для користувача {UserId} новий пароль збігається зі старим", userId);
            return new Result<bool>.Failure("Новий пароль не може бути таким же, як поточний", "SAME_PASSWORD_ERROR");
        }

        // Валідація нового пароля (підтвердження)
        if (newPassword != confirmPassword)
        {
            _logger.LogWarning("Для користувача {UserId} паролі не збігаються", userId);
            return new Result<bool>.Failure("Паролі не збігаються", "PASSWORD_MISMATCH");
        }

        // Валідація довжини нового пароля
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < _passwordSettings.MinLength)
        {
            _logger.LogWarning("Для користувача {UserId} новий пароль не відповідає політиці безпеки (мінімум {MinLength} символів)", userId, _passwordSettings.MinLength);
            return new Result<bool>.Failure($"Пароль повинен містити мінімум {_passwordSettings.MinLength} символів", "PASSWORD_TOO_SHORT");
        }

        // Виконання зміни пароля
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (result.Succeeded)
        {
            _logger.LogInformation("Користувач {UserId} успішно змінив пароль", userId);
            
            // Логування в журнал безпеки
            if (_securityLogService != null)
            {
                await _securityLogService.LogPasswordChangeAsync(userId, isSuccessful: true);
            }
            
            return new Result<bool>.Success(true);
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        _logger.LogError("Не вдалося змінити пароль для користувача {UserId}. Помилки: {Errors}", userId, errors);
        
        // Логування помилки в журнал безпеки
        if (_securityLogService != null)
        {
            await _securityLogService.LogPasswordChangeAsync(userId, isSuccessful: false, failureReason: errors);
        }

        return new Result<bool>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)), "PASSWORD_CHANGE_FAILED");
    }
}
