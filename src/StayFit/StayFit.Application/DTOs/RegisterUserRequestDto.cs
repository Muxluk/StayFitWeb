using System.ComponentModel.DataAnnotations;

namespace StayFit.Application.DTOs;

public sealed class RegisterUserRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public string? ActivityLevel { get; set; }
}
