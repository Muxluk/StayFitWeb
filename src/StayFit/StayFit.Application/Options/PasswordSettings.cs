namespace StayFit.Application.Options;

/// <summary>
/// Налаштування безпеки паролів
/// </summary>
public class PasswordSettings
{
    /// <summary>
    /// Мінімальна довжина пароля
    /// </summary>
    public int MinLength { get; set; } = 8;

    /// <summary>
    /// Максимальна кількість спроб введення невірного пароля (на майбутнє)
    /// </summary>
    public int MaxFailedAttempts { get; set; } = 5;
}
