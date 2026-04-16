using System.ComponentModel.DataAnnotations;

namespace StayFit.Application.DTOs;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Введіть поточний пароль")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть новий пароль")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Підтвердіть новий пароль")]
    [Compare(nameof(NewPassword), ErrorMessage = "Паролі не співпадають")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
