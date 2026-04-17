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

    public PagedResult<AdminUserListItemDto> Users { get; set; } = new();
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

public sealed class AdminUserEditViewModel
{
    [Key]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Ім'я користувача обов'язкове")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Ім'я користувача має бути від 3 до 50 символів")]
    [Display(Name = "Ім'я користувача")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email обов'язковий")]
    [EmailAddress(ErrorMessage = "Некоректний email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Повне ім'я не може перевищувати 100 символів")]
    [Display(Name = "Повне ім'я")]
    public string? FullName { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Дата народження")]
    public DateOnly? DateOfBirth { get; set; }

    [StringLength(20, ErrorMessage = "Стать не може перевищувати 20 символів")]
    [Display(Name = "Стать")]
    public string? Gender { get; set; }

    [Range(20, 300, ErrorMessage = "Вага має бути від 20 до 300 кг")]
    [Display(Name = "Вага (кг)")]
    public decimal? Weight { get; set; }

    [Range(100, 250, ErrorMessage = "Зріст має бути від 100 до 250 см")]
    [Display(Name = "Зріст (см)")]
    public decimal? Height { get; set; }
}
