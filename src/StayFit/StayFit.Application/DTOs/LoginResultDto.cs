namespace StayFit.Application.DTOs;

public sealed class LoginResultDto
{
    public bool Succeeded { get; init; }
    public bool IsLockedOut { get; init; }
    public string? Error { get; init; }
    public string? UserName { get; init; }

    public static LoginResultDto Success(string userName) =>
        new() { Succeeded = true, UserName = userName };

    public static LoginResultDto Failure(string error) =>
        new() { Succeeded = false, Error = error };

    public static LoginResultDto LockedOut() =>
        new() { Succeeded = false, IsLockedOut = true, Error = "Акаунт заблоковано. Спробуйте пізніше." };
}
