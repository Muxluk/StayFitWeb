namespace StayFit.Application.DTOs;

public sealed class AdminUserSearchRequestDto
{
    public int? UserId { get; init; }
    public string? Email { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
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
    public AdminUserProfileDto? Profile { get; init; }
}

public sealed class AdminUserProfileDto
{
    public string FullName { get; init; } = string.Empty;
    public DateOnly? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public decimal? Weight { get; init; }
    public decimal? Height { get; init; }
}

public sealed class AdminUpdateUserRequestDto
{
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FullName { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public decimal? Weight { get; init; }
    public decimal? Height { get; init; }
}
