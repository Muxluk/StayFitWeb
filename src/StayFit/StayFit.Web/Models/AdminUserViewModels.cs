using System.ComponentModel.DataAnnotations;
using StayFit.Application.DTOs;

namespace StayFit.Web.Models;

public sealed class AdminUserSearchViewModel
{
    [Display(Name = "ID користувача")]
    public int? UserId { get; set; }

    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Некоректний email")]
    public string? Email { get; set; }

    public IReadOnlyList<AdminUserListItemDto> Users { get; set; } = Array.Empty<AdminUserListItemDto>();
}

public sealed class AdminResetPasswordViewModel
{
    [Required(ErrorMessage = "Вкажіть новий пароль")]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Пароль має містити щонайменше 8 символів")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Підтвердіть пароль")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Паролі не співпадають")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class AdminUserDetailsViewModel
{
    public AdminUserDetailsDto? User { get; set; }
    public AdminResetPasswordViewModel ResetPassword { get; set; } = new();
}
