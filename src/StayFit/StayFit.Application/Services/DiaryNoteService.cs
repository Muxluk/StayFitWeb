using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StayFit.Application.Configuration;
using StayFit.Domain.Interfaces;

namespace StayFit.Application.Services;

public class DiaryNoteService
{
    private readonly IMealRepository _mealRepository;
    private readonly ILogger<DiaryNoteService> _logger;
    private readonly int _maxNoteLength;

    public DiaryNoteService(
        IMealRepository mealRepository,
        ILogger<DiaryNoteService> logger,
        IOptions<DiaryNoteSettings> diaryNoteSettings)
    {
        _mealRepository = mealRepository;
        _logger = logger;
        _maxNoteLength = diaryNoteSettings.Value.MaxNoteLength;
    }

    /// <summary>
    /// Adds or updates a note for a meal entry.
    /// </summary>
    /// <param name="mealId">ID of the meal</param>
    /// <param name="userEmail">Email of the current user</param>
    /// <param name="note">The note text (can be empty to clear)</param>
    /// <returns>True if successful, false otherwise</returns>
    public async Task<bool> UpdateNoteAsync(int mealId, string userEmail, string? note)
    {
        try
        {
            _logger.LogInformation("Updating note for meal {MealId} for user {UserEmail}", mealId, userEmail);

            // Validate input
            if (string.IsNullOrWhiteSpace(note))
            {
                note = null; // Clear the note
            }
            else if (note.Length > _maxNoteLength)
            {
                _logger.LogWarning(
                    "Note for meal {MealId} exceeds maximum length. Length: {NoteLength}, Max: {MaxLength}",
                    mealId, note.Length, _maxNoteLength);
                return false;
            }

            // Get the meal
            var meal = await _mealRepository.GetByIdAsync(mealId);
            if (meal == null || meal.UserEmail != userEmail)
            {
                _logger.LogWarning("Meal {MealId} not found or doesn't belong to user {UserEmail}", 
                    mealId, userEmail);
                return false;
            }

            // Update the note
            meal.Note = note;
            await _mealRepository.UpdateAsync(meal);

            _logger.LogInformation(
                string.IsNullOrEmpty(note) 
                    ? "Note cleared for meal {MealId}" 
                    : "Note updated for meal {MealId} with length {NoteLength}",
                mealId, note?.Length);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating note for meal {MealId} and user {UserEmail}", 
                mealId, userEmail);
            return false;
        }
    }

    /// <summary>
    /// Gets the current note for a meal.
    /// </summary>
    /// <param name="mealId">ID of the meal</param>
    /// <param name="userEmail">Email of the current user</param>
    /// <returns>The note text, or null if not found or doesn't belong to user</returns>
    public async Task<string?> GetNoteAsync(int mealId, string userEmail)
    {
        try
        {
            var meal = await _mealRepository.GetByIdAsync(mealId);
            if (meal == null || meal.UserEmail != userEmail)
            {
                _logger.LogWarning("Meal {MealId} not found or doesn't belong to user {UserEmail}", 
                    mealId, userEmail);
                return null;
            }

            return meal.Note;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting note for meal {MealId}", mealId);
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
