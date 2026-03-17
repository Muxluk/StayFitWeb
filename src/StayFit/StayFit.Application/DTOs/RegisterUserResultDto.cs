namespace StayFit.Application.DTOs;

public sealed class RegisterUserResultDto
{
    public bool Succeeded { get; init; }

    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public string? UserId { get; init; }
}
