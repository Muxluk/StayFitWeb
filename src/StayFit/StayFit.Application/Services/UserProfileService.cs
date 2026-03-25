using Microsoft.Extensions.Logging;
using StayFit.Application.DTOs;
using StayFit.Application.Interfaces;
using StayFit.Domain.Entities;
using StayFit.Domain.Exceptions;
using StayFit.Domain.Interfaces;

namespace StayFit.Application.Services;

/// <summary>
/// Сервіс для управління профілями користувачів з логуванням
/// </summary>
public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _repository;
    private readonly ILogger<UserProfileService> _logger;

    public UserProfileService(IUserProfileRepository repository, ILogger<UserProfileService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<UserProfileDto?> GetProfileAsync(int userId)
    {
        _logger.LogInformation("Отримання профілю користувача з ID {UserId}", userId);

        var profile = await _repository.GetByUserIdAsync(userId);

        if (profile == null)
        {
            _logger.LogWarning("Профіль користувача {UserId} не знайдено", userId);
            return null;
        }

        var dto = MapToDto(profile);
        _logger.LogInformation("Профіль користувача {UserId} успішно отримано", userId);
        return dto;
    }

    public async Task<bool> UpdateProfileAsync(int userId, UpdateUserProfileDto dto)
    {
        _logger.LogInformation("Оновлення профілю користувача {UserId}", userId);

        var profile = await _repository.GetByUserIdAsync(userId);

        if (profile == null)
        {
            _logger.LogWarning("Профіль для оновлення користувача {UserId} не знайдено", userId);
            throw new NotFoundException("Профіль користувача", userId);
        }

        profile.FullName = dto.FullName;
        profile.DateOfBirth = dto.DateOfBirth;
        profile.Gender = dto.Gender;
        profile.Weight = dto.Weight;
        profile.Height = dto.Height;
        profile.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(profile);
        _logger.LogInformation("Профіль користувача {UserId} успішно оновлено", userId);

        return true;
    }

    public async Task<UserProfileDto> CreateProfileAsync(CreateUserProfileDto dto)
    {
        _logger.LogInformation("Створення профілю для користувача {UserId}", dto.UserId);

        var exists = await _repository.ExistsForUserAsync(dto.UserId);

        if (exists)
        {
            _logger.LogWarning("Профіль для користувача {UserId} вже існує", dto.UserId);
            throw new InvalidOperationException($"Профіль для користувача {dto.UserId} вже існує");
        }

        var profile = new UserProfile
        {
            UserId = dto.UserId,
            FullName = dto.FullName,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            Weight = dto.Weight,
            Height = dto.Height,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _repository.AddAsync(profile);
        _logger.LogInformation("Профіль для користувача {UserId} успішно створено", dto.UserId);

        return MapToDto(profile);
    }

    public async Task<bool> DeleteProfileAsync(int userId)
    {
        _logger.LogInformation("Видалення профілю користувача {UserId}", userId);

        var profile = await _repository.GetByUserIdAsync(userId);

        if (profile == null)
        {
            _logger.LogWarning("Профіль користувача {UserId} не знайдено для видалення", userId);
            throw new NotFoundException("Профіль користувача", userId);
        }

        await _repository.DeleteAsync(profile.Id);
        _logger.LogInformation("Профіль користувача {UserId} успішно видалено", userId);

        return true;
    }

    private static UserProfileDto MapToDto(UserProfile profile) =>
        new UserProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FullName = profile.FullName,
            DateOfBirth = profile.DateOfBirth,
            Gender = profile.Gender,
            Weight = profile.Weight,
            Height = profile.Height,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt,
        };
}
