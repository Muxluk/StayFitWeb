using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StayFit.Application.Configuration;
using StayFit.Domain.Entities;
using StayFit.Domain.Interfaces;

namespace StayFit.Application.Services;

public class DiaryNoteService
{
    private readonly IFoodLogRepository _foodLogRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DiaryNoteService> _logger;
    private readonly int _maxNoteLength;

    public DiaryNoteService(
        IFoodLogRepository foodLogRepository,
        IUserRepository userRepository,
        ILogger<DiaryNoteService> logger,
        IOptions<DiaryNoteSettings> diaryNoteSettings)
    {
        _foodLogRepository = foodLogRepository;
        _userRepository = userRepository;
        _logger = logger;
        _maxNoteLength = diaryNoteSettings.Value.MaxNoteLength;
    }

    /// <summary>
    /// Adds or updates a note for a food log entry.
    /// </summary>
    /// <param name="logId">ID of the food log</param>
    /// <param name="userEmail">Email of the current user</param>
    /// <param name="note">The note text (can be empty to clear)</param>
    /// <returns>True if successful, false otherwise</returns>
    public async Task<bool> UpdateNoteAsync(int logId, string userEmail, string? note)
    {
        try
        {
            _logger.LogInformation("Updating note for food log {LogId} for user {UserEmail}", logId, userEmail);

            if (string.IsNullOrWhiteSpace(note))
            {
                note = null;
            }
            else if (note.Length > _maxNoteLength)
            {
                _logger.LogWarning(
                    "Note for log {LogId} exceeds maximum length. Length: {NoteLength}, Max: {MaxLength}",
                    logId, note.Length, _maxNoteLength);
                return false;
            }

            var foodLog = await _foodLogRepository.GetByIdAsync(logId);
            if (foodLog == null)
            {
                _logger.LogWarning("Food log {LogId} not found", logId);
                return false;
            }

            var user = await _userRepository.GetByEmailAsync(userEmail);
            if (user == null || foodLog.UserId != user.Id)
            {
                _logger.LogWarning("Food log {LogId} does not belong to user {UserEmail}", logId, userEmail);
                return false;
            }

            foodLog.Note = note;
            await _foodLogRepository.UpdateAsync(foodLog);

            _logger.LogInformation(
                string.IsNullOrEmpty(note)
                    ? "Note cleared for food log {LogId}"
                    : "Note updated for food log {LogId} with length {NoteLength}",
                logId, note?.Length);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating note for food log {LogId} and user {UserEmail}", logId, userEmail);
            return false;
        }
    }

    public async Task<string?> GetNoteAsync(int logId, string userEmail)
    {
        try
        {
            var foodLog = await _foodLogRepository.GetByIdAsync(logId);
            if (foodLog == null)
            {
                _logger.LogWarning("Food log {LogId} not found", logId);
                return null;
            }

            var user = await _userRepository.GetByEmailAsync(userEmail);
            if (user == null || foodLog.UserId != user.Id)
            {
                _logger.LogWarning("Food log {LogId} does not belong to user {UserEmail}", logId, userEmail);
                return null;
            }

            return foodLog.Note;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting note for food log {LogId}", logId);
            return null;
        }
    }

    /// <summary>
    /// Gets the maximum allowed note length.
    /// </summary>
    public int GetMaxNoteLength() => _maxNoteLength;

    /// <summary>
    /// Validates if a note text is valid (not exceeding maximum length).
    /// </summary>
    public bool IsValidNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
            return true;

        return note.Length <= _maxNoteLength;
    }
}
