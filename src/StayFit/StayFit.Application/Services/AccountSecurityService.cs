using Microsoft.Extensions.Logging;
using StayFit.Application.Interfaces;
using StayFit.Domain.Results;
using StayFit.Domain.Entities;

namespace StayFit.Application.Services;

/// <summary>
/// Сервіс для операцій безпеки акаунта
/// Використовує Result патерн - без throw, обробка глобально
/// </summary>
public class AccountSecurityService
{
    private readonly IAccountSecurityRepository _repository;
    private readonly ILogger<AccountSecurityService> _logger;

    public AccountSecurityService(
        IAccountSecurityRepository repository,
        ILogger<AccountSecurityService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Змінити пароль
    /// </summary>
    public async Task<Result<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword, string confirmPassword)
    {
        try
        {
            _logger.LogInformation("Запит на змінення пароля для користувача {UserId}", userId);

            // Валідація
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            {
                _logger.LogWarning("Недійсний новий пароль для користувача {UserId}", userId);
                return new Result<bool>.Failure("Пароль повинен мати мінімум 8 символів", "INVALID_PASSWORD");
            }

            if (newPassword == currentPassword)
            {
                _logger.LogWarning("Новий пароль збігається зі старим для користувача {UserId}", userId);
                return new Result<bool>.Failure("Новий пароль не може бути таким же, як старий", "PASSWORD_SAME");
            }

            if (newPassword != confirmPassword)
            {
                _logger.LogWarning("Паролі не співпадають для користувача {UserId}", userId);
                return new Result<bool>.Failure("Паролі не співпадають", "PASSWORD_MISMATCH");
            }

            // Виконання операції
            var success = await _repository.ChangePasswordAsync(userId, currentPassword, newPassword);

            if (success)
            {
                _logger.LogInformation("Пароль успішно змінено для користувача {UserId}", userId);
                return new Result<bool>.Success(true);
            }

            _logger.LogError("Не вдалося змінити пароль для користувача {UserId}", userId);
            return new Result<bool>.Failure("Не вдалося змінити пароль", "PASSWORD_CHANGE_FAILED");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при змінені пароля для користувача {UserId}", userId);
            return new Result<bool>.Failure("Внутрішня помилка сервера", "INTERNAL_ERROR");
        }
    }

    /// <summary>
    /// Отримати активні сесії
    /// </summary>
    public async Task<Result<IEnumerable<UserSession>>> GetActiveSessionsAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Запит на отримання активних сесій для користувача {UserId}", userId);

            var sessions = await _repository.GetActiveSessionsAsync(userId);

            _logger.LogInformation("Отримано {SessionCount} активних сесій для користувача {UserId}",
                sessions.Count(), userId);

            return new Result<IEnumerable<UserSession>>.Success(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні сесій для користувача {UserId}", userId);
            return new Result<IEnumerable<UserSession>>.Failure("Внутрішня помилка сервера", "INTERNAL_ERROR");
        }
    }

    /// <summary>
    /// Вихід з усіх сесій
    /// </summary>
    public async Task<Result<bool>> LogoutAllSessionsAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Запит на вихід з усіх сесій для користувача {UserId}", userId);

            var success = await _repository.InvalidateAllSessionsAsync(userId);

            if (success)
            {
                _logger.LogInformation("Користувач {UserId} вийшов зі всіх сесій", userId);
                return new Result<bool>.Success(true);
            }

            _logger.LogError("Не вдалося завершити всі сесії для користувача {UserId}", userId);
            return new Result<bool>.Failure("Не вдалося завершити сесії", "LOGOUT_FAILED");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при виході з усіх сесій для користувача {UserId}", userId);
            return new Result<bool>.Failure("Внутрішня помилка сервера", "INTERNAL_ERROR");
        }
    }

    /// <summary>
    /// Видалити акаунт
    /// </summary>
    public async Task<Result<bool>> DeleteAccountAsync(int userId, string confirmationToken)
    {
        try
        {
            _logger.LogWarning("Запит на видалення акаунта для користувача {UserId}", userId);

            // Валідація токена (додатково для безпеки)
            if (string.IsNullOrWhiteSpace(confirmationToken))
            {
                _logger.LogWarning("Відсутній токен підтвердження для видалення акаунта користувача {UserId}", userId);
                return new Result<bool>.Failure("Потрібне підтвердження видалення", "MISSING_CONFIRMATION");
            }

            // Перевірити чи користувач існує
            var userExists = await _repository.UserExistsAsync(userId);
            if (!userExists)
            {
                _logger.LogError("Користувач {UserId} не знайдений для видалення", userId);
                return new Result<bool>.Failure("Користувача не знайдено", "USER_NOT_FOUND");
            }

            // Видалити
            var success = await _repository.DeleteAccountAsync(userId);

            if (success)
            {
                _logger.LogWarning("Акаунт видалено для користувача {UserId}", userId);
                return new Result<bool>.Success(true);
            }

            _logger.LogError("Не вдалося видалити акаунт для користувача {UserId}", userId);
            return new Result<bool>.Failure("Не вдалося видалити акаунт", "DELETE_FAILED");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при видаленні акаунта для користувача {UserId}", userId);
            return new Result<bool>.Failure("Внутрішня помилка сервера", "INTERNAL_ERROR");
        }
    }
}
