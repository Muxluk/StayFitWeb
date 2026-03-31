namespace StayFit.Application.DTOs;

public sealed class AdminUserSearchRequestDto
{
    public int? UserId { get; init; }
    public string? Email { get; init; }
}

public sealed class AdminUserListItemDto
{
    public int UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool IsLocked { get; init; }
    public DateTimeOffset? LockoutEnd { get; init; }
}

public sealed class AdminUserDetailsDto
{
    public int UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool IsLocked { get; init; }
    public DateTimeOffset? LockoutEnd { get; init; }
    public int AccessFailedCount { get; init; }
}
