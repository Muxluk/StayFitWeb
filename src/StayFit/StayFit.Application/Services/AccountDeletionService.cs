using Microsoft.Extensions.Logging;
using StayFit.Application.Common;
using StayFit.Application.Interfaces;

namespace StayFit.Application.Services;

public class AccountDeletionService : IAccountDeletionService
{
    private readonly IAccountDeletionRepository _repository;
    private readonly ILogger<AccountDeletionService> _logger;

    public AccountDeletionService(
        IAccountDeletionRepository repository,
        ILogger<AccountDeletionService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result> DeleteAccountAsync(int userId, string password, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Початок процесу видалення акаунта для UserId={UserId}", userId);

        if (string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Видалення відхилено: пароль не вказано. UserId={UserId}", userId);
            return Result.Failure("Потрібно ввести пароль для підтвердження");
        }

        var isPasswordValid = await _repository.CheckPasswordAsync(userId, password);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Видалення відхилено: невірний пароль. UserId={UserId}", userId);
            return Result.Failure("Невірний пароль");
        }

        var success = await _repository.DeleteUserDataAsync(userId, cancellationToken);
        if (!success)
        {
            _logger.LogError("Помилка при видаленні даних з бази для UserId={UserId}", userId);
            return Result.Failure("Не вдалося видалити акаунт через внутрішню помилку");
        }

        _logger.LogInformation("Акаунт UserId={UserId} та всі пов'язані дані успішно видалено", userId);
        return Result.Success();
    }
}