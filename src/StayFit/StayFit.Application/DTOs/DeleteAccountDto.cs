using System.ComponentModel.DataAnnotations;

namespace StayFit.Application.DTOs;

public class DeleteAccountDto
{
    [Required(ErrorMessage = "Потрібно ввести пароль для підтвердження видалення")]
    public string Password { get; set; } = string.Empty;
}