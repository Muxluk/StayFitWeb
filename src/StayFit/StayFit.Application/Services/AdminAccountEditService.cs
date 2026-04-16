using Microsoft.Extensions.Logging;
using StayFit.Application.Common;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;

namespace StayFit.Application.Services;

/// <summary>
/// Сервіс для редагування та валідації даних акаунта користувача адміністратором.
/// </summary>
public sealed class AdminAccountEditService(ILogger<AdminAccountEditService> logger) 
    : IAdminAccountEditService
{
    /// <summary>
    /// Валідує дані для редагування акаунта користувача.
    /// </summary>
    public Result ValidateUpdateRequest(AdminUpdateUserRequestDto request, int userId)
    {
        logger.LogInformation("Validating admin account edit for userId={UserId}", userId);

        // Валідація ім'я користувача
        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            logger.LogWarning("Validation failed: UserName is empty for userId={UserId}", userId);
            return Result.Failure("Ім'я користувача обов'язкове");
        }

        if (request.UserName.Length < 3 || request.UserName.Length > 50)
        {
            logger.LogWarning(
                "Validation failed: UserName length invalid for userId={UserId}. Length: {Length}",
                userId,
                request.UserName.Length);
            return Result.Failure("Ім'я користувача має бути від 3 до 50 символів");
        }

        // Валідація email
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            logger.LogWarning("Validation failed: Email is empty for userId={UserId}", userId);
            return Result.Failure("Email обов'язковий");
        }

        if (!IsValidEmail(request.Email))
        {
            logger.LogWarning("Validation failed: Invalid email format for userId={UserId}. Email: {Email}", userId, request.Email);
            return Result.Failure("Некоректний формат email");
        }

        // Валідація полів профілю (якщо надаються)
        if (!string.IsNullOrWhiteSpace(request.FullName) && request.FullName.Length > 100)
        {
            logger.LogWarning(
                "Validation failed: FullName too long for userId={UserId}. Length: {Length}",
                userId,
                request.FullName.Length);
            return Result.Failure("Повне ім'я не може перевищувати 100 символів");
        }

        if (!string.IsNullOrWhiteSpace(request.Gender) && request.Gender.Length > 20)
        {
            logger.LogWarning("Validation failed: Gender too long for userId={UserId}", userId);
            return Result.Failure("Стать не може перевищувати 20 символів");
        }

        // Валідація ваги
        if (request.Weight.HasValue && (request.Weight < 20 || request.Weight > 300))
        {
            logger.LogWarning(
                "Validation failed: Weight out of range for userId={UserId}. Weight: {Weight}",
                userId,
                request.Weight);
            return Result.Failure("Вага має бути від 20 до 300 кг");
        }

        // Валідація зросту
        if (request.Height.HasValue && (request.Height < 100 || request.Height > 250))
        {
            logger.LogWarning(
                "Validation failed: Height out of range for userId={UserId}. Height: {Height}",
                userId,
                request.Height);
            return Result.Failure("Зріст має бути від 100 до 250 см");
        }

        // Валідація дати народження
        if (request.DateOfBirth.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var age = today.Year - request.DateOfBirth.Value.Year;
            
            if (request.DateOfBirth.Value > today.AddYears(-age))
                age--;

            if (age < 13)
            {
                logger.LogWarning(
                    "Validation failed: User too young for userId={UserId}. Age: {Age}",
                    userId,
                    age);
                return Result.Failure("Користувач має бути старшим за 13 років");
            }

            if (age > 120)
            {
                logger.LogWarning("Validation failed: Invalid age for userId={UserId}. Age: {Age}", userId, age);
                return Result.Failure("Некоректна дата народження");
            }
        }

        logger.LogInformation("Admin account edit validation succeeded for userId={UserId}", userId);
        return Result.Success();
    }

    /// <summary>
    /// Перевіряє, чи email змінини та чи він унікальний.
    /// </summary>
    public async Task<bool> IsEmailUniqueAsync(string email, int excludeUserId, IAdminUserRepository repository)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        logger.LogInformation("Checking email uniqueness: Email={Email}, ExcludeUserId={ExcludeUserId}", email, excludeUserId);

        var users = await repository.SearchUsersAsync(userId: null, email: email);
        var emailExists = users.Any(u => u.UserId != excludeUserId);

        if (emailExists)
        {
            logger.LogWarning("Email already exists: Email={Email}", email);
        }

        return !emailExists;
    }

    /// <summary>
    /// Базова валідація email формату.
    /// </summary>
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
